using System;
using System.Threading.Tasks;

namespace ElectronCgi.DotNet
{
    public interface IResponseHandler
    {
        Guid RequestId { get; }

        Type ResponseArgumentType { get; }

        Task HandleResponseAsync(object argument);
    }

    public class ResponseHandler : IResponseHandler
    {
        public Guid RequestId {get;}

        public Type ResponseArgumentType {get;}
        private readonly Func<object, Task> _handler;

        public async Task HandleResponseAsync(object argument)
        {
            await _handler(Convert.ChangeType(argument, ResponseArgumentType));
        }

        public ResponseHandler(Guid requestId, Type argumentType, Func<object, Task> handler)
        {
            RequestId = requestId;
            ResponseArgumentType = argumentType;
            _handler = handler;
        }
    }
}