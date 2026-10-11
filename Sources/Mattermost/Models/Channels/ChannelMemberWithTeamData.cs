using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>A channel membership with team metadata returned by the user membership listing.</summary>
    public class ChannelMemberWithTeamData : ChannelUserInfo
    {
        /// <summary>Display name of the channel's team.</summary>
        [JsonPropertyName("team_display_name")]
        public string TeamDisplayName { get; set; } = null!;

        /// <summary>URL name of the channel's team.</summary>
        [JsonPropertyName("team_name")]
        public string TeamName { get; set; } = null!;

        /// <summary>Team update time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("team_update_at")]
        public long TeamUpdatedAt { get; set; }

        /// <summary>The member's root-post mention counter.</summary>
        [JsonPropertyName("mention_count_root")]
        public long RootMentionCount { get; set; }

        /// <summary>The member's urgent mention counter.</summary>
        [JsonPropertyName("urgent_mention_count")]
        public long UrgentMentionCount { get; set; }

        /// <summary>The member's root-post message counter.</summary>
        [JsonPropertyName("msg_count_root")]
        public long RootMessageCount { get; set; }

        /// <summary>Whether the guest role is granted by the channel scheme.</summary>
        [JsonPropertyName("scheme_guest")]
        public bool SchemeGuest { get; set; }

        /// <summary>Whether the user role is granted by the channel scheme.</summary>
        [JsonPropertyName("scheme_user")]
        public bool SchemeUser { get; set; }

        /// <summary>Whether the administrator role is granted by the channel scheme.</summary>
        [JsonPropertyName("scheme_admin")]
        public bool SchemeAdmin { get; set; }

        /// <summary>Space-separated roles assigned explicitly, excluding scheme roles.</summary>
        [JsonPropertyName("explicit_roles")]
        public string ExplicitRoles { get; set; } = null!;

        /// <summary>Whether automatic translation is disabled for this membership.</summary>
        [JsonPropertyName("autotranslation_disabled")]
        public bool AutoTranslationDisabled { get; set; }
    }
}
