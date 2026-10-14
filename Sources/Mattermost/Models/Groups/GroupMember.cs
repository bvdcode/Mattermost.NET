using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>Membership of a user in a group.</summary>
    public class GroupMember
    {
        /// <summary>Group identifier.</summary>
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>User identifier.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }
    }
}
