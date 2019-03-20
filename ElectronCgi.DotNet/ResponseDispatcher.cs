using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public class ResponseDispatcher : IResponseDispatcher
    {
        private ISourceBlock<RequestExecutedResult> _source;
        private IChannel _channel;
        public void Init(ISourceBlock<RequestExecutedResult> source, IChannel channel)
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
                        var requestExecutedResult = _source.Receive();
                        if (requestExecutedResult.IsFaulted)
                        {
                            throw requestExecutedResult.Exception;
                        }
                        _channel.Write(requestExecutedResult.Response);
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