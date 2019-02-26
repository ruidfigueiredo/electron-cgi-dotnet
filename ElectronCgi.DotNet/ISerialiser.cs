using System;
using Newtonsoft.Json;

namespace ElectronCgi.DotNet 
{
    public interface ISerialiser 
    {
        Request DeserialiseRequest(string serialiserRequest);
        string SerialiseResponse(Response response);
        object DeserialiseArguments(string args, Type argumentsType);
    }


}