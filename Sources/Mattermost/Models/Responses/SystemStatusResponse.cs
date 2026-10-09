using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Public server health and mobile client requirements.</summary>
    public class SystemStatusResponse
    {
        /// <summary>Overall server status, such as OK or UNHEALTHY.</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Database status when backend checks are requested.</summary>
        [JsonPropertyName("database_status")]
        public string? DatabaseStatus { get; set; }

        /// <summary>File storage status when backend checks are requested.</summary>
        [JsonPropertyName("filestore_status")]
        public string? FileStoreStatus { get; set; }

        /// <summary>Latest supported Android client version.</summary>
        [JsonPropertyName("AndroidLatestVersion")]
        public string? AndroidLatestVersion { get; set; }

        /// <summary>Minimum supported Android client version.</summary>
        [JsonPropertyName("AndroidMinVersion")]
        public string? AndroidMinVersion { get; set; }

        /// <summary>Latest supported iOS client version.</summary>
        [JsonPropertyName("IosLatestVersion")]
        public string? IosLatestVersion { get; set; }

        /// <summary>Minimum supported iOS client version.</summary>
        [JsonPropertyName("IosMinVersion")]
        public string? IosMinVersion { get; set; }

        /// <summary>Active search backend when reported by the server.</summary>
        [JsonPropertyName("ActiveSearchBackend")]
        public string? ActiveSearchBackend { get; set; }
    }
}
