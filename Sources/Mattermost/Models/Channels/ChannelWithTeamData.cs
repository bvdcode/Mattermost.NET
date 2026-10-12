using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>A channel with team metadata returned by administrative discovery.</summary>
    public class ChannelWithTeamData : Channel
    {
        /// <summary>Display name of the channel's team.</summary>
        [JsonPropertyName("team_display_name")]
        public string TeamDisplayName { get; set; } = string.Empty;

        /// <summary>URL name of the channel's team.</summary>
        [JsonPropertyName("team_name")]
        public string TeamName { get; set; } = string.Empty;

        /// <summary>Team update time as Unix milliseconds.</summary>
        [JsonPropertyName("team_update_at")]
        public long TeamUpdatedAt { get; set; }
    }
}
