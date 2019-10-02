namespace ElectronCgi.DotNet
{
    public class PerformRequestChannelMessage : IChannelMessage
    {
        private readonly Request _request;

        public PerformRequestChannelMessage(Request request)
        {
            _request = request;
        }
        public void Send(IChannel channel)
        {
            channel.Write(_request);
        }
    }
}