using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ElectronCgi.DotNet.Logging;

namespace ElectronCgi.DotNet
{
    public class ResponseHandlerExecutor : IResponseHandlerExecutor
    {
        private readonly ISerialiser _serialiser;
        private ICollection<IResponseHandler> _responseHandlers = null;
        private ITargetBlock<IChannelMessage> _messageDispatcherBlock = null;
        public ResponseHandlerExecutor(ISerialiser serialiser)
        {
            _serialiser = serialiser;
        }
        public Task ExecuteAsync(Response<string> response, CancellationToken cancellationToken)
        {
            if (_responseHandlers == null || _messageDispatcherBlock == null)
            {
                throw new InvalidOperationException("ResponseHandlerExecutor invoked before being initialised");
            }

            return Task.Run(async () =>
            {
                IResponseHandler registeredResponseHandler = null;
                lock (_responseHandlers) //TODO: (RF) Get rid of this (maybe use TPL Dataflow)
                {
                    registeredResponseHandler = _responseHandlers.SingleOrDefault(r => r.RequestId == response.Id);
                }

                if (registeredResponseHandler != null)
                {
                    try
                    {
                        if (registeredResponseHandler.IsArgumentRequiredInHandler)
                        {
                            var args = _serialiser.DeserialiseArguments(response.Result, registeredResponseHandler.ResponseArgumentType);
                            await registeredResponseHandler.HandleResponseAsync(args);
                        }
                        else
                        {
                            await registeredResponseHandler.HandleResponseAsync();
                        }
                    }
                    catch (Exception ex)
                    {                        
                        Log.Error($"Failed to run response handler for request with id: {registeredResponseHandler.RequestId}\n{{0}}", ex);
                    }
                    lock (_responseHandlers)
                    {
                        _responseHandlers.Remove(registeredResponseHandler);
                    }
                }
            }, cancellationToken);
        }

        public void Init(ICollection<IResponseHandler> handlers, ITargetBlock<IChannelMessage> messageDispatcherBlock)
        {
            _responseHandlers = handlers;
            _messageDispatcherBlock = messageDispatcherBlock;
        }
    }
}