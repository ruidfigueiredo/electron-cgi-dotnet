using System;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ElectronCgi.DotNet.Logging
{
    internal static class Log
    {
        private static bool _isEnabled = false;
        public static bool IsLoggingEnabled
        {
            get
            {
                return _isEnabled;
            }
        }
        private static Microsoft.Extensions.Logging.ILogger _logger = null;

        public static void UseCustomLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            if (_logger != null)
            {
                throw new InvalidOperationException("Custom logger initialized more than once");
            }
            _isEnabled = true;
            _logger = logger;
        }

        public static void InitializeDefaultLogger(LogLevel minimumLogLevel, string logFilePath)
        {
            if (_logger != null)
                throw new InvalidOperationException("Trying to setup the default logger when a custom logger was already initialized");

            _isEnabled = true;

            var loggerConfiguration = new LoggerConfiguration();
            switch (minimumLogLevel)
            {
                case LogLevel.Trace:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Verbose();
                    break;
                case LogLevel.Information:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Information();
                    break;
                case LogLevel.Debug:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Debug();
                    break;
                case LogLevel.Warning:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Warning();
                    break;
                case LogLevel.Critical:
                case LogLevel.Error:
                    loggerConfiguration = loggerConfiguration.MinimumLevel.Error();
                    break;
                case LogLevel.None:
                    return;
                default:
                    throw new InvalidOperationException("Failed to initialized default logger (serilog), unknown log level");
            }

            Serilog.Log.Logger = loggerConfiguration.WriteTo.File(logFilePath).CreateLogger();
        }


        public static void Error(string message)
        {
            if (!_isEnabled) return;

            if (_logger == null)
            {
                Serilog.Log.Error(message);
            }
            else
            {
                _logger.LogError(message);
            }
        }

        public static void Error(string message, Exception ex)
        {
            if (!_isEnabled) return;
            if (_logger == null)
            {
                Serilog.Log.Error(message, ex);
            }
            else
            {
                _logger.LogError(message, ex);
            }
        }

        public static void Error(Exception ex, string messageTemplate)
        {
            if (!_isEnabled) return;
            if (_logger == null)
            {
                Serilog.Log.Error(ex, messageTemplate);
            }
            else
            {
                _logger.LogError(ex, messageTemplate);
            }
        }

        public static void Verbose(string message)
        {
            if (!_isEnabled) return;
            if (_logger == null)
            {
                Serilog.Log.Verbose(message);
            }
            else
            {
                _logger.LogDebug(message);
            }
        }
    }
}