using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public interface IMessageDispatcher
    {
        void Init(ISourceBlock<IChannelMessage> source, IChannel channel);
        Task StartAsync(CancellationToken cancellationToken);
    }
}