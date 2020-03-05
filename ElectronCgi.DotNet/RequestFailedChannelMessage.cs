using System;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class RequestFailedChannelMessage : IChannelMessage
    {
        private readonly Guid _requestId;
        private readonly object _error;
        private readonly ISerialiser _serialiser;
        public RequestFailedChannelMessage(ISerialiser serialiser, Guid requestId, object error)
        {
            _serialiser = serialiser;
            _error = error;
            _requestId = requestId;
        }

        public void Send(IChannel channel)
        {
            var serialisedResponse = _serialiser.Serialise(new { Type = "ERROR", RequestId = _requestId, Error = _serialiser.Serialise(_error)});
            Log.Verbose($"Sending Failed Response: {serialisedResponse}");            
            channel.Write(serialisedResponse);
        }
    }
}