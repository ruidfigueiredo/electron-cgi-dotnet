namespace ElectronCgi.DotNet
{
    public class RequestExecutedChannelMessage : IChannelMessage
    {
        private readonly RequestExecutedResult _requestExecutedResult;
        public RequestExecutedChannelMessage(RequestExecutedResult requestExecutedResult)
        {
            _requestExecutedResult = requestExecutedResult;
        }
        public void Send(IChannel channel)
        {
            if (_requestExecutedResult.IsFaulted)
            {
                throw _requestExecutedResult.Exception;
            }
            channel.Write(_requestExecutedResult.Response);
        }
    }
}