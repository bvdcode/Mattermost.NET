using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>Native group membership statistics.</summary>
    public class GroupStats
    {
        /// <summary>Group identifier.</summary>
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>Total number of members.</summary>
        [JsonPropertyName("total_member_count")]
        public long TotalMemberCount { get; set; }
    }
}
