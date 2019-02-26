using System;

namespace ElectronCgi.DotNet
{
    public class Request
    {
        public string Type {get; set;}
        public Guid Id { get; set; }
        public string Args { get; set; } = string.Empty;
    }
}