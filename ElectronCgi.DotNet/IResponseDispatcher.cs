using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public interface IResponseDispatcher
    {
        void Init(ISourceBlock<RequestExecutedResult> source, IChannel channel);
        Task StartAsync(CancellationToken cancellationToken);
    }
}