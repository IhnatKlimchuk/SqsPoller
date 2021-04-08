using System.Threading;
using System.Threading.Tasks;

namespace SqsPoller.Tests.Unit
{
    public class ThrowingMessageConsumer : IConsumer<ThrowingMessage>
    {
        public ThrowingMessageConsumer()
        {
        }

        public async Task Consume(ThrowingMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, new CancellationToken(canceled: true));
        }
    }
}