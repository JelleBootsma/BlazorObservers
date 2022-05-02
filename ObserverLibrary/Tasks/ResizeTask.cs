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
        internal Dictionary<Guid, ElementReference> ConnectedElementes { get; set; }
        internal ResizeTask(Func<JsResizeObserverEntry[], Task> taskFunc) : base(taskFunc)
        {
            ConnectedElementes = new Dictionary<Guid, ElementReference>();
        }

        /// <summary>
        /// Method to execute when an element is resized. 
        /// 
        /// Should not be called by user code.
        /// </summary>
        /// <param name="jsData"></param>
        /// <returns></returns>
        [JSInvokable("Execute")]
        public override Task Execute(JsResizeObserverEntry[] jsData)
        {
            foreach (var dataElement in jsData)
            {
                if (Guid.TryParse(dataElement.TargetElementTrackingId, out Guid trackingId) &&
                    ConnectedElementes.ContainsKey(trackingId))
                {
                    dataElement.TargetElement = ConnectedElementes[trackingId];
                }
            }
            return base.Execute(jsData);
        }
    }



}
