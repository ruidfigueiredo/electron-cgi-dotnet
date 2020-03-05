using System;
using Newtonsoft.Json;

namespace ElectronCgi.DotNet 
{
    public interface ISerialiser 
    {
        ChannelMessage DeserializeMessage(string message);
        Request DeserialiseRequest(string serialiserRequest);        
        string Serialise(object obj);
        object DeserialiseArguments(string args, Type argumentsType);
    }


}