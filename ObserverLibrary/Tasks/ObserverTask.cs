using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    /// <summary>
    /// Abstract base class for handles of C# task, which is executed after observer trigger.
    /// </summary>
    /// <typeparam name="T">The type of data provided to callback delegate on observer trigger.</typeparam>
    public abstract class ObserverTask<T> 
    {
        private bool _paused = false;
        private bool _delayTriggering = false;
        private int _delay = 0;
        private ulong _executionCount = 0;

        private protected Func<T, ValueTask> _taskFunc;
        internal DotNetObjectReference<ObserverTask<T>> SelfRef { get; }
        /// <summary>
        /// Unique identifier of this Task
        /// </summary>
        public Guid TaskId { get; } = Guid.NewGuid();
        internal Dictionary<Guid, ElementReference> ConnectedElements { get; } = new Dictionary<Guid, ElementReference>();

        private protected ObserverTask(Func<T, ValueTask> taskFunc)
        {
            _taskFunc = taskFunc;
            SelfRef = DotNetObjectReference.Create(this);
        }

        /// <summary>
        /// If enabled, a delay of the specified amount of ms will be awaited. 
        /// Then the delegate will only be executed, if the trigger is still the latest trigger.
        /// 
        /// This means that in during high-frequency resizing, the callback will be executed only once.
        /// 
        /// Use ResumeTaskTriggering() to resume regular task triggering.
        /// </summary>
        /// <param name="delay"></param>
        /// <exception cref="ArgumentException"></exception>
        public void OnlyTriggerLast(int delay)
        {
            if (delay < 0) throw new ArgumentException($"{nameof(delay)} must be positive");
            _paused = false;
            _delay = delay;
            _delayTriggering = true;
        }

        /// <summary>
        /// Stop triggering of C# task
        /// 
        /// Triggering of task can be resumed using ResumeTaskTriggering()
        /// </summary>
        public void HaltTaskTriggering()
        {
            _paused = true;
        }

        /// <summary>
        /// Resume regular triggering of C# task
        /// </summary>
        public void ResumeTaskTriggering()
        {
            _paused = false;
            _delayTriggering = false;
        }

        /// <summary>
        /// Task which is executed on observer trigger. 
        /// 
        /// Checks for paused status and then executes taskFunc if not paused
        /// </summary>
        /// <param name="jsData"></param>
        /// <returns></returns>
        [JSInvokable("Execute")]
        public virtual async ValueTask Execute(T jsData)
        {
            if (_paused) return;
            if (!_delayTriggering)
            {
                await _taskFunc(jsData);
                return;
            }
            _executionCount++;
            ulong runNumber = _executionCount;

            // Wait for the delay
            await Task.Delay(_delay);

            // Check if this execution is still the latest run
            if (runNumber < _executionCount) return;

            // Execute
            await _taskFunc(jsData);
        }
    }
}
