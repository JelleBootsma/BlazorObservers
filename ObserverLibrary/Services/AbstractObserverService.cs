using System.Collections.Concurrent;
using BlazorObservers.ObserverLibrary.Models;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Abstract base for all ObserverRegistractionServices
    /// </summary>
    public abstract class AbstractObserverService<TTask, TEntry> : IAsyncDisposable where TTask : ObserverTask<TEntry[]>
    {
        /// <summary>
        /// Static parser for Guid to avoid allocations
        /// </summary>
        protected static readonly Func<string, Guid> _guidParser = Guid.Parse;
        /// <summary>
        /// Js runtime to use for interop
        /// </summary>
        protected readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// Reference to the ES6 module
        /// </summary>
        protected readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        /// <summary>
        /// Dictionary to store the registered tasks managed by this service
        /// </summary>
        protected private readonly ConcurrentDictionary<Guid, TTask> _registeredTasks = new();

        /// <summary>
        /// Dictionary to store the registered elements managed by this service
        /// </summary>
        protected private readonly ConcurrentDictionary<Guid, ElementRegistration<TTask, TEntry>> _registeredElements = new();


        /// <summary>
        /// Base constructor of ObserverRegistrationServices. 
        /// 
        /// This constructor will create the task for async loading of the ES6 Module
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected AbstractObserverService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/BlazorObservers/ObserverManager.js").AsTask());
        }
        

        /// <summary>
        /// Dispose the JS module reference, if it has been created
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this);
        }


        protected virtual Task<TTask> ValidateObserverRegistration(Func<TEntry[], ValueTask> onObserve, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            if (targetElements is null)
                throw new ArgumentNullException(nameof(targetElements));

            if (targetElements.Length == 0)
                throw new ArgumentException("At least 1 element must be observed");

            return DoObserverRegistration(onObserve, targetElements);
        }

        /// <summary>
        /// Register an async function to execute on trigger of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on trigger</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<TTask> RegisterObserver(Func<TEntry[], Task> onObserve, params ElementReference[] targetElements)
        {
            return ValidateObserverRegistration(async (entries) => await onObserve(entries), targetElements);
        }

        /// <summary>
        /// Register a synchronous function to execute on trigger of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on trigger</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<TTask> RegisterObserver(Action<TEntry[]> onObserve, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            return ValidateObserverRegistration((entries) => { onObserve(entries); return ValueTask.CompletedTask; }, targetElements);
        }

        protected abstract Task<TTask> DoObserverRegistration(Func<TEntry[], ValueTask> onObserve, params ElementReference[] targetElements);

        /// <summary>
        /// Add a specified element to the list of observed elements of a specific task.
        /// 
        /// If the element is not successfully added return false.
        /// Otherwise return true.
        /// </summary>
        /// <param name="observerTask"></param>
        /// <param name="newElement"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If observerTask is null</exception>
        public Task<bool> StartObserving(TTask observerTask, ElementReference newElement)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));
            if (observerTask.ConnectedElements.Any(x => EqualityComparer<ElementReference>.Default.Equals(x.Value, newElement)))
                return Task.FromResult(true); // Element is already observed

            return DoStartObserving(observerTask, newElement);
        }



        /// <summary>
        /// Remove a specified element from the list of observed elements of a specific task.
        /// 
        /// If the element is successfully remove, and was present in the observed elements list, return true.
        /// Otherwise, return false.
        /// </summary>
        /// <param name="observerTask"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If observerTask is null</exception>
        public Task<bool> StopObserving(TTask observerTask, ElementReference element)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));
            if (!observerTask.ConnectedElements.Any(x => EqualityComparer<ElementReference>.Default.Equals(x.Value, element)))
                return Task.FromResult(false);

            return DoStopObserving(observerTask, element);
        }

        /// <summary>
        /// Perform actual Disposing
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        protected async Task<bool> DoStartObserving(TTask observerTask, ElementReference element)
        {
            var module = await _moduleTask.Value;
            var newTrackingIdString = await module.InvokeAsync<string?>("ObserverManager.StartObserving", observerTask.TaskId.ToString(), element);
            if (newTrackingIdString is null || !Guid.TryParse(newTrackingIdString, out Guid newTrackingId)) return false;
            var elRegistration = new ElementRegistration<TTask, TEntry>(element, observerTask);
            _registeredElements[newTrackingId] = elRegistration;
            observerTask.ConnectedElements[newTrackingId] = element;
            return true;
        }


        protected async Task<bool> DoStopObserving(TTask observerTask, ElementReference element)
        {
            var module = await _moduleTask.Value;
            var successRemovedByJs = await module.InvokeAsync<bool>("ObserverManager.StopObserving", observerTask.TaskId.ToString(), element);

            // Remove registration
            var toRemove = _registeredElements.Where(x => 
                EqualityComparer<ElementReference>.Default.Equals(x.Value.ElementReference, element) && 
                x.Value.TaskReference == observerTask);
            foreach (var reg in toRemove)
            {
                _registeredElements.Remove(reg.Key, out _);
            }
            var toRemoveFromTask = observerTask.ConnectedElements.Where(x => EqualityComparer<ElementReference>.Default.Equals(x.Value, element));
            foreach (var connectedElement in toRemoveFromTask)
            {
                observerTask.ConnectedElements.Remove(connectedElement.Key, out _);
            }
            return successRemovedByJs;
        }

        /// <summary>
        /// Remove a registered observer using the TaskId
        /// </summary>
        /// <param name="id">Id of the ObserverTask to remove</param>
        /// <returns></returns>
        public async Task DeregisterObserver(Guid id)
        {
            var module = await _moduleTask.Value;
            if (_registeredTasks.Remove(id, out TTask? taskRef))
            {
                taskRef?.SelfRef.Dispose();
                taskRef?.ConnectedElements.Clear();
            }
            await module.InvokeVoidAsync("ObserverManager.RemoveObserver", id.ToString());
            if (taskRef is not null)
            {
                var toRemove = _registeredElements.Where(x => x.Value.TaskReference == taskRef);
                foreach (var registration in toRemove)
                {
                    _registeredElements.Remove(registration.Key, out _);
                }
            }

        }

        /// <summary>
        /// Remove a resize observer using the observerTask reference
        /// </summary>
        /// <param name="observerTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task DeregisterObserver(TTask observerTask)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));


            return DeregisterObserver(observerTask.TaskId);
        }
    }
}