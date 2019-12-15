using System;

namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class HandlerFailedException : System.Exception
    {
        public Guid RequestId { get; set; }
        public HandlerFailedException(Guid requestId) { }
        public HandlerFailedException(Guid requestId, string message) : base(message) { }
        public HandlerFailedException(Guid requestId, string message, System.Exception inner) : base(message, inner) { }
        protected HandlerFailedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}