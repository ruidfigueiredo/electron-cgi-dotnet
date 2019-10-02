using System;

namespace ElectronCgi.DotNet
{
    public interface IResponseHandler
    {
        Guid RequestId { get; }

        Type ResponseArgumentType { get; }

        void HandleResponseAsync(object argument);
    }

    public class ResponseHandler : IResponseHandler
    {
        public Guid RequestId {get;}

        public Type ResponseArgumentType {get;}
        private readonly Action<object> _handler;

        public void HandleResponseAsync(object argument)
        {
            _handler(Convert.ChangeType(argument, ResponseArgumentType));
        }

        public ResponseHandler(Guid requestId, Type argumentType, Action<object> handler)
        {
            RequestId = requestId;
            ResponseArgumentType = argumentType;
            _handler = handler;
        }
    }
}