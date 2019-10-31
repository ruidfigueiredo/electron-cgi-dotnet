using System;

namespace ElectronCgi.DotNet
{
    public class Request<T>
    {
        public string Type {get; set;}
        public Guid Id { get; set; } = Guid.NewGuid();
        public T Args { get; set; } = default(T);
    }

    public class Request : Request<string> {} //default is Request<string>
}