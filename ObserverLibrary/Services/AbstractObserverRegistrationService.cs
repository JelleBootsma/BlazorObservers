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
               "import", "./_content/BlazorObservers/ObserverManager.js").AsTask());
        }
        

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