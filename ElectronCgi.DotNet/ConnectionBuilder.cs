using Microsoft.Extensions.Logging;

namespace ElectronCgi.DotNet
{
    public class ConnectionBuilder
    {
        private string _logFilePath;
        private LogLevel _minimumLogLevel;
        private bool _isLoggingEnabled = false;
        public ConnectionBuilder WithLogging(string logFilePath = "electron-cgi.log", LogLevel minimumLogLevel = LogLevel.Debug)
        {
            _isLoggingEnabled = true;
            _logFilePath = logFilePath;
            _minimumLogLevel = minimumLogLevel;
            return this;
        }

        public Connection Build()
        {
            var connection = new Connection(
                    new Channel(new TabSeparatedInputStreamParser(), new JsonSerialiser()),                     
                    new MessageDispatcher(), 
                    new RequestExecutor(new JsonSerialiser()),
                    new ResponseHandlerExecutor(new JsonSerialiser()),
                    new System.Threading.Tasks.Dataflow.BufferBlock<IChannelMessage>());
            connection.LogFilePath = _logFilePath;
            connection.MinimumLogLevel = _minimumLogLevel;
            connection.IsLoggingEnabled = _isLoggingEnabled;
            return connection;
        }
    }
}
