using AsyncCase.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    // sync over async
    // SynchronizationContext 
    internal class AsyncCase1
    {
        public void CaseWithBlock()
        {
            new Thread(() =>
            {
                SingleThreadedSynchronizationContext.Run((t) =>
                {
                    Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");
                    
                    //DoSthAsyncWithConfigureAwaitTrue().GetAwaiter().GetResult();
                    DoSthAsyncWithConfigureAwaitTrue().Wait();
                    
                    Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");
                }, null);
            })
            {
                IsBackground = true
            }.Start();
        }


        public void CaseNoBlock()
        {

            new Thread(() =>
            {
                SingleThreadedSynchronizationContext.Run((t) =>
                {
                    Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");
                    
                    DoSthAsyncWithConfigureAwaitFalse().GetAwaiter().GetResult();
                    
                    Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

                }, null);
            })
            {
                IsBackground = true
            }.Start();
        }


        public void CaseAwaitAsync()
        {

            new Thread(() =>
            {
                SingleThreadedSynchronizationContext.Run(async (t) =>
                {
                    Console.WriteLine($"Case started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

                    await DoSthAsyncWithConfigureAwaitTrue();

                    Console.WriteLine($"Case end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

                }, null);
            })
            {
                IsBackground = true
            }.Start();
        }


        private async Task DoSthAsyncWithConfigureAwaitTrue()
        {
            Console.WriteLine($"DoSthAsync started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

            await Task.Delay(100);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

        }


        private async Task DoSthAsyncWithConfigureAwaitFalse()
        {
            Console.WriteLine($"DoSthAsync started, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current.GetType().Name}");

            await Task.Delay(100).ConfigureAwait(false);

            Console.WriteLine($"DoSthAsync end, CurrentThreadId: {Environment.CurrentManagedThreadId}, SynchronizationContext: {SynchronizationContext.Current?.GetType().Name}");
        }


        private async Task<string> DoAsyncOperation()
        {
            await Task.Delay(100);
            return "dasda";
        }

        public string BadCase()
        {
            // Bad - Blocking the thread that enters.
            // DoAsyncOperation will be scheduled on the default task scheduler, and remove the risk of deadlocking.
            // In the case of an exception, this method will throw an AggregateException wrapping the original exception.
            _ = Task.Run(() => DoAsyncOperation()).Result;

            // Bad - Blocking the thread that enters.
            // DoAsyncOperation will be scheduled on the default task scheduler, and remove the risk of deadlocking.
            // In the case of an exception, this method will throw the exception without wrapping it in an AggregateException.
            Task.Run(() => DoAsyncOperation()).GetAwaiter().GetResult();

            // Bad - Blocking the thread that enters, and blocking the threadpool thread inside.
            // In the case of an exception, this method will throw an AggregateException containing another AggregateException, containing the original exception.
            _ = Task.Run(() => DoAsyncOperation().Result).Result;

            // Bad - Blocking the thread that enters, and blocking the threadpool thread inside.
            Task.Run(() => DoAsyncOperation().GetAwaiter().GetResult()).GetAwaiter().GetResult();

            // Bad - Blocking the thread that enters.
            // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
            // In the case of an exception, this method will throw an AggregateException wrapping the original exception.
            _ = DoAsyncOperation().Result;

            // Bad - Blocking the thread that enters.
            // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
            DoAsyncOperation().GetAwaiter().GetResult();

            // Bad - Blocking the thread that enters.
            // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
            var task = DoAsyncOperation();
            task.Wait();
            return task.GetAwaiter().GetResult();
        }


        
    }
}
