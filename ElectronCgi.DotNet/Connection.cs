using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class Connection
    {
        private readonly IChannel _channel;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IRequestExecutor _requestExecutor;
        private readonly BufferBlock<IChannelMessage> _dispatchMessagesBufferBlock;
        private readonly ISerialiser _serializer;
        public bool IsLoggingEnabled { get; set; } = true;
        public string LogFilePath { get; set; } = "electroncgi.log";
        private readonly List<IRequestHandler> _requestHandlers = new List<IRequestHandler>();
        private readonly List<IResponseHandler> _responseHandlers = new List<IResponseHandler>();



        public Connection(IChannel channel, IMessageDispatcher messageDispatcher, IRequestExecutor requestExecutor, ISerialiser serialiser, BufferBlock<IChannelMessage> dispatchMessagesBufferBlock)
        {
            _channel = channel;
            _messageDispatcher = messageDispatcher;
            _requestExecutor = requestExecutor;
            _serializer = serialiser;
            _dispatchMessagesBufferBlock = dispatchMessagesBufferBlock;
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
            if (_requestHandlers.Any(h => h.RequestType == handler.RequestType))
                throw new DuplicateHandlerForRequestTypeException($"There's already a request handler registered for request type: {handler.RequestType}.");

            _requestHandlers.Add(handler);
        }

        public void Send(string requestType)
        {
            var request = new Request
            {
                Type = requestType,
                Args = null
            };
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void Send(string requestType, Action responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = null
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id,
                new Func<Task>(() => { responseHandler(); return Task.CompletedTask; })));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void SendAsync(string requestType, Func<Task> responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = null
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id,
                responseHandler));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void Send<TRequestArgs>(string requestType, TRequestArgs args)
        {
            var request = new Request
            {
                Type = requestType,
                Args = _serializer.SerializeArguments(args)
            };
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void Send<TResponseArgs>(string requestType, Action<TResponseArgs> responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = null
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id, typeof(TResponseArgs),
                new Func<object, Task>(arg => { responseHandler((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))); return Task.CompletedTask; })));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void SendAsync<TResponseArgs>(string requestType, Func<TResponseArgs, Task> responseHandlerAsync)
        {
            var request = new Request
            {
                Type = requestType,
                Args = null
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id, typeof(TResponseArgs),
                    new Func<object, Task>(arg => responseHandlerAsync((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))))));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }


        public void Send<TRequestArgs>(string requestType, TRequestArgs args, Action responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = _serializer.SerializeArguments(args)
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id,
                new Func<Task>(() => { responseHandler(); return Task.CompletedTask; })));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void SendAsync<TRequestArgs>(string requestType, TRequestArgs args, Func<Task> responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = _serializer.SerializeArguments(args)
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id,
                responseHandler));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }        

        public void Send<TRequestArgs, TResponseArgs>(string requestType, TRequestArgs args, Action<TResponseArgs> responseHandler)
        {
            var request = new Request
            {
                Type = requestType,
                Args = _serializer.SerializeArguments(args)
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id, typeof(TResponseArgs),
                new Func<object, Task>(arg => { responseHandler((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))); return Task.CompletedTask; })));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
        }

        public void SendAsync<TRequestArgs, TResponseArgs>(string requestType, TRequestArgs args, Func<TResponseArgs, Task> responseHandlerAsync)
        {
            var request = new Request
            {
                Type = requestType,
                Args = _serializer.SerializeArguments(args)
            };
            _responseHandlers.Add(
                new ResponseHandler(request.Id, typeof(TResponseArgs),
                    new Func<object, Task>(arg => responseHandlerAsync((TResponseArgs)Convert.ChangeType(arg, typeof(TResponseArgs))))));
            Log.Debug($"Sending request form .net with id {request.Id} and type {request.Type}");
            _dispatchMessagesBufferBlock.Post(new PerformRequestChannelMessage(request));
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
            var channelClosedCancelationTokenSource = new CancellationTokenSource();
            try
            {
                if (IsLoggingEnabled)
                    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(LogFilePath).CreateLogger();


                _channel.Init(inputStream, writer);

                _messageDispatcher.Init(_dispatchMessagesBufferBlock, _channel);
                _requestExecutor.Init(_requestHandlers, _dispatchMessagesBufferBlock);

                var responseDispatcherTask = _messageDispatcher.StartAsync(channelClosedCancelationTokenSource.Token);

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
                            Log.Debug($"Received response with id {response.Id} with {response.Result}");
                            //TODO: (RF) move this somewhere more appropriate
                            var registeredResponseHandler = _responseHandlers.SingleOrDefault(r => r.RequestId == response.Id);
                            if (registeredResponseHandler != null)
                            {
                                if (registeredResponseHandler.IsArgumentRequiredInHandler)
                                {
                                    var args = _serializer.DeserialiseArguments(response.Result, registeredResponseHandler.ResponseArgumentType);
                                    registeredResponseHandler.HandleResponseAsync(args).Wait();
                                }
                                else
                                {
                                    registeredResponseHandler.HandleResponseAsync().Wait();
                                }
                                _responseHandlers.Remove(registeredResponseHandler);
                            }
                        }
                    }
                    channelClosedCancelationTokenSource.Cancel();
                });

                responseDispatcherTask.Wait(); //if anything goes wrong this is where the exceptions come from
            }
            catch (AggregateException ex)
            {
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

        protected virtual BufferBlock<IChannelMessage> CreateBufferBlockForDispatchingMessages()
        {
            throw new InvalidOperationException();
        }
    }
}