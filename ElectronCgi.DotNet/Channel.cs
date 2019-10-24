using System;
using System.IO;
using System.Linq;
using Serilog;

namespace ElectronCgi.DotNet
{
    public class Channel : IChannel
    {
        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private StreamReader _inputReader = null;
        private TextWriter _outputWriter = null;

        private readonly char[] _buffer = new char[2048];

        private readonly IInputStreamParser _inputStreamParser;
        private readonly ISerialiser _serialiser;

        public Channel(IInputStreamParser inputStreamParser, ISerialiser serialiser)
        {
            _inputStreamParser = inputStreamParser;
            _serialiser = serialiser;
        }

        public void Init(Stream inputStream, TextWriter outputWriter)
        {
            _inputReader = new StreamReader(inputStream);
            _outputWriter = outputWriter;
            _isOpen = true;
        }


        public ChannelReadResult Read()
        {
            if (!_isOpen)
                throw new InvalidOperationException("Channel is closed or was not opened");

            if (_inputReader.EndOfStream)
            {
                _isOpen = false;
                return new ChannelReadResult
                {
                    IsIdle = false
                };
            }

            var bytesRead = _inputReader.Read(_buffer, 0, _buffer.Length);
            if (bytesRead != 0)
            {
                _inputStreamParser.AddPartial(new string(_buffer, 0, bytesRead));
                if (_inputStreamParser.HasCompletedFrames)
                {

                    var frames = _inputStreamParser.GetCompletedFrames();
                    _inputStreamParser.ClearCompletedFrames();
                    var messages = frames.Select(message => _serialiser.DeserializeMessage(message));
                    return new ChannelReadResult
                    {
                        IsIdle = false,
                        Requests = messages.Where(m => m.IsRequest).Select(m => m.Request).ToArray(),
                        Responses = messages.Where(m => m.IsResponse).Select(m => m.Response).ToArray()
                    };
                }
                else
                {
                    return new ChannelReadResult
                    {
                        IsIdle = false
                    };
                }
            }
            else
            {
                return new ChannelReadResult
                {
                    IsIdle = true
                };
            }

        }

        public void Write(Response response)
        {
            var serialisedResponse = _serialiser.SerialiseResponse(response);
            Log.Verbose($"Sending Response: {serialisedResponse}");
            _outputWriter.Write($"{serialisedResponse}\t");
        }

        public void Write(Request request)
        {
            var serialisedRequest = _serialiser.SerialiseRequest(request);
            Log.Verbose($"Sending Request: {serialisedRequest}");
            _outputWriter.Write($"{serialisedRequest}\t");
        }


    }
}