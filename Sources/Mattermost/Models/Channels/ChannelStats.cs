using System.Text.Json.Serialization;

namespace Mattermost.Models.Channels
{
    /// <summary>
    /// Membership, pinned post and file counts for a channel.
    /// </summary>
    public class ChannelStats
    {
        /// <summary>
        /// Channel identifier.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = null!;

        /// <summary>
        /// Active member count, including guests.
        /// </summary>
        [JsonPropertyName("member_count")]
        public long MemberCount { get; set; }

        /// <summary>
        /// Active guest member count.
        /// </summary>
        [JsonPropertyName("guest_count")]
        public long GuestCount { get; set; }

        /// <summary>
        /// Pinned post count.
        /// </summary>
        [JsonPropertyName("pinnedpost_count")]
        public long PinnedPostCount { get; set; }

        /// <summary>
        /// File count, or -1 when counting files is excluded.
        /// </summary>
        [JsonPropertyName("files_count")]
        public long FilesCount { get; set; }
    }
}
