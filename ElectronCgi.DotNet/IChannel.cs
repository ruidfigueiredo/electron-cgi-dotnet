using System.IO;

namespace ElectronCgi.DotNet
{
    public interface IChannel 
    {
        bool IsOpen { get; }
        ChannelReadResult Read();
        void Write(Response response);
        void Write(Request<object> response);
        void Init(Stream inputStream, TextWriter outputWriter);
    }
}