using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class Connection
    {
        private readonly IChannel _channel;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IRequestExecutor _requestExecutor;
        private readonly IChannelMessageFactory _channelMessageFactory;
        private readonly IResponseHandlerExecutor _responseHandlerExecutor;
        private readonly BufferBlock<IChannelMessage> _dispatchMessagesBufferBlock;
        public bool IsLoggingEnabled { get; set; } = false;
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Error;
        public string LogFilePath { get; set; } = "electron-cgi.log";
        private readonly List<IRequestHandler> _requestHandlers = new List<IRequestHandler>();
        private readonly List<IResponseHandler> _responseHandlers = new List<IResponseHandler>();

        public Connection(IChannel channel, IMessageDispatcher messageDispatcher, IRequestExecutor requestExecutor, IResponseHandlerExecutor responseHandlerExecutor, BufferBlock<IChannelMessage> dispatchMessagesBufferBlock, IChannelMessageFactory channelMessageFactory)
        {
            _channel = channel;
            _messageDispatcher = messageDispatcher;
            _requestExecutor = requestExecutor;
            _responseHandlerExecutor = responseHandlerExecutor;
            _dispatchMessagesBufferBlock = dispatchMessagesBufferBlock;
            _channelMessageFactory = channelMessageFactory;
        }

        public void On(string requestType, Action handler)
        {
            On<object>(requestType, _ => handler());
        }

        public void OnAsync<T>(string requestType, Func<T, Task> handler)
        {
            RegisterRequestHandler(new RequestHandler<T>(requestType, handler));
        }

        public void OnAsync(string requestType, Func<Task> handler)
        {
            RegisterRequestHandler(new RequestHandler<object>(requestType, _ => handler()));
        }


        public void On<T>(string requestType, Action<T> handler)
        {
            RegisterRequestHandler(new RequestHandler<T>(requestType, (T args) =>
            {
                handler(args);
                return Task.CompletedTask;
            }));
        }

        public void On<T>(string requestType, Func<T> handler)
        {
            RegisterRequestHandler(new RequestHandler<object, T>(requestType, _ =>
            {   
                //we are deliberately ignoring the request parameter
                return Task.FromResult(handler());
            }));
        }        

        public void On<TIn, TOut>(string requestType, Func<TIn, TOut> handler)
        {
            RegisterRequestHandler(new RequestHandler<TIn, TOut>(requestType, (TIn args) =>
            {
                return Task.FromResult(handler(args));
            }));
        }

        public void OnAsync<TOut>(string requestType, Func<Task<TOut>> handler)
        {
            RegisterRequestHandler(new RequestHandler<object, TOut>(requestType, _ => handler()));
        }

        public void OnAsync<TIn, TOut>(string requestType, Func<TIn, Task<TOut>> handler)
        {
            RegisterRequestHandler(new RequestHandler<TIn, TOut>(requestType, handler));
        }



        private void RegisterRequestHandler(IRequestHandler handler)
        {
            if (_requestHandlers.Any(h => h.RequestType == handler.RequestType))
                throw new DuplicateHandlerForRequestTypeException($"There's already a request handler registered for request type: {handler.RequestType}.");

            _requestHandlers.Add(handler);
        }

        public void Send(string requestType)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = null
            };
            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }

        public void Send(string requestType, Action responseHandler)
        {
            SendAsync(requestType, new Func<Task>(() => { responseHandler(); return Task.CompletedTask; }));
        }

        public void SendAsync(string requestType, Func<Task> responseHandler)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = null
            };

            lock (_responseHandlers) //TODO: (RF) Get rid of the lock
                _responseHandlers.Add(
                    new ResponseHandler(request.Id,
                    responseHandler));

            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }

        public void Send<TRequestArgs>(string requestType, TRequestArgs args)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = args
            };
            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }

        public void Send<TResponseArgs>(string requestType, Action<TResponseArgs> responseHandler)
        {
            SendAsync(requestType, new Func<object, Task>(arg => { responseHandler((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))); return Task.CompletedTask; }));
        }

        public void SendAsync<TResponseArgs>(string requestType, Func<TResponseArgs, Task> responseHandlerAsync)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = null
            };
            lock (_responseHandlers) //TODO: (RF) Get rid of the lock
                _responseHandlers.Add(
                    new ResponseHandler(request.Id, typeof(TResponseArgs),
                        new Func<object, Task>(arg => responseHandlerAsync((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))))));

            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }


        public void Send<TRequestArgs>(string requestType, TRequestArgs args, Action responseHandler)
        {
            SendAsync(requestType, args, new Func<Task>(() => { responseHandler(); return Task.CompletedTask; }));
        }

        public void SendAsync<TRequestArgs>(string requestType, TRequestArgs args, Func<Task> responseHandler)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = args
            };

            lock (_responseHandlers) //TODO: (RF) Get rid of the lock
                _responseHandlers.Add(
                    new ResponseHandler(request.Id,
                    responseHandler));

            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }

        public void Send<TRequestArgs, TResponseArgs>(string requestType, TRequestArgs args, Action<TResponseArgs> responseHandler)
        {
            SendAsync(requestType, args, new Func<object, Task>(arg => { responseHandler((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))); return Task.CompletedTask; }));
        }

        public void SendAsync<TRequestArgs, TResponseArgs>(string requestType, TRequestArgs args, Func<TResponseArgs, Task> responseHandlerAsync)
        {
            var request = new Request<object>
            {
                Type = requestType,
                Args = args
            };

            lock (_responseHandlers) //TODO: (RF) Get rid of the lock
                _responseHandlers.Add(
                    new ResponseHandler(request.Id, typeof(TResponseArgs),
                        new Func<object, Task>(arg => responseHandlerAsync((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))))));

            _dispatchMessagesBufferBlock.Post(_channelMessageFactory.CreateRequestMessage(request));
        }

        public Task<TResponseArgs> SendAsync<TRequestArgs, TResponseArgs>(string requestType, TRequestArgs args) {
            var taskCompletionSource = new TaskCompletionSource<TResponseArgs>();
            Send(requestType, args, (TResponseArgs result) => {
                taskCompletionSource.SetResult(result);                
            });
            return taskCompletionSource.Task;
        }

        /**
         * This will block the executing thread until inputStream is closed
         */
        public void Listen()
        {
            Start(Console.OpenStandardInput(), Console.Out);
        }

        private void InitialiseLogger()
        {
            var loggerConfiguration = new LoggerConfiguration();
            switch (MinimumLogLevel)
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
                    throw new InvalidOperationException("Unknown log level");
            }

            Log.Logger = loggerConfiguration.WriteTo.File(LogFilePath).CreateLogger();
        }

        /**
         * This will block the executing thread until inputStream is closed
         */
        public void Start(Stream inputStream, TextWriter writer)
        {
            var channelClosedCancelationTokenSource = new CancellationTokenSource();
            try
            {
                if (IsLoggingEnabled)
                    InitialiseLogger();

                _channel.Init(inputStream, writer);

                _messageDispatcher.Init(_dispatchMessagesBufferBlock, _channel);
                _requestExecutor.Init(_requestHandlers, _dispatchMessagesBufferBlock);
                _responseHandlerExecutor.Init(_responseHandlers, _dispatchMessagesBufferBlock);

                var messageDispatcherTask = _messageDispatcher.StartAsync(channelClosedCancelationTokenSource.Token);

                Task.Run(() =>
                {
                    while (_channel.IsOpen)
                    {
                        var channelReadResult = _channel.Read();

                        foreach (var request in channelReadResult.Requests)
                        {
                            _requestExecutor.ExecuteAsync(request, channelClosedCancelationTokenSource.Token);
                        }
                        foreach (var response in channelReadResult.Responses)
                        {
                            _responseHandlerExecutor.ExecuteAsync(response, channelClosedCancelationTokenSource.Token);
                        }
                    }
                    channelClosedCancelationTokenSource.Cancel();
                });

                messageDispatcherTask.Wait(); //if anything goes wrong this is where the exceptions come from
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine(ex.Message);
                if (IsLoggingEnabled)
                {
                    var flattenedAggregateException = ex.Flatten();

                    foreach (var exception in flattenedAggregateException.InnerExceptions)
                    {
                        Log.Error(exception, string.Empty);
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
}