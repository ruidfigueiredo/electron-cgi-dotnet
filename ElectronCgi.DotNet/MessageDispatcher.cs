using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private ISourceBlock<IChannelMessage> _source;
        private IChannel _channel;
        public void Init(ISourceBlock<IChannelMessage> source, IChannel channel)
        {
            _source = source;
            _channel = channel;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (await _source.OutputAvailableAsync(cancellationToken))
                    {
                        var channelMessage = _source.Receive();
                        channelMessage.Send(_channel);
                    }
                }
                catch (OperationCanceledException)
                {
                    //all good, time to stop
                }
            });
        }
    }
}