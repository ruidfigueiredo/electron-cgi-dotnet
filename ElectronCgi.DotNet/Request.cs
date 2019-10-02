using System;

namespace ElectronCgi.DotNet
{
    public class Request
    {
        public string Type {get; set;}
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Args { get; set; } = string.Empty;
    }
}