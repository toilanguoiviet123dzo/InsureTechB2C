using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cores.Helpers
{
    public static class TaskHelper
    {
        //
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);
        /// <summary>
        /// Runs a TPL Task fire-and-forget style, the right way - in the
        /// background, separate from the current thread, with no risk
        /// of it trying to rejoin the current thread.
        /// </summary>
        public static void RunBg(Func<Task> fn)
        {
            try
            {
                Task.Run(fn).ConfigureAwait(false);
            }
            catch { }
        }

        /// <summary>
        /// Runs a task fire-and-forget style and notifies the TPL that this
        /// will not need a Thread to resume on for a long time, or that there
        /// are multiple gaps in thread use that may be long.
        /// Use for example when talking to a slow webservice.
        /// </summary>
        public static void RunBgLong(Func<Task> fn)
        {
            try
            {
                Task.Factory.StartNew(fn, TaskCreationOptions.LongRunning)
                .ConfigureAwait(false);
            }
            catch { }
        }
        /// <summary>
        /// Array params of action
        /// </summary>
        /// <param name="onComplete">On complete action</param>
        /// <param name="errorHandler">Error handler</param>
        /// <param name="actions"></param>
        public static void RunAsync(Action onComplete, Action<Exception> errorHandler, params Action[] actions)
        {
            if (actions.Length == 0)
            {
                //what to do when no actions/tasks provided?
                onComplete();
                return;
            }

            List<Task> tasks = new List<Task>(actions.Length);
            foreach (var action in actions)
            {
                Task task = new Task(action);
                task.ContinueWith(t => errorHandler(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                tasks.Add(task);
            }

            //last task calls onComplete
            tasks[actions.Length - 1].ContinueWith(t => onComplete(), TaskContinuationOptions.OnlyOnRanToCompletion);

            //wire all tasks to execute the next one, except of course, the last task
            for (int i = 0; i <= actions.Length - 2; i++)
            {
                var nextTask = tasks[i + 1];
                tasks[i].ContinueWith(t => nextTask.Start(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            tasks[0].Start();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
              _myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
