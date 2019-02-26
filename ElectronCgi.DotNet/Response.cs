using System;

namespace ElectronCgi.DotNet
{
    public class Response
    {
        public Guid Id { get; set; }
    }

    public class Response<T> : Response
    {
        public T Result { get; set; }
    }    
}