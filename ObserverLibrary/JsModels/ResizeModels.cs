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
        [JsonPropertyName("targetTrackingId")]
        public string? TargetElementTrackingId { get; set; }
    }
    /// <summary>
    /// Model for ResizeObserverSize javascript models
    /// </summary>
    public struct JsResizeObserverSize {
        /// <summary>
        /// The length of the observed element's content box in the block dimension.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentBoxSize#value
        /// </summary>
        [JsonPropertyName("blockSize")]
        public double BlockSize { get; set; }

        /// <summary>
        /// The length of the observed element's content box in the inline dimension.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentBoxSize#value
        /// </summary>
        [JsonPropertyName("inlineSize")]
        public double InlineSize { get; set; }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Model for DomRectReadOnly javascript objects
    /// </summary>
    public struct JsDomRect
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("top")]
        public double Top { get; set; }

        [JsonPropertyName("right")]
        public double Right { get; set; }

        [JsonPropertyName("bottom")]
        public double Bottom { get; set; }

        [JsonPropertyName("left")]
        public double Left { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member