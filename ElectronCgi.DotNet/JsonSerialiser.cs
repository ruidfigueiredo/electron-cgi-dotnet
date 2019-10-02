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

        public string SerializeArguments(object arguments)
        {
            try
            {
                return JsonConvert.SerializeObject(arguments);
            }
            catch (JsonSerializationException ex)
            {
                throw new SerialiserException($"Could not serilize arguments: {arguments}.", ex);
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


        public string SerialiseResponse(Response response)
        {
            return JsonConvert.SerializeObject(new { Type = "RESPONSE", Response = response }, _jsonSerializerSettings);
        }

        public string SerialiseRequest(Request request)
        {
            return JsonConvert.SerializeObject(new { Type = "REQUEST", Request = request }, _jsonSerializerSettings);
        }

    }
}