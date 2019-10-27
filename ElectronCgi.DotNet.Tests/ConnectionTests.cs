using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Moq;
using Xunit;

namespace ElectronCgi.DotNet.Tests
{
    public class ConnectionTests
    {        
        [Fact]
        public void Start_ValidInputs_CallsInitOnChannelRequestDispatcherAndRequestExecutor()
        {
            var connection = TestableConnection.Create();
            connection.ChannelMock.Setup(c => c.IsOpen).Returns(false);
            var inputStream = new MemoryStream();
            var outputWriter = new StringWriter();

            connection.Start(inputStream, outputWriter);

            connection.ChannelMock.Verify(c => c.Init(inputStream, outputWriter));
            connection.RequestExecutorMock.Verify(c => c.Init(It.IsAny<ICollection<IRequestHandler>>(), connection.BufferBlock));
            connection.MessageDispatcherMock.Verify(c => c.Init(connection.BufferBlock, connection.ChannelMock.Object));
            connection.ResponseHandlerExecutorMock.Verify(c => c.Init(It.IsAny<ICollection<IResponseHandler>>(), connection.BufferBlock));
        }

        [Fact]
        public void On_DuplicateRequestType_ThrowsDuplicateHandlerForTypeException()
        {
            var connection = TestableConnection.Create();
            connection.On<string>("requestType", _ => { });

            var exception = Assert.Throws<DuplicateHandlerForRequestTypeException>(() => connection.On<string>("requestType", _ => { }));

            Assert.Contains("requestType", exception.Message);
        }

        [Fact]
        public void Start_ValidRequestHandler_PassesHandlerToRequestExecutor()
        {
            var connection = TestableConnection.Create();
            connection.On<string>("requestType", _ => { });

            connection.ChannelMock.Setup(c => c.IsOpen).Returns(false);
            connection.MessageDispatcherMock.Setup(re => re.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            ICollection<IRequestHandler> requestHandlers = null;
            connection.RequestExecutorMock
                .Setup(re => re.Init(It.IsAny<ICollection<IRequestHandler>>(), It.IsAny<ITargetBlock<IChannelMessage>>()))
                .Callback<ICollection<IRequestHandler>, ITargetBlock<IChannelMessage>>((handlers, target) => {
                    requestHandlers = handlers;                        
                });
            var inputStream = new MemoryStream();
            var outputWriter = new StringWriter();

            connection.Start(inputStream, outputWriter);

            Assert.NotNull(requestHandlers);
            Assert.Single(requestHandlers);
            Assert.Contains("requestType", requestHandlers.Select(r => r.RequestType));
        }

        //TODO: (RF) Add tests that check that .Send methods correctly create IResponseHandler entries and interact correctly with the bufferblock
    }


    public class TestableConnection : Connection
    {
        //see: https://www.blinkingcaret.com/2016/02/17/handle-added-removed-dependencies-in-unit-tests/
        public Mock<IChannel> ChannelMock { get; private set; }
        public Mock<IMessageDispatcher> MessageDispatcherMock { get; private set; }
        public Mock<IRequestExecutor> RequestExecutorMock { get; private set; }
        public Mock<IResponseHandlerExecutor> ResponseHandlerExecutorMock { get; private set; }
        public Mock<ISerialiser> SerializerMock { get; private set; }

        public BufferBlock<IChannelMessage> BufferBlock { get; private set; }
        private TestableConnection(Mock<IChannel> channelMock,
            Mock<IMessageDispatcher> messageDispatcherMock,
            Mock<IRequestExecutor> requestExecutorMock,
            Mock<IResponseHandlerExecutor> responseHandlerExecutorMock,
            Mock<ISerialiser> serialiserMock, 
            BufferBlock<IChannelMessage> bufferBlock)
                : base(channelMock.Object, messageDispatcherMock.Object, requestExecutorMock.Object, responseHandlerExecutorMock.Object, serialiserMock.Object, bufferBlock) 
        {
            ChannelMock = channelMock;
            MessageDispatcherMock = messageDispatcherMock;
            RequestExecutorMock = requestExecutorMock;
            ResponseHandlerExecutorMock = responseHandlerExecutorMock;
            SerializerMock = serialiserMock;
            BufferBlock = bufferBlock;
        }

        public static TestableConnection Create()
        {
            return new TestableConnection(new Mock<IChannel>(), new Mock<IMessageDispatcher>(), new Mock<IRequestExecutor>(), new Mock<IResponseHandlerExecutor>(), new Mock<ISerialiser>(), new BufferBlock<IChannelMessage>()); 
        }
    }
}
