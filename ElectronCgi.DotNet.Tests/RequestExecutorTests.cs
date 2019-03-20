using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Moq;
using Xunit;

namespace ElectronCgi.DotNet.Tests
{
    public class RequestExecutorTests
    {
        [Fact]
        public void ExecuteAsync_NoHandlers_CreatesResponseWithNoRequestHandlerFoundException()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            var cancelationTokenSource = new CancellationTokenSource();

            requestExecutor.ExecuteAsync(new Request
            {
                Id = Guid.NewGuid(),
                Type = "requestType"
            }, cancelationTokenSource.Token).Wait();
            var executedRequest = requestExecutor.RequestExecutedResulSourceBlock.Receive();

            Assert.True(executedRequest.IsFaulted);
            Assert.Null(executedRequest.Response);
            Assert.IsType<NoRequestHandlerFoundException>(executedRequest.Exception);
        }

        [Fact]
        public void ExecuteAsync_NoHandlerForRequestType_CreatesResponseWithNoRequestHandlerFoundException()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            var cancelationTokenSource = new CancellationTokenSource();
            var requestHandlerMock = new Mock<IRequestHandler>();
            var requestId = Guid.NewGuid();
            requestHandlerMock.SetupGet(r => r.RequestType).Returns("requestType");
            requestExecutor.Handlers.Add(requestHandlerMock.Object);

            requestExecutor.ExecuteAsync(new Request
            {
                Id = Guid.NewGuid(),
                Type = "requesTypeWithNoHandler"
            }, cancelationTokenSource.Token).Wait();
            var executedRequest = requestExecutor.RequestExecutedResulSourceBlock.Receive();

            Assert.True(executedRequest.IsFaulted);
            Assert.Null(executedRequest.Response);
            Assert.IsType<NoRequestHandlerFoundException>(executedRequest.Exception);
        }

        [Fact]
        public void ExecuteAsync_ExistingHandlerForRequest_ExecutedRequestHandlerToProduceResponse()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            var requestHandlerMock = new Mock<IRequestHandler>();
            var requestId = Guid.NewGuid();
            requestHandlerMock.SetupGet(r => r.RequestType).Returns("requestType");
            requestHandlerMock.SetupGet(r => r.ArgumentsType).Returns(typeof(string));
            requestExecutor.SerialiserMock.Setup(s => s.DeserialiseArguments("the args for the request", typeof(string))).Returns("the deserialised arguments");
            requestHandlerMock.Setup(r => r.HandleRequestAsync(It.IsAny<Guid>(), It.IsAny<object>())).ReturnsAsync(new Response
            {
                Id = requestId
            });
            requestExecutor.Handlers.Add(requestHandlerMock.Object);
            var cancelationTokenSource = new CancellationTokenSource();

            requestExecutor.ExecuteAsync(new Request
            {
                Id = requestId,
                Type = "requestType",
                Args = "the args for the request"
            }, cancelationTokenSource.Token).Wait();
            var executedRequest = requestExecutor.RequestExecutedResulSourceBlock.Receive();

            Assert.Equal(requestId, executedRequest.Response.Id);
            Assert.False(executedRequest.IsFaulted);
            Assert.Null(executedRequest.Exception);
            requestHandlerMock.Verify(h => h.HandleRequestAsync(requestId, "the deserialised arguments"));
        }

        [Fact]
        public void ExecuteAsync_InvalidRequestArguments_ProducesFaultedRequestExecutedResultWithException()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            requestExecutor.SerialiserMock.Setup(s => s.DeserialiseArguments(It.IsAny<string>(), It.IsAny<Type>())).Throws<SerialiserException>();
            var cancelationTokenSource = new CancellationTokenSource();
            var requestHandlerMock = new Mock<IRequestHandler>();
            var requestId = Guid.NewGuid();
            requestHandlerMock.SetupGet(r => r.RequestType).Returns("requestType");
            requestExecutor.Handlers.Add(requestHandlerMock.Object);

            requestExecutor.ExecuteAsync(new Request
            {
                Id = Guid.NewGuid(),
                Type = "requestType"
            }, cancelationTokenSource.Token).Wait();
            var executedRequest = requestExecutor.RequestExecutedResulSourceBlock.Receive();

            Assert.True(executedRequest.IsFaulted);
            Assert.Null(executedRequest.Response);
            Assert.IsType<HandlerFailedException>(executedRequest.Exception);
            Assert.IsType<SerialiserException>(executedRequest.Exception.InnerException);
        }


        [Fact]
        public void ExecuteAsync_HandlerThrowsException_ProducesFaultedRequestExecutedResultWithException()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            var cancelationTokenSource = new CancellationTokenSource();
            var requestHandlerMock = new Mock<IRequestHandler>();
            var requestId = Guid.NewGuid();
            requestHandlerMock.SetupGet(r => r.RequestType).Returns("requestType");
            requestHandlerMock
                .Setup(h => h.HandleRequestAsync(It.IsAny<Guid>(), It.IsAny<object>()))
                .ThrowsAsync(new InvalidOperationException("the exception thrown by the handler"));
            requestExecutor.Handlers.Add(requestHandlerMock.Object);

            requestExecutor.ExecuteAsync(new Request
            {
                Id = Guid.NewGuid(),
                Type = "requestType"
            }, cancelationTokenSource.Token).Wait();
            var executedRequest = requestExecutor.RequestExecutedResulSourceBlock.Receive();

            Assert.True(executedRequest.IsFaulted);
            Assert.Null(executedRequest.Response);
            Assert.IsType<HandlerFailedException>(executedRequest.Exception);
            Assert.Equal("the exception thrown by the handler", executedRequest.Exception.InnerException.Message);
        }

        [Fact]
        public void ExecuteAsync_Cancelled_ThrowsTaskCancelledException()
        {
            var requestExecutor = TestableRequestExecutor.Create();
            var cancelationTokenSource = new CancellationTokenSource();
            cancelationTokenSource.Cancel();

            Assert.Throws<TaskCanceledException>(() => requestExecutor.ExecuteAsync(new Request
            {
                Id = Guid.NewGuid(),
                Type = "requestType"
            }, cancelationTokenSource.Token).GetAwaiter().GetResult());

        }
    }

    class TestableRequestExecutor : RequestExecutor
    {
        public Mock<ISerialiser> SerialiserMock { get; set; }
        private BufferBlock<RequestExecutedResult> _requestExecutedResultBufferBlock = new BufferBlock<RequestExecutedResult>();
        public ISourceBlock<RequestExecutedResult> RequestExecutedResulSourceBlock
        {
            get
            {
                return _requestExecutedResultBufferBlock;
            }
        }
        public IList<IRequestHandler> Handlers { get; } = new List<IRequestHandler>();

        private TestableRequestExecutor(Mock<ISerialiser> serialiserMock) : base(serialiserMock.Object)
        {
            SerialiserMock = serialiserMock;
            Init(Handlers, _requestExecutedResultBufferBlock);
        }

        public static TestableRequestExecutor Create()
        {
            return new TestableRequestExecutor(new Mock<ISerialiser>());
        }


    }
}