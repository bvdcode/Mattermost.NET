using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Server-reported progress or error for an in-place Team Edition to Enterprise Edition upgrade.</summary>
    public class EnterpriseUpgradeStatus
    {
        /// <summary>Integer completion percentage. Zero does not establish that an upgrade is running.</summary>
        [JsonPropertyName("percentage")]
        public long Percentage { get; set; }

        /// <summary>Upgrade error reported in a successful HTTP response, or null when no error is reported.</summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
