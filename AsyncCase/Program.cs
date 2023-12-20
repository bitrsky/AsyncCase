using System;

namespace AsyncCase
{
    internal class Program
    {
        static void Main()
        {
            // sync over async
            // SynchronizationContext 
            // var ac1 = new AsyncCase1();
            // ac1.CaseWithBlock();
            // ac1.CaseNoBlock();
            // ac1.CaseAwaitAsync();
            // ac1.PreferAwaitOverTask();

            // sync over async
            // SingleThreadTaskScheduler 
            // var ac7 = new AsyncCase7();
            // ac7.CaseWithBlock();
            // ac7.CaseNoBlock();
            // ac7.CaseAwaitAsync();


            // async void
            // var ac2 = new AsyncCase2();
            // ac2.TimeAsyncVoid();
            // ac2.CatchAsyncVoidException();


            // Task.Run
            // Action & Func<Task>
            // var ac3 = new AsyncCase3();
            // ac3.TaskRun();
            // ac3.CustomTaskRun();


            // await vs ContinueWith
            // var ac4 = new AsyncCase4();
            // ac4.CaseAwaitAsyncWithSynchronizationContext();
            // ac4.CaseContinueWithAsyncWithSynchronizationContext();
            // ac4.CaseAwaitAsyncWithSingleThreadTaskScheduler();
            // ac4.CaseContinueWithAsyncWithSingleThreadTaskScheduler();

            // Prefer `async`/`await` over directly returning `Task`
            // var ac8 = new AsyncCase8();
            // ac8.TaskThrowException();
            // ac8.TaskUseUsing();

            // CancellationTokenSource
            //var ac5 = new AsyncCase5();
            //_ = ac5.CancelWithTimeOut(1100);
            //_ = ac5.CancelWithCallBack();

            Console.ReadKey();
        }
    }
}