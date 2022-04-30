using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Services
{
    /// <summary>
    /// Abstract base for all ObserverRegistractionServices
    /// </summary>
    public abstract class AbstractObserverRegistrationService : IAsyncDisposable
    {
        protected readonly IJSRuntime _jsRuntime;
        protected readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        protected AbstractObserverRegistrationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/ObserverLibrary/ObserverManager.js").AsTask());
        }
        /// <summary>
        /// Remove an observer using the ObserverTask reference
        /// </summary>
        /// <param name="observerTask"></param>
        /// <returns></returns>
        public abstract Task DeregisterObserver(ObserverTask observerTask);

        /// <summary>
        /// Remove an observer using its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task DeregisterObserver(Guid id);

        /// <summary>
        /// Create and register a new observer for the given target elements
        /// </summary>
        /// <param name="onObserve"></param>
        /// <param name="targetElement"></param>
        /// <returns></returns>
        public abstract Task<ObserverTask> RegisterObserver(Func<Task> onObserve, params ElementReference[] targetElements);

        /// <summary>
        /// Dispose the JS module reference, if it has been created
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}