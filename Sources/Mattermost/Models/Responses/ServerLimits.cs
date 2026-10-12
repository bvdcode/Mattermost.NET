using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>Server-reported seat and message-history limits, filtered for the current account.</summary>
    public class ServerLimits
    {
        /// <summary>Soft user limit; zero may mean unlimited or hidden from the current account.</summary>
        [JsonPropertyName("maxUsersLimit")]
        public long MaxUsersLimit { get; set; }

        /// <summary>Hard active-user limit; zero may mean unlimited or hidden from the current account.</summary>
        [JsonPropertyName("maxUsersHardLimit")]
        public long MaxUsersHardLimit { get; set; }

        /// <summary>Active-seat count; the server may exclude free single-channel guests. Hidden counts are zero.</summary>
        [JsonPropertyName("activeUserCount")]
        public long ActiveUserCount { get; set; }

        /// <summary>Number of guests belonging to exactly one channel, when tracked and visible.</summary>
        [JsonPropertyName("singleChannelGuestCount")]
        public long SingleChannelGuestCount { get; set; }

        /// <summary>Server-reported single-channel guest limit, when tracked and visible.</summary>
        [JsonPropertyName("singleChannelGuestLimit")]
        public long SingleChannelGuestLimit { get; set; }

        /// <summary>Message-history limit, or zero when no limit applies.</summary>
        [JsonPropertyName("postHistoryLimit")]
        public long PostHistoryLimit { get; set; }

        /// <summary>Accessible-history boundary as Unix milliseconds, or zero when no limit is reached.</summary>
        [JsonPropertyName("lastAccessiblePostTime")]
        public long LastAccessiblePostTime { get; set; }
    }
}
