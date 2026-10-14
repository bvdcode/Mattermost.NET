using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>One counter increment or histogram measurement.</summary>
    public class MetricSample
    {
        /// <summary>Native metric name. Supported names depend on the server version; unknown names are ignored.</summary>
        [JsonPropertyName("metric")]
        public string Metric { get; set; } = string.Empty;

        /// <summary>Finite measurement in the metric's native units; timing measurements generally use milliseconds.</summary>
        [JsonPropertyName("value")]
        public double Value { get; set; }

        /// <summary>Additional labels defined for this metric.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string>? Labels { get; set; }
    }
}
