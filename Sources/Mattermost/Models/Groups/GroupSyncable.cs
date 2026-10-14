using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>A group link to a team or channel.</summary>
    public class GroupSyncable
    {
        /// <summary>Group identifier.</summary>
        [JsonPropertyName("group_id")]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>Linked team or parent team identifier.</summary>
        [JsonPropertyName("team_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TeamId { get; set; }

        /// <summary>Linked channel identifier.</summary>
        [JsonPropertyName("channel_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChannelId { get; set; }

        /// <summary>Native Team or Channel type.</summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>Whether users are automatically added.</summary>
        [JsonPropertyName("auto_add")]
        public bool AutoAdd { get; set; }

        /// <summary>Whether members receive the administrator role.</summary>
        [JsonPropertyName("scheme_admin")]
        public bool SchemeAdmin { get; set; }

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Linked or parent team display name.</summary>
        [JsonPropertyName("team_display_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TeamDisplayName { get; set; }

        /// <summary>Linked or parent team privacy type.</summary>
        [JsonPropertyName("team_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TeamType { get; set; }

        /// <summary>Linked channel display name.</summary>
        [JsonPropertyName("channel_display_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChannelDisplayName { get; set; }

        /// <summary>Linked channel privacy type.</summary>
        [JsonPropertyName("channel_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChannelType { get; set; }
    }
}
