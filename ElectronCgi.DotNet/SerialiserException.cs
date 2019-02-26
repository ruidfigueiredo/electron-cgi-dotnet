namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class SerialiserException : System.Exception
    {
        public SerialiserException() { }
        public SerialiserException(string message) : base(message) { }
        public SerialiserException(string message, System.Exception inner) : base(message, inner) { }
        protected SerialiserException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}