using System;

namespace ElectronCgi.DotNet
{
    public class ChannelMessageFactory : IChannelMessageFactory
    {
        private readonly ISerialiser _serialiser;
        public ChannelMessageFactory(ISerialiser serialiser)
        {
            _serialiser = serialiser;
        }
        public PerformRequestChannelMessage CreateRequestMessage(Request<object> request)
        {
            return new PerformRequestChannelMessage(_serialiser, request);
        }

        public RequestExecutedChannelMessage CreateResponseMessage(Response response)
        {
            return new RequestExecutedChannelMessage(_serialiser, response);
        }

        public RequestFailedChannelMessage CreateRequestFailedChannelMessage(Guid requestId, string errorMessage)
        {
            return new RequestFailedChannelMessage(_serialiser, requestId, errorMessage);
        }
    }
}