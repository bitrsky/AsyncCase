﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase.Common
{
    /// <summary>
    ///     Represents a <see cref="TaskScheduler"/> which executes code on a dedicated, single thread whose <see cref="ApartmentState"/> can be configured.
    /// </summary>
    /// <remarks>
    ///     You can use this class if you want to perform operations on a non thread-safe library from a multi-threaded environment.
    /// </remarks>
    public sealed class SingleThreadTaskScheduler
        : TaskScheduler, IDisposable
    {
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly BlockingCollection<Task> _tasks;
        private readonly Action _initAction;

        /// <summary>
        ///     The <see cref="System.Threading.ApartmentState"/> of the <see cref="Thread"/> this <see cref="SingleThreadTaskScheduler"/> uses to execute its work.
        /// </summary>
        public ApartmentState ApartmentState { get; private set; }

        /// <summary>
        ///     Indicates the maximum concurrency level this <see cref="T:System.Threading.Tasks.TaskScheduler"/> is able to support.
        /// </summary>
        /// 
        /// <returns>
        ///     Returns <c>1</c>.
        /// </returns>
        public override int MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleThreadTaskScheduler"/>, optionally setting an <see cref="System.Threading.ApartmentState"/>.
        /// </summary>
        /// <param name="apartmentState">
        ///     The <see cref="ApartmentState"/> to use. Defaults to <see cref="System.Threading.ApartmentState.STA"/>
        /// </param>
        public SingleThreadTaskScheduler(ApartmentState apartmentState = ApartmentState.STA)
            : this(null, null, apartmentState)
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleThreadTaskScheduler"/>, optionally setting an <see cref="System.Threading.ApartmentState"/>.
        /// </summary>
        /// <param name="apartmentState">
        ///     The <see cref="ApartmentState"/> to use. Defaults to <see cref="System.Threading.ApartmentState.STA"/>
        /// </param>
        public SingleThreadTaskScheduler(string threadName, ApartmentState apartmentState = ApartmentState.STA)
            : this(threadName, null, apartmentState)
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleThreadTaskScheduler"/> passsing an initialization action, optionally setting an <see cref="System.Threading.ApartmentState"/>.
        /// </summary>
        /// <param name="initAction">
        ///     An <see cref="Action"/> to perform in the context of the <see cref="Thread"/> this <see cref="SingleThreadTaskScheduler"/> uses to execute its work after it has been started.
        /// </param>
        /// <param name="apartmentState">
        ///     The <see cref="ApartmentState"/> to use. Defaults to <see cref="System.Threading.ApartmentState.STA"/>
        /// </param>
        public SingleThreadTaskScheduler(string threadName, Action initAction, ApartmentState apartmentState = ApartmentState.STA)
        {
            if (apartmentState != ApartmentState.MTA && apartmentState != ApartmentState.STA)
                throw new ArgumentException("apartment State");

            this.ApartmentState = apartmentState;
            this._cancellationToken = new CancellationTokenSource();
            this._tasks = new BlockingCollection<Task>();
            this._initAction = initAction ?? (() => { });
            string tName;
            if (threadName == null) tName = "SingleThreadTaskScheduler";
            else tName = threadName;

            this._thread = new Thread(this.ThreadStart)
            {
                Name = tName,
                IsBackground = true
            };
            this._thread.TrySetApartmentState(apartmentState);
            this._thread.Start();
        }

        /// <summary>
        ///     Waits until all scheduled <see cref="Task"/>s on this <see cref="SingleThreadTaskScheduler"/> have executed and then disposes this <see cref="SingleThreadTaskScheduler"/>.
        /// </summary>
        /// <remarks>
        ///     Calling this method will block execution. It should only be called once.
        /// </remarks>
        /// <exception cref="TaskSchedulerException">
        ///     Thrown when this <see cref="SingleThreadTaskScheduler"/> already has been disposed by calling either <see cref="Wait"/> or <see cref="Dispose"/>.
        /// </exception>
        public void Wait()
        {
            if (this._cancellationToken.IsCancellationRequested)
                throw new TaskSchedulerException("Cannot wait after disposal.");

            this._tasks.CompleteAdding();
            this._thread.Join();

            this._cancellationToken.Cancel();
        }

        /// <summary>
        ///     Disposes this <see cref="SingleThreadTaskScheduler"/> by not accepting any further work and not executing previously scheduled tasks.
        /// </summary>
        /// <remarks>
        ///     Call <see cref="Wait"/> instead to finish all queued work. You do not need to call <see cref="Dispose"/> after calling <see cref="Wait"/>.
        /// </remarks>
        public void Dispose()
        {
            if (this._cancellationToken.IsCancellationRequested)
                return;

            try
            {
                this._tasks.CompleteAdding();
                this._cancellationToken.Cancel();
            }
            catch (InvalidOperationException) { }
        }

        protected override void QueueTask(Task task)
        {
            // New task cannot be added to Queue after Cancel, but no exception will be thrown
            if (this._cancellationToken.IsCancellationRequested)
                return;

            try
            {
                this._tasks.Add(task, this._cancellationToken.Token);
            }
            catch { }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            this.VerifyNotDisposed();

            if (this._thread != Thread.CurrentThread)
                return false;
            if (this._cancellationToken.Token.IsCancellationRequested)
                return false;
            this.TryExecuteTask(task);
            return true;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            this.VerifyNotDisposed();

            return this._tasks.ToArray();
        }

        private void ThreadStart()
        {
            try
            {
                var token = this._cancellationToken.Token;

                this._initAction();

                foreach (var task in this._tasks.GetConsumingEnumerable(token))
                {
                    this.TryExecuteTask(task);
                }
                    
            }
            catch (OperationCanceledException) { }
            catch (InvalidOperationException) { }
            catch (Exception) { }
            finally
            {
                this._tasks.Dispose();
            }
        }

        private void VerifyNotDisposed()
        {
            if (this._cancellationToken.IsCancellationRequested)
                throw new ObjectDisposedException(typeof(SingleThreadTaskScheduler).Name);
        }
    }
}
