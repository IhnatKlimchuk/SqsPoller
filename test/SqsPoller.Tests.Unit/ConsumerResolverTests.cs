using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Xunit;

namespace SqsPoller.Tests.Unit
{
    public class ConsumerResolverTests
    {
        [Fact]
        public async Task Resolve_FirstMessageConsumerFound_HandleMethodIsInvoked()
        {
            //Arrange
            var fakeService = Substitute.For<IFakeService>();
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var firstConsumer = new FirstMessageConsumer(fakeService);
            var secondConsumer = new SecondMessageConsumer(fakeService);
            var consumers = new IConsumer[] {firstConsumer, secondConsumer};
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var message = new FirstMessage {Value = "First Message"};
            var messageAsString = JsonConvert.SerializeObject(message);
            var messageType = nameof(FirstMessage);
            var sqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = messageType}}
                },
                Body = messageAsString
            };

            //Act
            await consumerResolver.Resolve(sqsMessage, CancellationToken.None);

            //Assert
            fakeService.Received(1).FirstMethod();
            fakeService.DidNotReceive().SecondMethod();
        }

        [Fact]
        public async Task Resolve_SecondMessageConsumerFound_HandleMethodIsInvoked()
        {
            //Arrange
            var fakeService = Substitute.For<IFakeService>();
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var firstConsumer = new FirstMessageConsumer(fakeService);
            var secondConsumer = new SecondMessageConsumer(fakeService);
            var consumers = new IConsumer[] {firstConsumer, secondConsumer};
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var message = new SecondMessage {Value = "First Message"};
            var messageAsString = JsonConvert.SerializeObject(message);
            var messageType = nameof(SecondMessage);
            var sqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = messageType}}
                },
                Body = messageAsString
            };

            //Act
            await consumerResolver.Resolve(sqsMessage, CancellationToken.None);

            //Assert
            fakeService.DidNotReceive().FirstMethod();
            fakeService.Received(1).SecondMethod();
        }

        [Fact]
        public void Resolve_ConsumerNotFound_ConsumerNotFoundExceptionThrown()
        {
            //Arrange
            var fakeService = Substitute.For<IFakeService>();
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var firstConsumer = new FirstMessageConsumer(fakeService);
            var secondConsumer = new SecondMessageConsumer(fakeService);
            var consumers = new IConsumer[] {firstConsumer, secondConsumer};
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var message = new SecondMessage {Value = "First Message"};
            var messageAsString = JsonConvert.SerializeObject(message);
            var messageType = "fakeMessageType";
            var sqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = messageType}}
                },
                Body = messageAsString
            };

            //Act
            var task = consumerResolver.Resolve(sqsMessage, CancellationToken.None);

            //Assert
            Should.Throw<ConsumerNotFoundException>(task);
        }
        
        [Fact]
        public async Task Resolve_ConsumerFound_TwoMethodsAreInvoked()
        {
            //Arrange
            var fakeService = Substitute.For<IFakeService>();
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var thirdConsumer = new ThirdMessageConsumer(fakeService);
            var consumers = new IConsumer[] {thirdConsumer};
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var firstMessage = new FirstMessage {Value = "First Message"};
            var firstSqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = nameof(FirstMessage)}}
                },
                Body = JsonConvert.SerializeObject(firstMessage)
            };
            
            var secondMessage = new SecondMessage {Value = "Second Message"};
            var secondSqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = nameof(SecondMessage)}}
                },
                Body = JsonConvert.SerializeObject(secondMessage)
            };

            //Act
            await consumerResolver.Resolve(firstSqsMessage, CancellationToken.None);
            await consumerResolver.Resolve(secondSqsMessage, CancellationToken.None);

            //Assert
            fakeService.Received(1).FirstMethod();
            fakeService.Received(1).SecondMethod();
        }
        
        [Fact]
        public async Task Resolve_ConsumersFound_TwoMethodsAreInvokedTwice()
        {
            //Arrange
            var fakeService = Substitute.For<IFakeService>();
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var firstConsumer = new FirstMessageConsumer(fakeService);
            var secondConsumer = new SecondMessageConsumer(fakeService);
            var thirdConsumer = new ThirdMessageConsumer(fakeService);
            var consumers = new IConsumer[] {firstConsumer, secondConsumer, thirdConsumer};
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var firstMessage = new FirstMessage {Value = "First Message"};
            var firstSqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = nameof(FirstMessage)}}
                },
                Body = JsonConvert.SerializeObject(firstMessage)
            };
            
            var secondMessage = new SecondMessage {Value = "Second Message"};
            var secondSqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = nameof(SecondMessage)}}
                },
                Body = JsonConvert.SerializeObject(secondMessage)
            };

            //Act
            await consumerResolver.Resolve(firstSqsMessage, CancellationToken.None);
            await consumerResolver.Resolve(secondSqsMessage, CancellationToken.None);

            //Assert
            fakeService.Received(2).FirstMethod();
            fakeService.Received(2).SecondMethod();
        }

        [Fact]
        public async Task Test()
        {
            //Arrange
            var fakeLogger = Substitute.For<ILogger<ConsumerResolver>>();
            var throwingMessageConsumer = new ThrowingMessageConsumer();
            var consumers = new IConsumer[] { throwingMessageConsumer };
            var consumerResolver = new ConsumerResolver(consumers, fakeLogger);
            var firstMessage = new ThrowingMessage { Value = "First Message" };
            var firstSqsMessage = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"MessageType", new MessageAttributeValue {StringValue = nameof(ThrowingMessage)}}
                },
                Body = JsonConvert.SerializeObject(firstMessage)
            };

            var sqs = Substitute.For<IAmazonSQS>();
            sqs.ReceiveMessageAsync(Arg.Any<ReceiveMessageRequest>(), Arg.Any<CancellationToken>())
                .Returns(new ReceiveMessageResponse { Messages = new List<Message> { firstSqsMessage } });

            SqsPollerHostedService sqsPollerHostedService = new SqsPollerHostedService(
                sqs,
                new SqsPollerConfig { QueueUrl = "blah" },
                consumerResolver,
                Substitute.For<ILogger<SqsPollerHostedService>>());

            await sqsPollerHostedService.Handle("blah", CancellationToken.None);


            await sqs.DidNotReceive().DeleteMessageAsync(Arg.Any<DeleteMessageRequest>(), Arg.Any<CancellationToken>());
        }
    }
}