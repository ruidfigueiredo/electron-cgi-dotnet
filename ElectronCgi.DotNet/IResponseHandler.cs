using System;
using System.Threading.Tasks;

namespace ElectronCgi.DotNet
{
    public interface IResponseHandler
    {
        Guid RequestId { get; }

        bool IsArgumentRequiredInHandler { get; }

        Type ResponseArgumentType { get; }

        Task HandleResponseAsync(object argument = null);
    }

    public class ResponseHandler : IResponseHandler
    {
        public Guid RequestId { get; }

        public Type ResponseArgumentType { get; }

        public bool IsArgumentRequiredInHandler => ResponseArgumentType != null;

        private readonly Func<object, Task> _handler = null;
        private readonly Func<Task> _arglessHandler = null;

        public async Task HandleResponseAsync(object argument = null)
        {
            if (IsArgumentRequiredInHandler)
            {
                if (_handler == null)
                    throw new InvalidOperationException($"Response handler for request {RequestId} has no defined handler (invoked with arguments)");
                await _handler(Convert.ChangeType(argument, ResponseArgumentType));
            }
            else
            {
                if (_arglessHandler == null)
                    throw new InvalidOperationException($"Response handler for request {RequestId} has no defined handler (invoked with arguments)");

                await _arglessHandler();
            }
        }

        public ResponseHandler(Guid requestId, Type argumentType, Func<object, Task> handler)
        {
            RequestId = requestId;
            ResponseArgumentType = argumentType;
            _handler = handler;
        }

        public ResponseHandler(Guid requestId, Func<Task> handler)
        {
            RequestId = requestId;
            ResponseArgumentType = null;
            _arglessHandler = handler;
        }

    }
}