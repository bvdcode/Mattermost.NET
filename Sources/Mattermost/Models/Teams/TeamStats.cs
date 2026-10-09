using System.Text.Json.Serialization;

namespace Mattermost.Models.Teams
{
    /// <summary>
    /// Team membership counts visible to the current user.
    /// </summary>
    public class TeamStats
    {
        /// <summary>
        /// Team identifier.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

        /// <summary>
        /// Total member count, including deactivated users.
        /// </summary>
        [JsonPropertyName("total_member_count")]
        public long TotalMemberCount { get; set; }

        /// <summary>
        /// Member count excluding deactivated users.
        /// </summary>
        [JsonPropertyName("active_member_count")]
        public long ActiveMemberCount { get; set; }
    }
}
