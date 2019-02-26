using System;
using System.Threading.Tasks;

namespace ElectronCgi.DotNet
{
    internal interface IRequestHandler
    {
        string RequestType { get; }
        Type ArgumentsType { get; }
        Task<Response> HandleRequestAsync(Guid requestId, object arguments);
    }
}