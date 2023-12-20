using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    //  Prefer `async`/`await` over directly returning `Task`
    internal class AsyncCase8
    {

        public async Task TaskUseUsing()
        {
            try
            {
                //var t1 = DoSthAsyncTaskWithCancel();
                //await t1;

                var t2 = DoSthAsyncAwaitWithCancel();
                await t2;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }

        private Task DoSthAsyncTaskWithCancel()
        {
            using (var c = new CancellationTokenSource(1000))
            {
                return DoSthAsync(c.Token);
            }
        }

        private async Task DoSthAsyncAwaitWithCancel()
        {
            using (var c = new CancellationTokenSource(1000))
            {
                await DoSthAsync(c.Token);
            }
        }
        


        public async Task TaskThrowException()
        {
            try
            {
                var t1 = DoSthAsyncTask();
                await t1;

                //var t2 = DoSthAsyncAwait();
                //await t2;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }


        private async Task DoSthAsync(CancellationToken token)
        {
            var sw = Stopwatch.StartNew();
            await Task.Delay(2000, token);
            Console.WriteLine($"do sth over, cost:{sw.ElapsedMilliseconds}");
        }


        private Task DoSthAsyncTask()
        {
            throw new ArgumentException(nameof(DoSthAsync));
            return DoSthAsync();
        }

        private async Task DoSthAsyncAwait()
        {
            throw new ArgumentException(nameof(DoSthAsync));
            await DoSthAsync();
        }

        private async Task DoSthAsync()
        {
            await Task.Delay(100);
        }
    }
}
