using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ElectronCgi.DotNet
{
    public class ConnectionBuilder
    {
        private string _logFilePath;
        private LogLevel _minimumLogLevel;
        private bool _isLoggingEnabled = false;

        private Microsoft.Extensions.Logging.ILogger _logger;
        public ConnectionBuilder WithLogging(string logFilePath = "electron-cgi.log", LogLevel minimumLogLevel = LogLevel.Debug)
        {
            _isLoggingEnabled = true;
            _logFilePath = logFilePath;
            _minimumLogLevel = minimumLogLevel;
            return this;
        }
        public ConnectionBuilder UsingEncoding(Encoding encoding) {
            Console.OutputEncoding = encoding;
            return this;
        }

        public ConnectionBuilder WithLogging(Microsoft.Extensions.Logging.ILogger logger)
        {
            _isLoggingEnabled = true;
            _logger = logger;
            return this;
        }

        public Connection Build()
        {
            var serialiser = new JsonSerialiser();
            var channelMessageFactory = new ChannelMessageFactory(serialiser);
            var connection = new Connection(
                    new Channel(new TabSeparatedInputStreamParser(), serialiser),
                    new MessageDispatcher(),
                    new RequestExecutor(serialiser, channelMessageFactory),
                    new ResponseHandlerExecutor(serialiser),
                    new System.Threading.Tasks.Dataflow.BufferBlock<IChannelMessage>(),
                    channelMessageFactory);
            if (_isLoggingEnabled)            
            {
                if (_logger == null)
                {
                    Logging.Log.InitializeDefaultLogger(_minimumLogLevel, _logFilePath);                    
                }
                else
                {
                    Logging.Log.UseCustomLogger(_logger);
                }
            }
            return connection;
        }

    }
}
