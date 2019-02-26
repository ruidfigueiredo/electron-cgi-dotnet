using System.Runtime.CompilerServices;

namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class HandlerFailedException : System.Exception
    {
        public HandlerFailedException() { }
        public HandlerFailedException(string message) : base(message) { }
        public HandlerFailedException(string message, System.Exception inner) : base(message, inner) { }
        protected HandlerFailedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}