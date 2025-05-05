using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Models;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Service to manage intersection observer registrations.
    /// 
    /// With this, you can execute dotNET functions when elements intersect the viewport or a parent element.
    /// </summary>
    public class IntersectionObserverService : OptionedObserverService<IntersectionTask, JsIntersectionObserverEntry, JsIntersectionObserverOptions>
    {
        /// <summary>
        /// Constructor of an IntersectionObserverRegistrationService. 
        /// 
        /// Should not be used by user code, but service should be injected using Dependency Injection.
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IntersectionObserverService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        protected override Task<IntersectionTask> ValidateObserverRegistration(
            Func<JsIntersectionObserverEntry[], ValueTask> onObserve,
            JsIntersectionObserverOptions options,
            params ElementReference[] targetElements)
        {
            if (options.Threshold?.Any(t => t < 0 || t > 1) ?? false)
                throw new ArgumentOutOfRangeException(nameof(options.Threshold), "All threshold values must be between 0 and 1");
            return base.ValidateObserverRegistration(onObserve, options, targetElements);
        }

        protected override async Task<IntersectionTask> DoObserverRegistration(
            Func<JsIntersectionObserverEntry[], ValueTask> onObserve,
            JsIntersectionObserverOptions options,
            params ElementReference[] targetElements)
        {
            var module = await _moduleTask.Value;
            var task = new IntersectionTask(onObserve, options);
            _registeredTasks[task.TaskId] = task;


            object[] jsParams = [task.TaskId.ToString(), task.SelfRef, options, .. targetElements];

            var stringIds = await module.InvokeAsync<string[]>("ObserverManager.CreateNewIntersectionObserver", jsParams);
            var uniqueIds = stringIds.Select(_guidParser); // Reuse static delegate to avoid allocation

            var newReferences = uniqueIds.Zip(
                targetElements,
                (id, el) => new KeyValuePair<Guid, ElementRegistration<IntersectionTask, JsIntersectionObserverEntry>>(
                    id,
                    new ElementRegistration<IntersectionTask, JsIntersectionObserverEntry>(el, task)));

            foreach (var newRef in newReferences)
            {
                _registeredElements[newRef.Key] = newRef.Value;
                task.ConnectedElements[newRef.Key] = newRef.Value.ElementReference;
            }

            return task;
        }

    }

}
