using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Current team usage reported by the server.</summary>
    public class TeamsUsage
    {
        /// <summary>Number of active teams.</summary>
        [JsonPropertyName("active")]
        public long Active { get; set; }

        /// <summary>Number of deleted teams marked as archived by cloud limits.</summary>
        [JsonPropertyName("cloud_archived")]
        public long CloudArchived { get; set; }
    }
}
