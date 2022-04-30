using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    public class ObserverTask
    {
        private bool _paused = false;
        private readonly Func<Task> _taskFunc;
        internal DotNetObjectReference<ObserverTask> SelfRef { get; }
        public Guid TaskId { get; } = Guid.NewGuid();

        public ObserverTask(Func<Task> taskFunc)
        {
            _taskFunc = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
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
        public async Task Execute()
        {
            if (_paused) return;
            await _taskFunc();
        }
    }
}
