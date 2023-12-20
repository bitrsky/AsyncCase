using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AsyncCase.Common
{
    internal class SingleThreadedSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object State)> _queue;

        public SingleThreadedSynchronizationContext()
        {
            _queue = new BlockingCollection<(SendOrPostCallback Callback, object State)>();
        }


        public override void Send(SendOrPostCallback d, object state) // Sync operations
        {
            Execute(d, state);
        }

        public override void Post(SendOrPostCallback d, object state) // Async operations
        {
             _queue.Add((d, state));
        }

        private void Execute(SendOrPostCallback d, object state)
        {

            try
            {
                d(state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static void Run(SendOrPostCallback d, object state)
        {
            var context = new SingleThreadedSynchronizationContext();
            SetSynchronizationContext(context);
            context.Execute(d, state);
            while (true)
            {
                var item = context._queue.Take();
                context.Execute(item.Callback, item.State);
            }
        }
    }
}
