namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class InvalidArgumentsFormatException : System.Exception
    {
        public InvalidArgumentsFormatException() { }
        public InvalidArgumentsFormatException(string message) : base(message) { }
        public InvalidArgumentsFormatException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidArgumentsFormatException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}