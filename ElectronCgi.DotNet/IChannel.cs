using System.IO;

namespace ElectronCgi.DotNet
{
    public interface IChannel 
    {
        bool IsOpen { get; }
        ChannelReadResult Read();
        void Write(string message);
        void Init(Stream inputStream, TextWriter outputWriter);
    }
}