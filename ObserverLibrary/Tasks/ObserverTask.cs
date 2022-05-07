using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    /// <summary>
    /// Abstract base class for handles of C# task, which is executed after observer trigger.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObserverTask<T>
    {
        private bool _paused = false;
        private protected Func<T, Task> _taskFunc;
        internal DotNetObjectReference<ObserverTask<T>> SelfRef { get; }
        /// <summary>
        /// Unique identifier of this Task
        /// </summary>
        public Guid TaskId { get; } = Guid.NewGuid();

        private protected ObserverTask(Func<T, Task> taskFunc)
        {
            _taskFunc = taskFunc;
            SelfRef = DotNetObjectReference.Create(this);
        }
        /// <summary>
        /// Stop triggering of C# task
        /// </summary>
        public void HaltTaskTriggering()
        {
            _paused = true;
        }

        /// <summary>
        /// Result triggering of C# task
        /// </summary>
        public void ResumeTaskTriggering()
        {
            _paused = false;
        }

        /// <summary>
        /// Task which is executed on observer trigger. 
        /// 
        /// Checks for paused status and then executes taskFunc if not paused
        /// </summary>
        /// <param name="jsData"></param>
        /// <returns></returns>
        [JSInvokable("Execute")]
        public virtual async Task Execute(T jsData)
        {
            if (_paused) return;
            await _taskFunc(jsData);
        }
    }
}
