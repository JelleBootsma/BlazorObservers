using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using BlazorObservers.ObserverLibrary.Tasks;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Service to manage resize observer registrations
    /// 
    /// With this, you can execute dotNET functions on element resize
    /// </summary>
    public class ResizeObserverRegistrationService : AbstractObserverRegistrationService
    {
        private static readonly ConcurrentDictionary<Guid, ObserverTask> _registeredTasks = new();

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
        public override Task<ObserverTask> RegisterObserver(Func<Task> onObserve, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            if (targetElements is null)
                throw new ArgumentNullException(nameof(targetElements));
            

            if (targetElements.Length == 0) 
                throw new ArgumentException("At least 1 element must be observed");
            return DoObserverRegistration(onObserve, targetElements);
        }

        private async Task<ObserverTask> DoObserverRegistration(Func<Task> onObserve, params ElementReference[] targetElements)
        {
            var module = await _moduleTask.Value;
            var task = new ObserverTask(onObserve);
            _registeredTasks[task.TaskId] = task;

            var JsParams = new List<object> { task.TaskId.ToString(), task.SelfRef };
            JsParams.AddRange(targetElements.Cast<object>());

            await module.InvokeVoidAsync("ObserverManager.CreateNewResizeObserver", JsParams.ToArray());
            return task;
        }

        /// <summary>
        /// Remove a resize observer using the observerTask reference
        /// </summary>
        /// <param name="observerTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override Task DeregisterObserver(ObserverTask observerTask)
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
        public override async Task DeregisterObserver(Guid id)
        {
            var module = await _moduleTask.Value;
            if (_registeredTasks.Remove(id, out ObserverTask? taskRef))
            {
                taskRef?.SelfRef.Dispose();
            }
            await module.InvokeVoidAsync("ObserverManager.RemoveResizeObserver", id.ToString());
        }

    }
}
