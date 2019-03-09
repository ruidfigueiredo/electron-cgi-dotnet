using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class Connection
    {
        private readonly IChannel _channel;
        private readonly IResponseDispatcher _responseDispatcher;
        private readonly IRequestExecutor _requestExecutor;
        public bool IsLoggingEnabled { get; set; } = true;
        public string LogFilePath { get; set; } = "electroncgi.log";
        private readonly List<IRequestHandler> _handlers = new List<IRequestHandler>();

        public Connection(IChannel channel, IResponseDispatcher responseDispatcher, IRequestExecutor requestExecutor)
        {
            _channel = channel;
            _responseDispatcher = responseDispatcher;
            _requestExecutor = requestExecutor;
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
                Log.Logger = new LoggerConfiguration().WriteTo.File(LogFilePath).CreateLogger();

                var responsesBufferBlock = CreateBufferBlockForExecutedRequests();
                var channelClosedCancelationTokenSource = new CancellationTokenSource();

                _channel.Init(inputStream, writer);

                _responseDispatcher.Init(responsesBufferBlock, _channel);
                _requestExecutor.Init(_handlers, responsesBufferBlock);

                var responseDispatcherTask = _responseDispatcher.StartAsync(channelClosedCancelationTokenSource.Token);

                while (_channel.IsOpen)
                {
                    var channelReadResult = _channel.Read();

                    foreach (var request in channelReadResult.Requests)
                    {
                        _requestExecutor.ExecuteAsync(request, channelClosedCancelationTokenSource.Token);
                    }
                }
                channelClosedCancelationTokenSource.Cancel();

                responseDispatcherTask.Wait();
            }
            catch (AggregateException ex)
            {
                var flattenedAggregateException = ex.Flatten();

                foreach (var exception in flattenedAggregateException.InnerExceptions)
                {
                    Log.Error(ex, string.Empty);
                }
            }
        }

        protected virtual BufferBlock<RequestExecutedResult> CreateBufferBlockForExecutedRequests()
        {
            return new BufferBlock<RequestExecutedResult>();
        }
    }
}