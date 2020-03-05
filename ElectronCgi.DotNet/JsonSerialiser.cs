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

        public ChannelMessage DeserializeMessage(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<ChannelMessage>(message);
            }
            catch (JsonReaderException ex)
            {
                throw new SerialiserException($"Invalid format in serialized message: {message}.", ex);
            }
        }

        public string Serialise(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
            }
            catch (JsonSerializationException ex)
            {
                throw new SerialiserException($"Serialisation failed for: {obj}.", ex);
            }
        }
    }
}