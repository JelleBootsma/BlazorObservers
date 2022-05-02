using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using BlazorObservers.ObserverLibrary.Tasks;
using BlazorObservers.ObserverLibrary.JsModels;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Service to manage resize observer registrations
    /// 
    /// With this, you can execute dotNET functions on element resize.
    /// WARNING: Sets ResizeObservationRegistrationGuid attribute on tracked elements. 
    /// If this attribute is used for a different purpose, the functionality might break
    /// </summary>
    public class ResizeObserverRegistrationService : AbstractObserverRegistrationService
    {
        private static readonly ConcurrentDictionary<Guid, ResizeTask> _registeredTasks = new();
        private static readonly ConcurrentDictionary<Guid, ElementRegistration> _registeredElements = new();

        public ResizeObserverRegistrationService(IJSRuntime jsRuntime): base(jsRuntime)
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
                task.ConnectedElementes[newRef.Key] = newRef.Value.ElementReference;
            }

            return task;
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
            ResizeTask? taskRef;
            if (_registeredTasks.Remove(id, out taskRef))
            {
                taskRef?.SelfRef.Dispose();
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
