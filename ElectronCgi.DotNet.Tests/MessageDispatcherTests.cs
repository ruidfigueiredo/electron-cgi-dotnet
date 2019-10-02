using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Moq;
using Xunit;

namespace ElectronCgi.DotNet.Tests
{
    public class MessageDispatcherTests
    {
        [Fact]
        public void StartAsync_ReceivesFaultedRequestExecutedResult_ThrowsExceptionInRequestExecutedResult()
        {
            var messageDispatcher = TestableMessageDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            messageDispatcher.DispatchMessagesBufferBlock.Post(new RequestExecutedChannelMessage(new RequestExecutedResult(new InvalidOperationException("exception message"))));
            messageDispatcher.DispatchMessagesBufferBlock.Complete();

            Assert.Throws<InvalidOperationException>(() => messageDispatcher.StartAsync(cancellationTokenSource.Token).GetAwaiter().GetResult());
        }

        [Fact]
        public void StartAsync_ReceivesdRequestExecutedResult_CallsChannelWriteOnTheResponse()
        {
            var messageDispatcher = TestableMessageDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            var response = new Response
            {
                Id = Guid.NewGuid()
            };
            messageDispatcher.DispatchMessagesBufferBlock.Post(new RequestExecutedChannelMessage(new RequestExecutedResult(response)));
            messageDispatcher.DispatchMessagesBufferBlock.Complete();

            messageDispatcher.StartAsync(cancellationTokenSource.Token).Wait();

            messageDispatcher.ChannelMock.Verify(c => c.Write(response), Times.Once);
        }

        [Fact]
        public void StartAsync_ReceivesRequestExecutedResultBitThereWasACancellation_DoesNotCallsChannelWriteOnTheResponse()
        {
            var messageDispatcher = TestableMessageDispatcher.Create();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var response = new Response
            {
                Id = Guid.NewGuid()
            };
            messageDispatcher.DispatchMessagesBufferBlock.Post(new RequestExecutedChannelMessage(new RequestExecutedResult(response)));
            messageDispatcher.DispatchMessagesBufferBlock.Complete();

            messageDispatcher.StartAsync(cancellationTokenSource.Token).Wait();

            messageDispatcher.ChannelMock.Verify(c => c.Write(response), Times.Never);
        }        
    }

    class TestableMessageDispatcher : MessageDispatcher
    {
        private BufferBlock<IChannelMessage> _dispatchMessagesBufferBlock = new BufferBlock<IChannelMessage>();
        public ITargetBlock<IChannelMessage> DispatchMessagesBufferBlock
        {
            get
            {
                return _dispatchMessagesBufferBlock;
            }
        }
        public Mock<IChannel> ChannelMock { get; private set; }

        private TestableMessageDispatcher(Mock<IChannel> channelMock)
        {
            ChannelMock = channelMock;
            Init(_dispatchMessagesBufferBlock, channelMock.Object);
        }

        public static TestableMessageDispatcher Create()
        {
            return new TestableMessageDispatcher(new Mock<IChannel>());
        }


    }
}