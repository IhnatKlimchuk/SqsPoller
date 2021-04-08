using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SqsPoller.LoadTest
{
    public class SqsMessageConsumer : IConsumer<SqsMessage>
    {
        private static HttpClient client = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = 4, }) { Timeout = TimeSpan.FromSeconds(10) };
        private MessageCounter _messageCounter;
        private ILogger<SqsMessageConsumer> _logger;
        public SqsMessageConsumer(MessageCounter messageCounter, ILogger<SqsMessageConsumer> logger)
        {
            _messageCounter = messageCounter;
            _logger = logger;
        }

        public async Task Consume(SqsMessage message, CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {

                long counter = 0;
                for (int i = 0; i < 10000; i++)
                {
                    counter += 1;
                }
                await client.GetAsync("http://stackoverflow.com/");
                await client.GetAsync("http://stackoverflow.com/");
                stopwatch.Stop();
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                var end = DateTime.UtcNow;
                _messageCounter.Decrement(stopwatch.Elapsed, start, end);
            }
            _logger.LogInformation("SUCCESS!");
        }
    }
}