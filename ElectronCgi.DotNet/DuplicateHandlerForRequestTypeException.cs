using System.Runtime.CompilerServices;

namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class DuplicateHandlerForRequestTypeException : System.Exception
    {
        public DuplicateHandlerForRequestTypeException() { }
        public DuplicateHandlerForRequestTypeException(string message) : base(message) { }
        public DuplicateHandlerForRequestTypeException(string message, System.Exception inner) : base(message, inner) { }
        protected DuplicateHandlerForRequestTypeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}