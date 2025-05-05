using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Services
{
    public abstract class OptionedObserverService<TTask, TEntry, TOptions> : AbstractObserverService<TTask, TEntry> where TTask : ObserverTask<TEntry[]> where TOptions : new()
    {
        protected OptionedObserverService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        protected virtual Task<TTask> ValidateObserverRegistration(Func<TEntry[], ValueTask> onObserve, TOptions options, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            if (targetElements is null)
                throw new ArgumentNullException(nameof(targetElements));

            if (targetElements.Length == 0)
                throw new ArgumentException("At least 1 element must be observed");

            return DoObserverRegistration(onObserve, options, targetElements);
        }

        /// <summary>
        /// Register an async function to execute on trigger of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on trigger</param>
        /// <param name="options">Options to add to this observer</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<TTask> RegisterObserver(Func<TEntry[], Task> onObserve, TOptions options, params ElementReference[] targetElements)
        {
            return ValidateObserverRegistration(async (entries) => await onObserve(entries), options, targetElements);
        }

        /// <summary>
        /// Register a synchronous function to execute on trigger of any one of the elements
        /// </summary>
        /// <param name="onObserve">Function to execute on trigger</param>
        /// <param name="options">Options to add to this observer</param>
        /// <param name="targetElements">Elements to observe</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null</exception>
        /// <exception cref="ArgumentException">Thrown if targetElements is an empty array</exception>
        public Task<TTask> RegisterObserver(Action<TEntry[]> onObserve, TOptions options, params ElementReference[] targetElements)
        {
            if (onObserve is null)
                throw new ArgumentNullException(nameof(onObserve));

            return ValidateObserverRegistration((entries) => { onObserve(entries); return ValueTask.CompletedTask; }, options, targetElements);
        }

        protected abstract Task<TTask> DoObserverRegistration(Func<TEntry[], ValueTask> onObserve, TOptions options, params ElementReference[] targetElements);


        protected override Task<TTask> DoObserverRegistration(Func<TEntry[], ValueTask> onObserve, params ElementReference[] targetElements) =>
            DoObserverRegistration(onObserve, new TOptions(), targetElements);

    }
}
