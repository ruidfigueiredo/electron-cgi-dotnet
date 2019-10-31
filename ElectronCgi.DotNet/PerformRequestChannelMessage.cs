namespace ElectronCgi.DotNet
{
    public class PerformRequestChannelMessage : IChannelMessage
    {
        private readonly Request<object> _request;

        public PerformRequestChannelMessage(Request<object> request)
        {
            _request = request;
        }
        public void Send(IChannel channel)
        {
            channel.Write(_request);
        }
    }
}