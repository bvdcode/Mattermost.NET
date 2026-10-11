using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>State of a resumable server upload.</summary>
    public class UploadSession
    {
        /// <summary>Upload-session identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>Purpose of this upload.</summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UploadType Type { get; set; }

        /// <summary>Creation time as a Unix timestamp in milliseconds.</summary>
        [JsonPropertyName("create_at")]
        public long CreatedAt { get; set; }

        /// <summary>User performing the upload.</summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>Destination channel, when the upload is an attachment.</summary>
        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }

        /// <summary>File name.</summary>
        [JsonPropertyName("filename")]
        public string FileName { get; set; } = null!;

        /// <summary>Total expected file size in bytes.</summary>
        [JsonPropertyName("file_size")]
        public long FileSize { get; set; }

        /// <summary>Number of bytes received. Resume by sending content starting at this offset.</summary>
        [JsonPropertyName("file_offset")]
        public long FileOffset { get; set; }

        /// <summary>Remote cluster identifier for shared-channel uploads, if any.</summary>
        [JsonPropertyName("remote_id")]
        public string RemoteId { get; set; } = null!;

        /// <summary>Requested remote file identifier for shared-channel uploads, if any.</summary>
        [JsonPropertyName("req_file_id")]
        public string RequestedFileId { get; set; } = null!;
    }
}
