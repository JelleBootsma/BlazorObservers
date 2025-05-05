using System.Text.Json.Serialization;
namespace BlazorObservers.ObserverLibrary.JsModels
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Model for DomRectReadOnly javascript objects
    /// </summary>
    public readonly struct JsDomRect
    {
        [JsonPropertyName("x")]
        public double X { get; init; }

        [JsonPropertyName("y")]
        public double Y { get; init; }

        [JsonPropertyName("width")]
        public double Width { get; init; }

        [JsonPropertyName("height")]
        public double Height { get; init; }

        [JsonPropertyName("top")]
        public double Top { get; init; }

        [JsonPropertyName("right")]
        public double Right { get; init; }

        [JsonPropertyName("bottom")]
        public double Bottom { get; init; }

        [JsonPropertyName("left")]
        public double Left { get; init; }
    }

}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member