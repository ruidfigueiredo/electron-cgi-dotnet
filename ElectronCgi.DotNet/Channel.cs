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
                    return new ChannelReadResult
                    {
                        IsIdle = false,
                        Requests = frames.Select(f => _serialiser.DeserialiseRequest(f)).ToArray()
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
            _outputWriter.Write($"{_serialiser.SerialiseResponse(response)}\t");
        }

    }
}