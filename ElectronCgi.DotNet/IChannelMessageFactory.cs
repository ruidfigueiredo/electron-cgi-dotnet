using System;

namespace ElectronCgi.DotNet
{
    public interface IChannelMessageFactory {
        RequestExecutedChannelMessage CreateResponseMessage(Response response);
        PerformRequestChannelMessage CreateRequestMessage(Request<object> request);
        RequestFailedChannelMessage CreateRequestFailedChannelMessage(Guid requestId, object error);
    }
}