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
        private readonly IChannelMessageFactory _channelMessageFactory;
        private ICollection<IRequestHandler> _handlers;
        private ITargetBlock<IChannelMessage> _target;
        
        public RequestExecutor(ISerialiser serializer, IChannelMessageFactory channelMessageFactory)
        {
            _serialiser = serializer;
            _channelMessageFactory = channelMessageFactory;
        }

        public void Init(ICollection<IRequestHandler> handlers, ITargetBlock<IChannelMessage> target)
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
                    _target.Post(_channelMessageFactory.CreateResponseMessage(response));
                }
                catch (NoRequestHandlerFoundException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    throw;
                }
                catch(HandlerFailedException ex){
                    _target.Post(_channelMessageFactory.CreateRequestFailedChannelMessage(request.Id, ex.Error));
                }
                catch (Exception ex)
                {
                    _target.Post(_channelMessageFactory.CreateRequestFailedChannelMessage(request.Id, ex));
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