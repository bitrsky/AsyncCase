using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCase.Common
{
    public interface IAppLifetime
    {
        CancellationToken ApplicationStarted { get; }

        CancellationToken ApplicationStopping { get; }

        CancellationToken ApplicationStopped { get; }

        void StopApplication();
    }
    public interface IAppContext
    {

        IAppLifetime AppLifetime { get; }

        DateTime StartTime { get; }
    }

    public static class ControllableTask
    {
        /// <summary>
        /// Can be used to run a task in the background. The task will be removed from the list when it completes.
        /// </summary>
        public static Task Run(Action action, IAppContext context = null, CancellationToken cancellationToken = default)
        {
            return TaskManager.Instance.RunCore((token) => Task.Run(action, token), null, context, cancellationToken);
        }

        /*
        public static Task Run(Func<Task> function, IAppContext context = null, CancellationToken cancellationToken = default)
        {
            return TaskManager.Instance.RunCore((token) => Task.Run(function, token), null, context, cancellationToken);
        }*/

        /// <summary>
        /// Can be used to run a task in the background. The task will be removed from the list when it completes.
        /// You can use the task name to identify the task in the list.
        /// </summary>
        public static Task Run(Action action, string taskName, IAppContext context = null,
            CancellationToken cancellationToken = default)
        {
            return TaskManager.Instance.RunCore((token) => Task.Run(action, token), taskName, context, cancellationToken);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function, string taskName,
            IAppContext context = null,
            CancellationToken cancellationToken = default)
        {
            return TaskManager.Instance.RunCore((token) => Task.Run(function, token), taskName, context, cancellationToken);
        }
    }

    public class TaskManager
    {
        readonly Dictionary<int, string> _taskNames =
            new Dictionary<int, string>();
        readonly Dictionary<int, WeakReference<Task>> _tasks =
            new Dictionary<int, WeakReference<Task>>();
        readonly object _lock = new object();

        public static TaskManager Instance => TaskManagerHolder.Instance;

        static class TaskManagerHolder
        {
            internal static readonly TaskManager Instance = new TaskManager();
        }

        public Task Run(Action action, string name = null,
            IAppContext appContext = null,
            CancellationToken cancellationToken = default)
        {
            return RunCore((token) => Task.Run(action, token), name, appContext, cancellationToken);
        }

        public Task<TResult> Run<TResult>(Func<TResult> function, string name = null,
            IAppContext appContext = null,
           CancellationToken cancellationToken = default)
        {
            return RunCore((token) => Task.Run(function, token), name, appContext, cancellationToken);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:丢失范围之前释放对象", Justification = "<挂起>")]
        public TResult RunCore<TResult>(
            Func<CancellationToken, TResult> action,
            string name = null,
            IAppContext appContext = null,
            CancellationToken cancellationToken = default)
            where TResult : Task
        {
            CancellationTokenSource cts = null;
            var applicationStopping = appContext?.AppLifetime?.ApplicationStopping;
            if (applicationStopping != null
                && applicationStopping.Value != default && applicationStopping.Value != cancellationToken)
            {
                cts = CancellationTokenSource
                               .CreateLinkedTokenSource(cancellationToken, applicationStopping.Value);
            }

            var task = action?.Invoke(cts?.Token ?? cancellationToken);

            lock (_lock)
            {
                var taskRef = new WeakReference<Task>(task);
                _tasks.Add(task.Id, taskRef);

                if (name != null)
                {
                    _taskNames.Add(task.Id, name);
                }

                // Add a continuation to remove the task from the list when it completes
                task.ContinueWith(t =>
                {
                    cts?.Dispose();
                    lock (_lock)
                    {
                        _tasks.Remove(t.Id);
                        _taskNames.Remove(t.Id);
                    }

                    if (t.IsFaulted)
                    { // Log it if the task faulted.
                        var taskName = name ?? t.Id.ToString(CultureInfo.InvariantCulture);
                    }
                }, TaskScheduler.Current);
            }

            return task;
        }

        public async Task WaitForAllTasksAsync(CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            lock (_lock)
            {
                foreach (var kv in _tasks.ToArray())
                {
                    if (kv.Value.TryGetTarget(out var t))
                    {
                        tasks.Add(t);
                    }
                }
            }

            if (!tasks.Any())
                return;

            var delayTask = Task.Delay(Timeout.Infinite, cancellationToken);
            var completedTask = await Task.WhenAny(Task.WhenAll(tasks.ToArray()), delayTask)
                .ConfigureAwait(false);

            if (completedTask == delayTask)
            {
                foreach (var task in tasks)
                {
                    if (!task.IsCompleted)
                    {
                        if (task.AsyncState is CancellationTokenSource cts)
                        {
                            cts.Cancel();
                        }

                        if (!_taskNames.TryGetValue(task.Id, out var name))
                            name = task.Id.ToString(CultureInfo.InvariantCulture);

                    }
                }
            }
        }
    }
}
