using System.Text.Json.Serialization;

namespace Mattermost.Models.Bots
{
    /// <summary>A bot account and its ownership metadata.</summary>
    public class Bot
    {
        /// <summary>Identifier of the associated user account.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Bot username.</summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Bot display name.</summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Bot description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Owning user identifier or plugin identifier.</summary>
        [JsonPropertyName("owner_id")]
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>Last custom icon update as Unix milliseconds; zero when unset.</summary>
        [JsonPropertyName("last_icon_update")]
        public long LastIconUpdate { get; set; }

        /// <summary>Creation time as Unix milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>Last update time as Unix milliseconds.</summary>
        [JsonPropertyName("update_at")]
        public long UpdatedAt { get; set; }

        /// <summary>Deactivation time as Unix milliseconds; zero when active.</summary>
        [JsonPropertyName("delete_at")]
        public long DeletedAt { get; set; }

        /// <summary>Whether the server reports this as a protected system-owned bot.</summary>
        [JsonPropertyName("system_owned")]
        public bool SystemOwned { get; set; }
    }
}
