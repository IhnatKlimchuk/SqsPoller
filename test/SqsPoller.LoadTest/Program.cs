using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SqsPoller.LoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int messagesToProcess = 3000;
            var messages = Enumerable
                .Range(0, messagesToProcess)
                .Select(x => new Message 
                { 
                    Body = "{}", 
                    MessageId = "blah", 
                    ReceiptHandle = "blah",
                    MessageAttributes = new Dictionary<string, MessageAttributeValue> { { "MessageType", new MessageAttributeValue { StringValue = "SqsMessage" } } } 
                } )
                .ToList();

            var sqs = new SqsEmulator();
            var counter = new MessageCounter(messagesToProcess);

            using var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton(counter)
                .AddSqsPoller(sqs, new SqsPollerConfig { QueueUrl = "who cares anyway..." /*other is default*/ }, new[] { typeof(SqsMessageConsumer) })
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var sqsHostedService = serviceProvider.GetRequiredService<IHostedService>();

            sqs.PutMessages(messages);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            await sqsHostedService.StartAsync(CancellationToken.None);
            await counter.Task;

            stopwatch.Stop();
            logger.LogInformation($"Total time taken: {stopwatch.Elapsed.TotalSeconds} seconds");
            logger.LogInformation($"First start: {counter.Times.Min(x => x.Item2)}");
            logger.LogInformation($"Last start: {counter.Times.Max(x => x.Item2)}");
            logger.LogInformation($"First end: {counter.Times.Min(x => x.Item3)}");
            logger.LogInformation($"Last end: {counter.Times.Max(x => x.Item3)}");
            var test = counter.Times.OrderByDescending(x => x).ToList();
            logger.LogInformation($"Longest task time taken: {test.First().Item1.TotalSeconds} seconds, start: {test.First().Item2} end:{test.First().Item3}");

            await sqsHostedService.StopAsync(CancellationToken.None);
            Console.ReadKey();
        }
    }
}
