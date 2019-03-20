using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Moq;
using Xunit;

namespace ElectronCgi.DotNet.Tests
{
    public class ResponseDispatcherTests
    {
        [Fact]
        public void StartAsync_ReceivesFaultedRequestExecutedResult_ThrowsExceptionInRequestExecutedResult()
        {
            var responseDispatcher = TestableResponseDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            responseDispatcher.RequestExecutedResultTargetBlock.Post(new RequestExecutedResult(new InvalidOperationException("exception message")));
            responseDispatcher.RequestExecutedResultTargetBlock.Complete();

            Assert.Throws<InvalidOperationException>(() => responseDispatcher.StartAsync(cancellationTokenSource.Token).GetAwaiter().GetResult());
        }

        [Fact]
        public void StartAsync_ReceivesdRequestExecutedResult_CallsChannelWriteOnTheResponse()
        {
            var responseDispatcher = TestableResponseDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            var response = new Response
            {
                Id = Guid.NewGuid()
            };
            responseDispatcher.RequestExecutedResultTargetBlock.Post(new RequestExecutedResult(response));
            responseDispatcher.RequestExecutedResultTargetBlock.Complete();

            responseDispatcher.StartAsync(cancellationTokenSource.Token).Wait();

            responseDispatcher.ChannelMock.Verify(c => c.Write(response), Times.Once);
        }

        [Fact]
        public void StartAsync_ReceivesRequestExecutedResultBitThereWasACancellation_DoesNotCallsChannelWriteOnTheResponse()
        {
            var responseDispatcher = TestableResponseDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var response = new Response
            {
                Id = Guid.NewGuid()
            };
            responseDispatcher.RequestExecutedResultTargetBlock.Post(new RequestExecutedResult(response));
            responseDispatcher.RequestExecutedResultTargetBlock.Complete();

            responseDispatcher.StartAsync(cancellationTokenSource.Token).Wait();

            responseDispatcher.ChannelMock.Verify(c => c.Write(response), Times.Never);
        }        
    }

    class TestableResponseDispatcher : ResponseDispatcher
    {
        private BufferBlock<RequestExecutedResult> _responseExecutedResultBufferBlock = new BufferBlock<RequestExecutedResult>();
        public ITargetBlock<RequestExecutedResult> RequestExecutedResultTargetBlock
        {
            get
            {
                return _responseExecutedResultBufferBlock;
            }
        }
        public Mock<IChannel> ChannelMock { get; private set; }

        private TestableResponseDispatcher(Mock<IChannel> channelMock)
        {
            ChannelMock = channelMock;
            Init(_responseExecutedResultBufferBlock, channelMock.Object);
        }

        public static TestableResponseDispatcher Create()
        {
            return new TestableResponseDispatcher(new Mock<IChannel>());
        }


    }
}