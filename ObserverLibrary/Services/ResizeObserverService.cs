using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Models;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
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
    public sealed class ResizeObserverService : AbstractObserverService<ResizeTask, JsResizeObserverEntry>
    {

        /// <summary>
        /// Constructor of a ResizeObserverRegistrationService. 
        /// 
        /// Should not be used by user code, but service should be injected using Dependency Injection.
        /// </summary>
        /// <param name="jsRuntime"></param>
        public ResizeObserverService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        protected override async Task<ResizeTask> DoObserverRegistration(Func<JsResizeObserverEntry[], ValueTask> onObserve, params ElementReference[] targetElements)
        {
            var module = await _moduleTask.Value;
            var task = new ResizeTask(onObserve);
            _registeredTasks[task.TaskId] = task;

            object[] jsParams = [task.TaskId.ToString(), task.SelfRef, .. targetElements];

            var stringIds = await module.InvokeAsync<string[]>("ObserverManager.CreateNewResizeObserver", jsParams);
            var uniqueIds = stringIds.Select(_guidParser);

            var newReferences = uniqueIds.Zip(
                targetElements, 
                (id, el) => new KeyValuePair<Guid, ElementRegistration<ResizeTask, JsResizeObserverEntry>>(
                    id, 
                    new ElementRegistration<ResizeTask, JsResizeObserverEntry>(el, task)));

            foreach (var newRef in newReferences)
            {
                _registeredElements[newRef.Key] = newRef.Value;
                task.ConnectedElements[newRef.Key] = newRef.Value.ElementReference;
            }

            return task;
        }
    }

}
