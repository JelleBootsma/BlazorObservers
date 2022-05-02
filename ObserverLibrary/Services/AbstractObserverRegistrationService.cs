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
        /// <summary>
        /// Js runtime to use for interop
        /// </summary>
        protected readonly IJSRuntime _jsRuntime;
        /// <summary>
        /// Reference to the ES6 module
        /// </summary>
        protected readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        /// <summary>
        /// Base constructor of ObserverRegistrationServices. 
        /// 
        /// This constructor will create the task for async loading of the ES6 Module
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
            await DisposeAsyncCore();

            GC.SuppressFinalize(this);
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
    }
}