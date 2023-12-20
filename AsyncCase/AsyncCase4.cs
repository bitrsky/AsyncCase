using AsyncCase.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    // await vs ContinueWith
    internal class AsyncCase4
    {

        public void CaseAwaitAsyncWithSynchronizationContext()
        {
            SingleThreadedSynchronizationContext.Run(async (d) => await CaseAwait(d), null);
        }


        public void CaseContinueWithAsyncWithSynchronizationContext()
        {
            SingleThreadedSynchronizationContext.Run(async (d) => await CaseContinueWith(d), null);
        }



        public void CaseAwaitAsyncWithSingleThreadTaskScheduler()
        {
            _ = Task.Factory.StartNew(async () => await CaseAwait(null), default, TaskCreationOptions.None, new SingleThreadTaskScheduler());
        }


        public void CaseContinueWithAsyncWithSingleThreadTaskScheduler()
        {
            _ = Task.Factory.StartNew(async () => await CaseContinueWith(null), default, TaskCreationOptions.None, new SingleThreadTaskScheduler());
        }


        private async Task CaseAwait(object _)
        {
            Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");

            await DoSthAsyncWithConfigureAwaitTrue();
            
            Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");

        }

        private async Task CaseContinueWith(object _)
        {
            Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");

            await DoSthAsyncWithConfigureAwaitTrue().ContinueWith((d) =>
            {
                Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");
            });
        }





        private static async Task DoSthAsyncWithConfigureAwaitTrue()
        {
            Console.WriteLine($"DoSthAsync start, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");

            await Task.Delay(100);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");

        }

    }
}
