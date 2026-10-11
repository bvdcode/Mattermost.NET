using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Result of viewing channels or marking them read.</summary>
    public class ChannelViewResponse
    {
        /// <summary>The server's operation status, normally OK.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Channel identifiers mapped to Unix-millisecond view times. May be null when no channels were updated.</summary>
        [JsonPropertyName("last_viewed_at_times")]
        public IDictionary<string, long>? LastViewedAtTimes { get; set; } = new Dictionary<string, long>();
    }
}
