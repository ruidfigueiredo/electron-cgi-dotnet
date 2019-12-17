using System;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class RequestFailedChannelMessage : IChannelMessage
    {
        private readonly Guid _requestId;
        private readonly string _errorMessage;
        private readonly ISerialiser _serialiser;
        public RequestFailedChannelMessage(ISerialiser serialiser, Guid requestId, string errorMessage)
        {
            _serialiser = serialiser;
            _errorMessage = errorMessage;
            _requestId = requestId;
        }

        public void Send(IChannel channel)
        {
            var serialisedResponse = _serialiser.Serialise(new { Type = "ERROR", RequestId = _requestId, Error = _errorMessage});
            Log.Verbose($"Sending Failed Response: {serialisedResponse}");            
            channel.Write(serialisedResponse);
        }
    }
}