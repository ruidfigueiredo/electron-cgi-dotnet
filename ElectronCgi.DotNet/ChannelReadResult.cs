using System.Collections.Generic;
using System.Linq;

namespace ElectronCgi.DotNet
{
    public class ChannelReadResult
    {
        public static ChannelReadResult Empty = new ChannelReadResult();
        public IEnumerable<Request> Requests { get; set; } = Enumerable.Empty<Request>();
        public IEnumerable<Response<string>> Responses { get; set; } = Enumerable.Empty<Response<string>>();
    }
}