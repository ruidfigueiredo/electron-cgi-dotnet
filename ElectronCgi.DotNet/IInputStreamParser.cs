using System;
using System.Collections.Generic;

namespace ElectronCgi.DotNet 
{
    public interface IInputStreamParser 
    {
        bool HasCompletedFrames { get; }
        IEnumerable<string> GetCompletedFrames();
        void ClearCompletedFrames();
        void AddPartial(string partialContents);
    }
}