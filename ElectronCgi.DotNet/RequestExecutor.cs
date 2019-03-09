using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public class RequestExecutor : IRequestExecutor
    {
        private readonly ISerialiser _serialiser;
        private ICollection<IRequestHandler> _handlers;
        private ITargetBlock<RequestExecutedResult> _target;
        public RequestExecutor(ISerialiser serializer)
        {
            _serialiser = serializer;
        }

        public void Init(ICollection<IRequestHandler> handlers, ITargetBlock<RequestExecutedResult> target)
        {
            _handlers = handlers;
            _target = target;
        }


        public Task ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var handler = FindHandler(request.Type);
                    var arguments = _serialiser.DeserialiseArguments(request.Args, handler.ArgumentsType);
                    var response = await handler.HandleRequestAsync(request.Id, arguments);
                    _target.Post(new RequestExecutedResult(response));
                }
                catch (NoRequestHandlerFoundException ex)
                {
                    _target.Post(new RequestExecutedResult(ex));
                }
                catch (Exception ex)
                {
                    _target.Post(new RequestExecutedResult(new HandlerFailedException($"Request handler for request of type '{request.Type}' failed.", ex)));
                }
            }, cancellationToken);
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