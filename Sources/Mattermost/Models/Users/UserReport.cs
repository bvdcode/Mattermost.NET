using System.Text.Json.Serialization;

namespace Mattermost.Models.Users
{
    /// <summary>Administrative user profile with server-calculated activity and membership statistics.</summary>
    public class UserReport : User
    {
        /// <summary>Last account login as Unix milliseconds, or zero when unavailable.</summary>
        [JsonPropertyName("last_login")]
        public long LastLoginUnixTimestamp { get; set; }

        /// <summary>Last recorded status activity as Unix milliseconds, when available.</summary>
        [JsonPropertyName("last_status_at")]
        public long? LastStatusAtUnixTimestamp { get; set; }

        /// <summary>Last post time within the selected statistics period, as Unix milliseconds.</summary>
        [JsonPropertyName("last_post_date")]
        public long? LastPostDateUnixTimestamp { get; set; }

        /// <summary>Number of days with posts within the selected statistics period.</summary>
        [JsonPropertyName("days_active")]
        public long? DaysActive { get; set; }

        /// <summary>Number of posts within the selected statistics period, when available.</summary>
        [JsonPropertyName("total_posts")]
        public long? TotalPosts { get; set; }

        /// <summary>Number of active public/private channel memberships.</summary>
        [JsonPropertyName("channel_count")]
        public long? ChannelCount { get; set; }

        /// <summary>Server-formatted, comma-separated active-team display names; not team identifiers.</summary>
        [JsonPropertyName("teams")]
        public string Teams { get; set; } = string.Empty;
    }
}
