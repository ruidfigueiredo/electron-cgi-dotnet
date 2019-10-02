namespace ElectronCgi.DotNet
{
    public class RequestExecutedChannelMessage : IChannelMessage
    {
        public RequestExecutedResult RequestExecutedResult {get; set;}
        public RequestExecutedChannelMessage(RequestExecutedResult requestExecutedResult)
        {
            RequestExecutedResult = requestExecutedResult;
        }
        public void Send(IChannel channel)
        {
            if (RequestExecutedResult.IsFaulted)
            {
                throw RequestExecutedResult.Exception;
            }
            channel.Write(RequestExecutedResult.Response);
        }
    }
}