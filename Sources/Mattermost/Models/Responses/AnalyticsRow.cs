using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>A native analytics metric or date bucket.</summary>
    public class AnalyticsRow
    {
        /// <summary>Metric name or date bucket label, depending on the report.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Numeric metric value, including fractional values.</summary>
        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}
