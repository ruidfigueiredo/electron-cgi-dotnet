using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class Connection
    {
        private readonly IChannel _channel;
        private readonly ISerialiser _serialiser;

        public bool IsLoggingEnabled { get; set; } = true;
        public string LogFilePath { get; set; } = "electroncgi.log";
        private readonly List<IRequestHandler> _handlers = new List<IRequestHandler>();

        public Connection(IChannel channel, ISerialiser serialiser)
        {
            _channel = channel;
            _serialiser = serialiser;
        }

        public void On(string requestType, Action handler)
        {
            On<object>(requestType, _ => handler());
        }

        public void OnAsync<T>(string requestType, Func<T, Task> handler)
        {
            RegisterRequestHandler(new RequestHandler<T>(requestType, handler));
        }

        public void On<T>(string requestType, Action<T> handler)
        {
            RegisterRequestHandler(new RequestHandler<T>(requestType, (T args) =>
            {
                handler(args);
                return Task.CompletedTask;
            }));
        }

        public void On<TIn, TOut>(string requestType, Func<TIn, TOut> handler)
        {
            RegisterRequestHandler(new RequestHandler<TIn, TOut>(requestType, (TIn args) =>
            {
                return Task.FromResult(handler(args));
            }));
        }

        public void OnAsync<TIn, TOut>(string requestType, Func<TIn, Task<TOut>> handler)
        {
            RegisterRequestHandler(new RequestHandler<TIn, TOut>(requestType, handler));
        }


        private void RegisterRequestHandler(IRequestHandler handler)
        {
            if (_handlers.Any(h => h.RequestType == handler.RequestType))
                throw new DuplicateHandlerForRequestTypeException($"There's already a request handler registered for request type: {handler.RequestType}.");

            _handlers.Add(handler);
        }

        /**
         * This will block the executing thread until inputStream is closed
         */
        public void Listen()
        {
            Start(Console.OpenStandardInput(), Console.Out);
        }

        /**
         * This will block the executing thread until inputStream is closed
         */
        public void Start(Stream inputStream, TextWriter writer)
        {
            try
            {
                StartAsync(inputStream, writer).Wait();
            }
            catch (AggregateException ex)
            {
                var flattenedAggregateException = ex.Flatten();

                if (flattenedAggregateException.InnerExceptions.Count == 1)
                    throw flattenedAggregateException.InnerException;
                else
                {
                    throw;
                }
            }
        }
        private async Task StartAsync(Stream inputStream, TextWriter writer)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File(LogFilePath).CreateLogger();

            try
            {
                _channel.Init(inputStream, writer);

                while (_channel.IsOpen)
                {
                    var channelReadResult = _channel.Read();
                    if (!channelReadResult.IsIdle)
                    {
                        foreach (var request in channelReadResult.Requests)
                            await HandleRequestAsync(request);
                    }
                    else
                        BackOffForAWhile();
                }
            }
            catch (Exception ex)
            {
                if (IsLoggingEnabled)
                    Log.Error(ex, "");
                throw;
            }
        }

        private async Task HandleRequestAsync(Request request)
        {
            var handler = FindHandler(request.Type);
            try
            {
                var arguments = _serialiser.DeserialiseArguments(request.Args, handler.ArgumentsType);
                var response = await handler.HandleRequestAsync(request.Id, arguments);
                _channel.Write(response);
            }
            catch (SerialiserException ex)
            {
                throw new InvalidArgumentsFormatException($"The received arguments for request with Id: {request.Id} and type: '{request.Type}' are invalid. Expected arguments of type {handler.ArgumentsType} but received: {request.Args}", ex);
            }
            catch (Exception ex)
            {
                throw new HandlerFailedException($"Request handler for request of type '{request.Type}' failed.", ex);
            }
        }

        protected virtual void BackOffForAWhile()
        {
            Thread.Sleep(10);
        }


        private IRequestHandler FindHandler(string requestType)
        {
            var handler = _handlers.SingleOrDefault(h => h.RequestType == requestType);
            if (handler == null)
                throw new NoRequestHandlerFoundException($"No request handler found for request type: {requestType}");
            return handler;
        }
    }
}