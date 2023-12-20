using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase.Common
{
    internal class MaxConcurrencySynchronizationContext : SynchronizationContext
    {
        private readonly SemaphoreSlim _semaphore;

        public MaxConcurrencySynchronizationContext(int maxConcurrencyLevel)
        {
            _semaphore = new SemaphoreSlim(maxConcurrencyLevel);
        }
        public override void Post(SendOrPostCallback d, object state)
        {
            _semaphore.WaitAsync().ContinueWith(delegate
            {
                try { Execute(d, state); } finally { _semaphore.Release(); }
            },
            default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        public override void Send(SendOrPostCallback d, object state)
        {
            _semaphore.Wait();
            try { Execute(d, state); } finally { _semaphore.Release(); }
        }

        private void Execute(SendOrPostCallback d, object state)
        {
            Console.WriteLine("Executing action, CurrentThreadId: {0}", Environment.CurrentManagedThreadId);
            d(state);
        }

        public static void Run(Action action, int maxConcurrencyLevel = 1)
        {
            var previous = Current;
            var context = new MaxConcurrencySynchronizationContext(maxConcurrencyLevel);
            SetSynchronizationContext(context);
            try
            {
                context.Send(d =>
                {
                    action();
                }, null);
                
            }
            finally
            {
                SetSynchronizationContext(previous);
            }
        }


    }
}

