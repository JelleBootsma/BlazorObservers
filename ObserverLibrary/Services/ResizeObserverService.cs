using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Service to manage resize observer registrations
    /// 
    /// With this, you can execute dotNET functions on element resize. 
    /// 
    /// </summary>
    public class ResizeObserverService : AbstractObserverService
    {
        private static readonly ConcurrentDictionary<Guid, ResizeTask> _registeredTasks = new();
        private static readonly ConcurrentDictionary<Guid, ElementRegistration> _registeredElements = new();

        /// <summary>
        /// Constructor of a ResizeObserverRegistrationService. 
        /// 
        /// Should not be used by user code, but service should be injected using Dependency Injection.
        /// </summary>
        /// <param name="jsRuntime"></param>
        public ResizeObserverService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        /// <summary>
        /// Register a function to execute on resize of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on resize</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<ResizeTask> RegisterObserver(Action<JsResizeObserverEntry[]> onObserve, params ElementReference[] targetElements)
        {
            return RegisterObserver((entries) => { onObserve(entries); return Task.CompletedTask; }, targetElements);
        }

        /// <summary>
        /// Register a function to execute on resize of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on resize</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<ResizeTask> RegisterObserver(Func<JsResizeObserverEntry[], Task> onObserve, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            if (targetElements is null)
                throw new ArgumentNullException(nameof(targetElements));


            if (targetElements.Length == 0)
                throw new ArgumentException("At least 1 element must be observed");
            return DoObserverRegistration(onObserve, targetElements);
        }

        private async Task<ResizeTask> DoObserverRegistration(Func<JsResizeObserverEntry[], Task> onObserve, params ElementReference[] targetElements)
        {
            var module = await _moduleTask.Value;
            var task = new ResizeTask(onObserve);
            _registeredTasks[task.TaskId] = task;

            var JsParams = new List<object> { task.TaskId.ToString(), task.SelfRef };
            JsParams.AddRange(targetElements.Cast<object>());

            var stringIds = await module.InvokeAsync<string[]>("ObserverManager.CreateNewResizeObserver", JsParams.ToArray());
            var uniqueIds = stringIds.Select(x => Guid.Parse(x));

            var newReferences = uniqueIds.Zip(targetElements, (id, el) => new KeyValuePair<Guid, ElementRegistration>(id, new ElementRegistration(el, task)));

            foreach (var newRef in newReferences)
            {
                _registeredElements[newRef.Key] = newRef.Value;
                task.ConnectedElements[newRef.Key] = newRef.Value.ElementReference;
            }

            return task;
        }

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
        public Task<bool> StartObserving(ResizeTask observerTask, ElementReference newElement)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));
            if (observerTask.ConnectedElements.Any(x => x.Value.Equals(newElement)))
                return Task.FromResult(true); // Element is already observed

            return DoStartObserving(observerTask, newElement);
        }

        private async Task<bool> DoStartObserving(ResizeTask observerTask, ElementReference element)
        {
            var module = await _moduleTask.Value;
            var newTrackingIdString = await module.InvokeAsync<string?>("ObserverManager.StartObserving", observerTask.TaskId.ToString(), element);
            if (newTrackingIdString is null || !Guid.TryParse(newTrackingIdString, out Guid newTrackingId)) return false;
            var elRegistration = new ElementRegistration(element, observerTask);
            _registeredElements[newTrackingId] = elRegistration;
            observerTask.ConnectedElements[newTrackingId] = element;
            return true;
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
        public Task<bool> StopObserving(ResizeTask observerTask, ElementReference element)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));
            if (!observerTask.ConnectedElements.Any(x => x.Value.Equals(element)))
                return Task.FromResult(false);

            return DoStopObserving(observerTask, element);
        }


        private async Task<bool> DoStopObserving(ResizeTask observerTask, ElementReference element)
        {
            var module = await _moduleTask.Value;
            var successRemovedByJs = await module.InvokeAsync<bool>("ObserverManager.StopObserving", observerTask.TaskId.ToString(), element);

            // Remove registration
            var toRemove = _registeredElements.Where(x => x.Value.ElementReference.Equals(element) && x.Value.TaskReference == observerTask);
            foreach (var reg in toRemove)
            {
                _registeredElements.Remove(reg.Key, out _);
            }
            var toRemoveFromTask = observerTask.ConnectedElements.Where(x => x.Value.Equals(element));
            foreach (var connectedElement in toRemoveFromTask)
            {
                observerTask.ConnectedElements.Remove(connectedElement.Key, out _);
            }
            return successRemovedByJs;
        }

        /// <summary>
        /// Remove a resize observer using the observerTask reference
        /// </summary>
        /// <param name="observerTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task DeregisterObserver(ResizeTask observerTask)
        {
            if (observerTask is null)
                throw new ArgumentNullException(nameof(observerTask));


            return DeregisterObserver(observerTask.TaskId);
        }

        /// <summary>
        /// Remove a resize observer using the TaskId
        /// </summary>
        /// <param name="id">Id of the ObserverTask to remove</param>
        /// <returns></returns>
        public async Task DeregisterObserver(Guid id)
        {
            var module = await _moduleTask.Value;
            if (_registeredTasks.Remove(id, out ResizeTask? taskRef))
            {
                taskRef?.SelfRef.Dispose();
                taskRef?.ConnectedElements.Clear();
            }
            await module.InvokeVoidAsync("ObserverManager.RemoveResizeObserver", id.ToString());
            if (taskRef is not null)
            {
                var toRemove = _registeredElements.Where(x => x.Value.TaskReference == taskRef);
                foreach (var registration in toRemove)
                {
                    _registeredElements.Remove(registration.Key, out _);
                }
            }

        }

    }
    internal record ElementRegistration(ElementReference ElementReference, ResizeTask TaskReference);
}
