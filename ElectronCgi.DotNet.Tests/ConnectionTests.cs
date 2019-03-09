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

        // [Fact]
        // public void Start_ChannelHasRequestThatDoesNotRequireReturn_ExecutesRequestAndSendsResponse()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(c => c.IsOpen).Returns(true).Returns(false);
        //     var requestId = Guid.NewGuid();
        //     connection.ChannelMock.Setup(c => c.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //             new Request
        //             {
        //                 Id = requestId,
        //                 Type = "requestType",
        //                 Args = "The arguments"
        //             }
        //         }
        //     });
        //     connection.SerialiserMock.Setup(s => s.DeserialiseArguments("The arguments", typeof(string))).Returns("the arguments passed to the handler");
        //     var receivedArguments = string.Empty;
        //     var letMeKnownWhenHandlerWasExecuted = new ManualResetEvent(initialState: false);
        //     connection.On<string>("requestType", requestArgs =>
        //     {
        //         receivedArguments = requestArgs;
        //         letMeKnownWhenHandlerWasExecuted.Set();
        //     });

        //     connection.Start(new MemoryStream(), new StringWriter());

        //     Assert.True(letMeKnownWhenHandlerWasExecuted.WaitOne(TimeSpan.FromMilliseconds(100)));
        //     Assert.Equal("the arguments passed to the handler", receivedArguments);
        //     connection.ChannelMock.Verify(c => c.Write(It.Is<Response>(response => response.Id == requestId)));
        // }

        // [Fact]
        // public void Start_ChannelHasRequestWithInvalidArguments_ThrowsHandlerFailedExceptionWithInvalidArgumentsInnerException()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(c => c.IsOpen).Returns(true).Returns(false);
        //     connection.SerialiserMock.Setup(s => s.DeserialiseArguments(It.IsAny<string>(), It.IsAny<Type>())).Throws(new SerialiserException());
        //     var requestId = Guid.NewGuid();
        //     connection.ChannelMock.Setup(c => c.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //             new Request
        //             {
        //                 Id = requestId,
        //                 Type = "requestType",
        //                 Args = "The args that will make the request fail"
        //             }
        //         }
        //     });
        //     connection.On<int>("requestType", requestArgs => { });


        //     var exception = Assert.Throws<InvalidArgumentsFormatException>(() => connection.Start(new MemoryStream(), new StringWriter()));
        //     Assert.Contains("requestType", exception.Message);
        //     Assert.Contains("The args that will make the request fail", exception.Message);
        //     Assert.Contains(typeof(int).ToString(), exception.Message); // error mentions expected type
        // }

        // [Fact]
        // public void Start_ChannelHasTwoRequests_ExecutesBoth()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(c => c.IsOpen).Returns(true).Returns(false);
        //     var request1Id = Guid.NewGuid();
        //     var request2Id = Guid.NewGuid();
        //     connection.SerialiserMock.Setup(s => s.DeserialiseArguments("serialised args for request 1", typeof(string))).Returns("args for the request");
        //     connection.SerialiserMock.Setup(s => s.DeserialiseArguments("serialised args for request 2", typeof(string))).Returns("args for the request 2");
        //     connection.ChannelMock.Setup(c => c.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //             new Request
        //             {
        //                 Id = request1Id,
        //                 Type = "requestType",
        //                 Args = "serialised args for request 1"
        //             },
        //             new Request
        //             {
        //                 Id = request2Id,
        //                 Type = "requestType",
        //                 Args = "serialised args for request 2"
        //             }
        //         }
        //     });
        //     var receivedArguments = new List<string>();
        //     connection.On<string>("requestType", requestArgs =>
        //     {
        //         receivedArguments.Add(requestArgs);
        //     });

        //     connection.Start(new MemoryStream(), new StringWriter());

        //     Assert.Equal(2, receivedArguments.Count);
        //     Assert.Equal("args for the request", receivedArguments[0]);
        //     Assert.Equal("args for the request 2", receivedArguments[1]);
        //     connection.ChannelMock.Verify(c => c.Write(It.IsAny<Response>()), Times.Exactly(2));
        //     connection.ChannelMock.Verify(c => c.Write(It.Is<Response>(response => response.Id == request1Id)));
        //     connection.ChannelMock.Verify(c => c.Write(It.Is<Response>(response => response.Id == request2Id)));
        // }

        // [Fact]
        // public void Start_ChannelHasRequestThatRequiresReturn_ExecutesRequestAndSendsResponse()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(c => c.IsOpen).Returns(true).Returns(false);
        //     var requestId = Guid.NewGuid();
        //     connection.SerialiserMock.Setup(s => s.DeserialiseArguments("the args for the request", typeof(string))).Returns("args for the request");
        //     connection.ChannelMock.Setup(c => c.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //         new Request
        //             {
        //                 Id = requestId,
        //                 Type = "requestType",
        //                 Args = "the args for the request"
        //             }
        //         }
        //     });
        //     var receivedArguments = string.Empty;
        //     var wasRequestExecuted = false;
        //     connection.On<string, string>("requestType", requestArgs =>
        //     {
        //         receivedArguments = requestArgs;
        //         wasRequestExecuted = true;
        //         return "the return value";
        //     });

        //     connection.Start(new MemoryStream(), new StringWriter());

        //     Assert.True(wasRequestExecuted);
        //     Assert.Equal("args for the request", receivedArguments);
        //     connection.ChannelMock.Verify(c => c.Write(It.Is<Response<string>>(response => response.Id == requestId && response.Result == "the return value")));
        // }

        // [Fact]
        // public void Start_ChannelHasRequestWithoutRegisteredHandlers_ThrowsNoRequestHandlerFoundException()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(c => c.IsOpen).Returns(true).Returns(false);
        //     connection.ChannelMock.Setup(c => c.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //             new Request{
        //                 Id = Guid.NewGuid(),
        //                 Type = "type_without_handler"
        //             }
        //         }
        //     });

        //     Assert.Throws<NoRequestHandlerFoundException>(() => connection.Start(new MemoryStream(), new StringWriter()));
        // }

        // [Fact(Skip = "Handler exceptions are being only logged for the moment and won't take the connection down")]
        // public void Start_ChannelHasRequestWhoseHandlerThrowsException_ThrowsHandlerFailedException()
        // {
        //     var connection = TestableConnection.Create();
        //     connection.ChannelMock.SetupSequence(channel => channel.IsOpen).Returns(true).Returns(false);
        //     connection.ChannelMock.Setup(channel => channel.Read()).Returns(new ChannelReadResult
        //     {
        //         IsIdle = false,
        //         Requests = new[] {
        //             new Request
        //             {
        //                 Type = "requestType",
        //                 Id = Guid.NewGuid(),
        //                 Args = string.Empty
        //             }
        //         }
        //     });
        //     connection.On<object>("requestType", _ =>
        //     {
        //         throw new InvalidOperationException("this is a test");
        //     });

        //     var exception = Assert.Throws<HandlerFailedException>(() => connection.Start(new MemoryStream(), new StringWriter()));
        //     Assert.Contains("requestType", exception.Message);
        //     Assert.IsType<InvalidOperationException>(exception.InnerException);
        // }

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
            Assert.True(requestHandlers.Any(r => r.RequestType == "requestType"));
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
