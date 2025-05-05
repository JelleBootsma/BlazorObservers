using BlazorObservers.ObserverLibrary.JsModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    /// <summary>
    /// Handle for a function which is executed when an element's intersection state changes.
    /// 
    /// Should not be created by user code. Instead the IntersectionObserverService should be used.
    /// </summary>
    public class IntersectionTask : OptionedTask<JsIntersectionObserverEntry[], JsIntersectionObserverOptions>
    {
        internal IntersectionTask(Func<JsIntersectionObserverEntry[], ValueTask> taskFunc, JsIntersectionObserverOptions options) : base(taskFunc, options)
        {
        }

        /// <summary>
        /// Method to execute when an element's intersection changes.
        /// 
        /// Should not be called by user code.
        /// </summary>
        /// <param name="jsData">Array of intersection entries from JavaScript</param>
        /// <returns></returns>
        [JSInvokable("Execute")]
        public override ValueTask Execute(JsIntersectionObserverEntry[] jsData)
        {
            foreach (var dataElement in jsData)
            {
                if (Guid.TryParse(dataElement.TargetElementTrackingId, out Guid trackingId) &&
                    ConnectedElements.TryGetValue(trackingId, out var element))
                {
                    dataElement.TargetElement = element;
                }
            }
            return base.Execute(jsData);
        }
    }
}
