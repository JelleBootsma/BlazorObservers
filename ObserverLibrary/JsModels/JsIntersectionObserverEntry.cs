using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace BlazorObservers.ObserverLibrary.JsModels
{
    /// <summary>
    /// Model for callback parameter of the IntersectionObserver
    /// </summary>
    public class JsIntersectionObserverEntry
    {
        /// <summary>
        /// A DOMRectReadOnly object describing the bounds rectangle of the target element.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/boundingClientRect
        /// </summary>
        [JsonPropertyName("boundingClientRect")]
        public JsDomRect BoundingClientRect { get; set; }

        /// <summary>
        /// A ratio representing the percentage of the target element that is visible.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/intersectionRatio
        /// </summary>
        [JsonPropertyName("intersectionRatio")]
        public double IntersectionRatio { get; set; }

        /// <summary>
        /// A DOMRectReadOnly representing the visible area of the target element.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/intersectionRect
        /// </summary>
        [JsonPropertyName("intersectionRect")]
        public JsDomRect IntersectionRect { get; set; }

        /// <summary>
        /// True if the target element is currently intersecting with the root.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/isIntersecting
        /// </summary>
        [JsonPropertyName("isIntersecting")]
        public bool IsIntersecting { get; set; }

        /// <summary>
        /// A DOMRectReadOnly representing the root's bounding box.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/rootBounds
        /// </summary>
        [JsonPropertyName("rootBounds")]
        public JsDomRect? RootBounds { get; set; }

        /// <summary>
        /// The Element whose intersection with the root has changed.
        /// </summary>
        public ElementReference? TargetElement { get; set; }

        /// <summary>
        /// Tracking Id to match element from JS to ElementReference in C#
        /// </summary>
        [JsonPropertyName("targetTrackingId")]
        public string? TargetElementTrackingId { get; set; }

        /// <summary>
        /// A high-resolution timestamp indicating when the intersection was recorded.
        /// https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserverEntry/time
        /// </summary>
        [JsonPropertyName("time")]
        public double Time { get; set; }
    }
}