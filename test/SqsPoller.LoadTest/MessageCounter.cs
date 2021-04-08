using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SqsPoller.LoadTest
{
    public sealed class MessageCounter
    {
        private int _counter = 0;
        private readonly TaskCompletionSource<object> _taskCompletionSource;
        private readonly ConcurrentQueue<(TimeSpan, DateTime, DateTime)> _timeQueue; 
        public MessageCounter(int counter)
        {
            _counter = counter;
            _taskCompletionSource = new TaskCompletionSource<object>();
            _timeQueue = new ConcurrentQueue<(TimeSpan, DateTime, DateTime)>();
        }

        public void Decrement(TimeSpan timeSpan, DateTime start, DateTime end)
        {
            _timeQueue.Enqueue((timeSpan, start, end));
            if (Interlocked.Decrement(ref _counter) == 0)
            {
                _taskCompletionSource.SetResult(null);
            }
        }

        public Task Task => _taskCompletionSource.Task;
        public IEnumerable<(TimeSpan, DateTime, DateTime)> Times => _timeQueue;
    }
}
