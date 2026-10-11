using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>The server's busy flag and its automatic-clear time.</summary>
    public class ServerBusyState
    {
        /// <summary>Whether the server is marked as busy.</summary>
        [JsonPropertyName("busy")]
        public bool IsBusy { get; set; }

        /// <summary>Expiry as Unix seconds. The server may report a negative value while idle.</summary>
        [JsonPropertyName("expires")]
        public long ExpiresAt { get; set; }

        /// <summary>Human-readable UTC expiry timestamp, when reported by the server.</summary>
        [JsonPropertyName("expires_ts")]
        public string? ExpiresTimestamp { get; set; }
    }
}
