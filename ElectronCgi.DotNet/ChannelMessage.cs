namespace ElectronCgi.DotNet
{
    public class ChannelMessage
    {
        public const string REQUEST_MESSAGE = "REQUEST";
        private const string RESPONSE_MESSAGE = "RESPONSE";
        public string Type {get; set;}
        public Request Request { get; set; }
        public Response<string> Response { get; set; }

        public bool IsRequest  => Type == REQUEST_MESSAGE;
        public bool IsResponse  => Type == RESPONSE_MESSAGE;

    }
}