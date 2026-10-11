using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>Channel, team and current-user membership information for a post.</summary>
    public class PostLocationInfo
    {
        /// <summary>Channel containing the post.</summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>Native channel type: O (public), P (private), D (direct), or G (group).</summary>
        [JsonPropertyName("channel_type")]
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>Channel display name.</summary>
        [JsonPropertyName("channel_display_name")]
        public string ChannelDisplayName { get; set; } = string.Empty;

        /// <summary>Whether the current user belongs to the channel.</summary>
        [JsonPropertyName("has_joined_channel")]
        public bool HasJoinedChannel { get; set; }

        /// <summary>Team identifier, or an empty string for direct and group channels.</summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>O when the team allows open invitations, I otherwise; empty when there is no team.</summary>
        [JsonPropertyName("team_type")]
        public string TeamType { get; set; } = string.Empty;

        /// <summary>Team display name, or an empty string when there is no team.</summary>
        [JsonPropertyName("team_display_name")]
        public string TeamDisplayName { get; set; } = string.Empty;

        /// <summary>Whether the current user belongs to the team.</summary>
        [JsonPropertyName("has_joined_team")]
        public bool HasJoinedTeam { get; set; }
    }
}
