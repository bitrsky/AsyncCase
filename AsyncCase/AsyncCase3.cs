using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    // Task.Run
    // Action & Func<Task>
    internal class AsyncCase3
    {
        private async Task TaskAsync()
        {
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"async task, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }

        private void TaskSync()
        {
            Thread.Sleep(1000);
            Console.WriteLine($"sync task, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }

        public void TaskRun()
        {

            //var t1 = Task.Run(() => TaskSync());
            //t1.Wait();

            //var t2 = Task.Run(() => TaskAsync());
            //t2.Wait();
            
            //var t3 = Task.Run(async () => await TaskAsync());
            //t3.Wait();
 
            //var t4 = Task.Run(async () => TaskAsync());
            //t4.Wait();

            //var t5 = Task.Run(async () => {
            //    await Task.Delay(1000);
            //    Console.WriteLine($"async task, CurrentThreadId: {Environment.CurrentManagedThreadId}");
            //});
            //t5.Wait();
            
            Console.WriteLine($"case over, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }




        public void CustomTaskRun()
        {

            //var t1 = CustomTask.Run(() => TaskSync());
            //t1.Wait();

            //var t2 = CustomTask.Run(() => TaskAsync());
            //t2.Wait();

            //var t3 = CustomTask.Run(async () => await TaskAsync());
            //t3.Wait();

            //var t4 = CustomTask.Run(async () => TaskAsync());
            //t4.Wait();

            //var t5 = CustomTask.Run(async () => {
            //    await Task.Delay(1000);
            //    Console.WriteLine($"async task, CurrentThreadId: {Environment.CurrentManagedThreadId}");
            //});
            //t5.Wait();

            Console.WriteLine($"case over, CurrentThreadId: {Environment.CurrentManagedThreadId}");
        }






    }


    internal static class CustomTask
    {

        public static Task Run(Action action)
        {
            return Task.Run(action);
        }

        /*
        public static Task Run(Func<Task> function)
        {
            return Task.Run(function);
        }*/

    }
}
