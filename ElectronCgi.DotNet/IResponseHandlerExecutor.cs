using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ElectronCgi.DotNet
{
    public interface IResponseHandlerExecutor
    {
        void Init(ICollection<IResponseHandler> handlers, ITargetBlock<IChannelMessage> target);
        Task ExecuteAsync(Response<string> response, CancellationToken cancellationToken);
    }


}