using System;

namespace ElectronCgi.DotNet
{
    [System.Serializable]
    public class HandlerFailedException : System.Exception
    {
        public object Error { get; set; }
        public HandlerFailedException(object error) { Error = error; }
    }
}