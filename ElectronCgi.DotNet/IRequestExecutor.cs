using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public interface IRequestExecutor
    {
        void Init(ICollection<IRequestHandler> handlers, ITargetBlock<IChannelMessage> target);
        Task ExecuteAsync(Request request, CancellationToken cancellationToken);
    }
}