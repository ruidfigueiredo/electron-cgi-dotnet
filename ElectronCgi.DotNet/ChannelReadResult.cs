using System.Collections.Generic;
using System.Linq;

namespace ElectronCgi.DotNet
{
    public class ChannelReadResult
    {
        public bool IsIdle { get; set; }
        public IEnumerable<Request> Requests { get; set; } = Enumerable.Empty<Request>();
        public IEnumerable<Response<string>> Responses { get; set; } = Enumerable.Empty<Response<string>>();
    }
}