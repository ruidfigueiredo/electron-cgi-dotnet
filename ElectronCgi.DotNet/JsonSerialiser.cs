using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ElectronCgi.DotNet
{
    public class JsonSerialiser : ISerialiser
    {
        private readonly static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public object DeserialiseArguments(string args, Type argumentsType)
        {
            try
            {
                return JsonConvert.DeserializeObject(args, argumentsType);
            }
            catch (JsonReaderException ex)
            {
                throw new SerialiserException($"Failed to deserialise arguments", ex);
            } 
        }

        public Request DeserialiseRequest(string serialiserRequest)
        {
            try
            {
                return JsonConvert.DeserializeObject<Request>(serialiserRequest);
            }
            catch (JsonReaderException ex)
            {
                throw new SerialiserException($"Invalid format in request: {serialiserRequest}.", ex);
            }
        }

        public string SerialiseResponse(Response response)
        {
            return JsonConvert.SerializeObject(response, _jsonSerializerSettings);
        }
    }
}