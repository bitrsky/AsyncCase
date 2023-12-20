using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AsyncCase
{
    // async void
    internal class AsyncCase2
    {
        public void TimeAsyncVoid()
        {
            Console.WriteLine("Timing...");
            Stopwatch sw = Stopwatch.StartNew();
            DoSthAsyncVoid();
            Console.WriteLine($"...done timing: {sw.Elapsed}");
        }

        private async void DoSthAsyncVoid()
        {
            Console.WriteLine("Enter");
            await Task.Delay(1000);
            Console.WriteLine("Exit");
        }


        public async Task CatchAsyncVoidException()
        {
            Console.WriteLine("start...");
            try
            {
                await DoSthAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine($"end...");
        }


        private async Task DoSthAsync()
        {
            Console.WriteLine("Enter DoSthAsync");
            DoSthAsyncVoidException();
            await Task.Delay(1000);
        }

        private async void DoSthAsyncVoidException()
        {
            Console.WriteLine("Enter DoSthAsyncVoidException");
            await Task.Delay(1000);
            // Unhandled Exception
            throw new Exception("do sth async void exception");
        }

    }
}
