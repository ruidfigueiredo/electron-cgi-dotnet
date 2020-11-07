using System;
using System.Threading.Tasks;

namespace ElectronCgi.DotNet
{
    public class RequestHandler<T> : IRequestHandler
    {
        public string RequestType { get; }
        public Type ArgumentsType { get; }
        private readonly Func<T, Task> _handler;

        public RequestHandler(string requestType, Func<T, Task> handler)
        {
            RequestType = requestType;
            _handler = handler;
            ArgumentsType = typeof(T);
        }
        public async Task<Response> HandleRequestAsync(Guid requestId, object arguments)
        {
            await _handler((T)arguments);
            return new Response
            {
                Id = requestId
            };
        }
    }

    public class RequestHandler<TIn, TOut> : IRequestHandler
    {
        public string RequestType { get; }
        public Type ArgumentsType {get;}

        private readonly Func<TIn, Task<TOut>> _handler;

        public RequestHandler(string requestType, Func<TIn, Task<TOut>> handler)
        {
            RequestType = requestType;
            _handler = handler;
            ArgumentsType = typeof(TIn);
        }
        public async Task<Response> HandleRequestAsync(Guid id, object arguments)
        {
            var result = await _handler((TIn)arguments);
            return new Response<TOut>
            {
                Id = id,
                Result = result
            };
        }
    }
}