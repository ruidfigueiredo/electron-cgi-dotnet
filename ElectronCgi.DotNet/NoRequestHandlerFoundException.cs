using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class NoRequestHandlerFoundException : Exception
    {
        public NoRequestHandlerFoundException()
        {
        }

        public NoRequestHandlerFoundException(string message) : base(message)
        {
        }

        public NoRequestHandlerFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoRequestHandlerFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}