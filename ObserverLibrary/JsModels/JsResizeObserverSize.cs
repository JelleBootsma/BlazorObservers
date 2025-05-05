using System.Text.Json.Serialization;
namespace BlazorObservers.ObserverLibrary.JsModels
{
    /// <summary>
    /// Model for ResizeObserverSize JavaScript models
    /// </summary>
    public readonly struct JsResizeObserverSize
    {
        /// <summary>
        /// The length of the observed element's content box in the block dimension.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentBoxSize#value
        /// </summary>
        [JsonPropertyName("blockSize")]
        public double BlockSize { get; init; }

        /// <summary>
        /// The length of the observed element's content box in the inline dimension.
        /// 
        /// https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserverEntry/contentBoxSize#value
        /// </summary>
        [JsonPropertyName("inlineSize")]
        public double InlineSize { get; init; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member