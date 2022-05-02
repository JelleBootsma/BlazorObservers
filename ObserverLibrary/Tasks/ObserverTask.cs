using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    public abstract class ObserverTask<T>
    {
        private bool _paused = false;
        protected Func<T, Task> _taskFunc;
        internal DotNetObjectReference<ObserverTask<T>> SelfRef { get; }
        public Guid TaskId { get; } = Guid.NewGuid();

        protected ObserverTask(Func<T, Task> taskFunc)
        {
            _taskFunc = taskFunc;
            SelfRef = DotNetObjectReference.Create(this);
        }

        public void HaltTaskTriggering()
        {
            _paused = true;
        }

        public void ResumeTaskTriggering()
        {
            _paused = false;
        }

        [JSInvokable("Execute")]
        public virtual async Task Execute(T jsData)
        {
            if (_paused) return;
            await _taskFunc(jsData);
        }
    }
}
