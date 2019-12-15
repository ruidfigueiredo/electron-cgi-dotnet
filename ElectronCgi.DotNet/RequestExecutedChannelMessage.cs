using System;
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
            var serialisedResponse = _serialiser.SerialiseResponse(_response);
            Log.Verbose($"Sending Response: {serialisedResponse}");            
            channel.Write(serialisedResponse);
        }
    }
}