using System;
using System.IO;

namespace ElectronCgi.DotNet
{
    public interface IChannel 
    {
        bool IsOpen { get; }
        ChannelReadResult Read();
        void Write(Response response);
        void Init(Stream inputStream, TextWriter outputWriter);
    }
}