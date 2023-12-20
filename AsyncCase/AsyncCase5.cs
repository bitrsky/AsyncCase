using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase
{
    // CancellationTokenSource(s)
    internal class AsyncCase5
    {
        public async Task CancelWithTimeOut(int ms)
        {
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ms)))
                {
                    //cts.CancelAfter(TimeSpan.FromSeconds(10));
                    await RunAsync(cts.Token);
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine(e);
            }
        }



        public async Task CancelWithMultiToken(CancellationToken token)
        {
            try
            {
                using (var c1 = new CancellationTokenSource(TimeSpan.FromMilliseconds(10)))
                using (var c2 = new CancellationTokenSource())
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(c1.Token, c2.Token))
                {
                    await RunAsync(cts.Token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        public async Task CancelWithCallBack()
        {
            using (var cts = new CancellationTokenSource())
            {
                using (cts.Token.Register(DoSth))
                {
                    await Task.Delay(100);
                    cts.Cancel();
                }
            }
        }


        

        private void DoSth()
        {
            Console.WriteLine("enter do sth");
        }

        private async Task RunAsync(CancellationToken token)
        {
            Console.WriteLine("enter run async");
            await Task.Delay(1000, token);
            Console.WriteLine("exit run async");
        }



        //Cancelling uncancellable operations
        public async Task<T> BadCancelUncancellableTask<T>(Task<T> task, CancellationToken cancellationToken)
        {
            // There's no way to dispose of the registration
            var delayTask = Task.Delay(-1, cancellationToken);

            var resultTask = await Task.WhenAny(task, delayTask);
            if (resultTask == delayTask)
            {
                // Operation cancelled
                throw new OperationCanceledException();
            }

            return await task;
        }

        //Cancelling uncancellable operations
        public async Task<T> GoodCancelUncancellableTask<T>(Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            // This disposes the registration as soon as one of the tasks trigger
            using (cancellationToken.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            },
            tcs))
            {
                var resultTask = await Task.WhenAny(task, tcs.Task);
                if (resultTask == tcs.Task)
                {
                    // Operation cancelled
                    throw new OperationCanceledException(cancellationToken);
                }

                return await task;
            }
        }


    }
}
