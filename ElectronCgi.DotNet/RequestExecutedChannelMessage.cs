using Serilog;

namespace ElectronCgi.DotNet
{
    public class RequestExecutedChannelMessage : IChannelMessage
    {
        private readonly Response _response;
        private readonly ISerialiser _serialiser;
        public RequestExecutedChannelMessage(ISerialiser serialiser, Response response)
        {
            _response = response;
            _serialiser = serialiser;
        }
        
        public void Send(IChannel channel)
        {
            var serialisedResponse = _serialiser.Serialise(new { Type = "RESPONSE", Response = _response });            
            channel.Write(serialisedResponse);
        }
    }
}