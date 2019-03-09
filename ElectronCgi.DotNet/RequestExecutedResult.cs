using System;

namespace ElectronCgi.DotNet
{
    public class RequestExecutedResult
    {
        public bool IsFaulted { get; }
        public Response Response { get; }
        public Exception Exception { get; }

        public RequestExecutedResult(Response response)
        {
            Response = response;
        }

        public RequestExecutedResult(Exception exception)
        {
            IsFaulted = true;
            Exception = exception;
        }
    }
}