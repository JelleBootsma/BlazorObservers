using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/// <summary>
/// Namespace for models which mirror models in Javascript
/// </summary>
namespace BlazorObservers.ObserverLibrary.JsModels
{
    /// <summary>
    /// Model for callback parameter of the ResizeObserver
    /// </summary>
    public class JsResizeObserverEntry
    {
        [JsonPropertyName("contentRect")]
        public JsDomRect ContentRect { get; set; }
        [JsonPropertyName("borderBoxSize")]
        public JsResizeObserverSize BorderBoxSize { get; set; }
        [JsonPropertyName("contentBoxSize")]
        public JsResizeObserverSize ContentBoxSize { get; set; }

        public ElementReference? TargetElement { get; set; }
        [JsonPropertyName("targetTrackingId")]
        public string? TargetElementTrackingId { get; set; }
    }
    /// <summary>
    /// Model for ResizeObserverSize javascript models
    /// </summary>
    public struct JsResizeObserverSize {
        [JsonPropertyName("blockSize")]
        public double BlockSize { get; set; }

        [JsonPropertyName("inlineSize")]
        public double InlineSize { get; set; }
    }


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
