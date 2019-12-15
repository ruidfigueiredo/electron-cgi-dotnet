using Serilog;

namespace ElectronCgi.DotNet
{
    public class PerformRequestChannelMessage : IChannelMessage
    {
        private readonly ISerialiser _serialiser;
        private readonly Request<object> _request;

        public PerformRequestChannelMessage(ISerialiser serialiser, Request<object> request)
        {
            _serialiser = serialiser;
            _request = request;
        }
        public void Send(IChannel channel)
        {
            var serialisedRequest = _serialiser.SerialiseRequest(_request);
            Log.Verbose($"stdout: {serialisedRequest}");            
            channel.Write(serialisedRequest);
        }
    }
}