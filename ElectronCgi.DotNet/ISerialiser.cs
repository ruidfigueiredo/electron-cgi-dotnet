using System;
using Newtonsoft.Json;

namespace ElectronCgi.DotNet 
{
    public interface ISerialiser 
    {
        ChannelMessage DeserializeMessage(string message);
        Request DeserialiseRequest(string serialiserRequest);
        string SerializeArguments(object arguments);
        string SerialiseResponse(Response response);
        string SerialiseRequest(Request<object> request);
        object DeserialiseArguments(string args, Type argumentsType);
    }


}