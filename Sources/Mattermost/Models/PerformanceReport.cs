using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>Client performance samples submitted to the server's metrics backend.</summary>
    public class PerformanceReport
    {
        /// <summary>The current native report schema version.</summary>
        public const string CurrentVersion = "0.1.0";

        /// <summary>Report schema version; native support is validated by the server.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = CurrentVersion;

        /// <summary>Client identifier; currently unused by the native metrics backend.</summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>Labels shared by all samples, such as platform and agent.</summary>
        [JsonPropertyName("labels")]
        public IDictionary<string, string>? Labels { get; set; }

        /// <summary>First sample time in Unix milliseconds; fractional values are supported.</summary>
        [JsonPropertyName("start")]
        public double Start { get; set; }

        /// <summary>Last sample time in Unix milliseconds; the server normally rejects reports older than five minutes.</summary>
        [JsonPropertyName("end")]
        public double End { get; set; }

        /// <summary>Counter increments, such as long_tasks.</summary>
        [JsonPropertyName("counters")]
        public IList<MetricSample>? Counters { get; set; }

        /// <summary>Histogram measurements, such as TTFB or channel_switch.</summary>
        [JsonPropertyName("histograms")]
        public IList<MetricSample>? Histograms { get; set; }
    }
}
