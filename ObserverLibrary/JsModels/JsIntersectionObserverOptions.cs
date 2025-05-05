using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace BlazorObservers.ObserverLibrary.JsModels
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Model for IntersectionObserver options JavaScript object.
    /// </summary>
    public readonly struct JsIntersectionObserverOptions
    {
        public JsIntersectionObserverOptions()
        {
            Root = null;
            Threshold = [0];
        }

        /// <summary>
        /// The root element used for intersection calculations. If null, the viewport is used.
        /// </summary>
        [JsonPropertyName("root")]
        public ElementReference? Root { get; init; }

        /// <summary>
        /// Margin around the root. Can have values similar to CSS margin property, e.g., "10px 20px".
        /// </summary>
        [JsonPropertyName("rootMargin")]
        public string RootMargin { get; init; } = "0px 0px 0px 0px";

        /// <summary>
        /// A set of numbers between 0.0 and 1.0 indicating at what percentage of the target's visibility the observer's callback should be executed.
        /// Default is a single value of 0, meaning the callback will be executed as soon as even one pixel is visible, and not again until the target is fully out of view again.
        /// A value of 1.0 means the callback will be executed when the target is fully visible. A value of [0, 0.5, 1.0] means the callback will be executed when the target is 0%, 50%, and 100% visible.
        /// </summary>
        [JsonPropertyName("threshold")]
        public IEnumerable<double> Threshold { get; init; }
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
