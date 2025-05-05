using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace BlazorObservers.ObserverLibrary.JsModels
{
    /// <summary>
    /// Model for callback parameter of the ResizeObserver
    /// </summary>
    public class JsResizeObserverEntry
    {
        /// <summary>
        /// A JsDomRect object containing the new size of the observed element when the callback is run.
        /// 
        /// Better supported than BorderBoxSize and ContentBoxSize.
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentRect
        /// </summary>
        [JsonPropertyName("contentRect")]
        public JsDomRect ContentRect { get; set; }

        /// <summary>
        /// An object containing the new border box size of the observed element when the callback is run.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/borderBoxSize
        /// </summary>
        [JsonPropertyName("borderBoxSize")]
        public JsResizeObserverSize BorderBoxSize { get; set; }

        /// <summary>
        /// An object containing the new content box size of the observed element when the callback is run.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentBoxSize
        /// </summary>
        [JsonPropertyName("contentBoxSize")]
        public JsResizeObserverSize ContentBoxSize { get; set; }

        /// <summary>
        /// Element correspoding to the sizes in ContentRect, BorderBoxSize and ContentBoxSize
        /// </summary>
        public ElementReference? TargetElement { get; set; }
        
        /// <summary>
        /// Tracking Id to match element from JS to ElementReference in C#
        /// </summary>
        [JsonPropertyName("targetTrackingId")]
        public string? TargetElementTrackingId { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member