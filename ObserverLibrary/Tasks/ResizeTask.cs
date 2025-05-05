using BlazorObservers.ObserverLibrary.JsModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    /// <summary>
    /// Handle for a function which is executed when an element is resized. 
    /// 
    /// Should not be created by user code. Instead the ResizeObserverRegistrationService should be used.
    /// </summary>
    public class ResizeTask : ObserverTask<JsResizeObserverEntry[]>
    {
        internal ResizeTask(Func<JsResizeObserverEntry[], ValueTask> taskFunc) : base(taskFunc)
        {
        }

        /// <summary>
        /// Method to execute when an element is resized. 
        /// 
        /// Should not be called by user code.
        /// </summary>
        /// <param name="jsData"></param>
        /// <returns></returns>
        [JSInvokable("Execute")]
        public override ValueTask Execute(JsResizeObserverEntry[] jsData)
        {
            foreach (var dataElement in jsData)
            {
                if (Guid.TryParse(dataElement.TargetElementTrackingId, out Guid trackingId) &&
                    ConnectedElements.ContainsKey(trackingId))
                {
                    dataElement.TargetElement = ConnectedElements[trackingId];
                }
            }
            return base.Execute(jsData);
        }
    }



}
