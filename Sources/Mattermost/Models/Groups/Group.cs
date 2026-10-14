using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Groups
{
    /// <summary>A directory or custom group.</summary>
    public class Group
    {
        /// <summary>Group identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Mention name when available.</summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>Display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Group description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Native ldap, custom or plugin source.</summary>
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        /// <summary>External directory identifier.</summary>
        [JsonPropertyName("remote_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RemoteId { get; set; }

        /// <summary>Creation time in Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time in Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deletion time in Unix milliseconds, or zero.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Whether resource links exist.</summary>
        [JsonPropertyName("has_syncables")]
        public bool HasSyncables { get; set; }

        /// <summary>Member count when requested.</summary>
        [JsonPropertyName("member_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MemberCount { get; set; }

        /// <summary>Whether the group can be mentioned.</summary>
        [JsonPropertyName("allow_reference")]
        public bool AllowReference { get; set; }

        /// <summary>Member count in the selected channel when requested.</summary>
        [JsonPropertyName("channel_member_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ChannelMemberCount { get; set; }

        /// <summary>Distinct member timezone count when requested.</summary>
        [JsonPropertyName("channel_member_timezones_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ChannelMemberTimezonesCount { get; set; }

        /// <summary>Member identifiers when requested.</summary>
        [JsonPropertyName("member_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? MemberIds { get; set; } = new List<string>();
    }
}
