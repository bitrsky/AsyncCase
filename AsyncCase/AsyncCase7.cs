using AsyncCase.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    // 
    internal class AsyncCase7
    {
        public void CaseWithBlock()
        {
            var scheduler = new SingleThreadTaskScheduler();

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

                DoSthAsyncWithConfigureAwaitTrue().Wait();

                Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            }, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }


        public void CaseNoBlock()
        {

            var scheduler = new SingleThreadTaskScheduler();

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

                DoSthAsyncWithConfigureAwaitFalse().Wait();

                Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            }, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }


        public void CaseAwaitAsync()
        {
            var scheduler = new SingleThreadTaskScheduler();

            Task.Factory.StartNew(async () => {
                Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

                await DoSthAsyncWithConfigureAwaitTrue();

                Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            }, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }

        private async Task DoSthAsyncWithConfigureAwaitTrue()
        {
            Console.WriteLine($"DoSthAsync started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            await Task.Delay(100);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}");

        }

        private async Task DoSthAsyncWithConfigureAwaitFalse()
        {
            Console.WriteLine($"DoSthAsync started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            await Task.Delay(100).ConfigureAwait(false);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }





        private async Task DoSth()
        {
            Console.WriteLine($"DoSthAsync started, CurrentThreadId: {Environment.CurrentManagedThreadId}");

            await Task.Delay(100);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }




    }
}
