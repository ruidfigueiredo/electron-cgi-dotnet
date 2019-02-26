using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog;

namespace ElectronCgi.DotNet
{

    public class TabSeparatedInputStreamParser : IInputStreamParser
    {
        private string _input = string.Empty;
        private List<string> _completedFrames = new List<string>();

        public bool HasCompletedFrames => _completedFrames.Count > 0;

        public IEnumerable<string> GetCompletedFrames()
        {
            return new List<string>(_completedFrames);
        }

        public void ClearCompletedFrames()
        {
            _completedFrames.Clear();
        }

        public void AddPartial(string partialStreamContent)
        {
            _input += partialStreamContent;
            while (_input.IndexOf("\t") != -1)
            {
                var frame = _input.Substring(0, _input.IndexOf("\t"));                                
                _completedFrames.Add(frame);
                _input = _input.Substring(_input.IndexOf("\t") + 1);
            }            
        }
    }
}