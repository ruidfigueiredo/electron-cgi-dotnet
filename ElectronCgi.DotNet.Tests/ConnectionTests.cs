using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            connection.ResponseDispatcherMock.Verify(c => c.Init(connection.BufferBlock, connection.ChannelMock.Object));
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
            connection.ResponseDispatcherMock.Setup(re => re.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            ICollection<IRequestHandler> requestHandlers = null;
            connection.RequestExecutorMock
                .Setup(re => re.Init(It.IsAny<ICollection<IRequestHandler>>(), It.IsAny<ITargetBlock<RequestExecutedResult>>()))
                .Callback<ICollection<IRequestHandler>, ITargetBlock<RequestExecutedResult>>((handlers, target) => {
                    requestHandlers = handlers;                        
                });
            var inputStream = new MemoryStream();
            var outputWriter = new StringWriter();

            connection.Start(inputStream, outputWriter);

            Assert.NotNull(requestHandlers);
            Assert.Single(requestHandlers);
            Assert.Contains("requestType", requestHandlers.Select(r => r.RequestType));
        }
    }


    public class TestableConnection : Connection
    {
        //see: https://www.blinkingcaret.com/2016/02/17/handle-added-removed-dependencies-in-unit-tests/
        public Mock<IChannel> ChannelMock { get; private set; }
        public Mock<IResponseDispatcher> ResponseDispatcherMock { get; private set; }
        public Mock<IRequestExecutor> RequestExecutorMock { get; private set; }

        public BufferBlock<RequestExecutedResult> BufferBlock { get; private set; } = new BufferBlock<RequestExecutedResult>();

        private TestableConnection(Mock<IChannel> channelMock,
            Mock<IResponseDispatcher> responseDispatcherMock,
            Mock<IRequestExecutor> requestExecutorMock)
                : base(channelMock.Object, responseDispatcherMock.Object, requestExecutorMock.Object)
        {
            ChannelMock = channelMock;
            ResponseDispatcherMock = responseDispatcherMock;
            RequestExecutorMock = requestExecutorMock;
        }

        protected override BufferBlock<RequestExecutedResult> CreateBufferBlockForExecutedRequests()
        {
            return BufferBlock;
        }

        public static TestableConnection Create()
        {
            return new TestableConnection(new Mock<IChannel>(), new Mock<IResponseDispatcher>(), new Mock<IRequestExecutor>());
        }
    }
}
