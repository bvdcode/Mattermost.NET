using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>
    /// Remote file details.
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// The unique identifier for this file.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user that uploaded this file.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// If this file is attached to a post, the ID of that post.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// The time in milliseconds a file was created.
        /// </summary>
        [JsonPropertyName("create_at")]
        public long CreateAt { get; set; }

        /// <summary>
        /// The time in milliseconds a file was last updated.
        /// </summary>
        [JsonPropertyName("update_at")]
        public long UpdateAt { get; set; }

        /// <summary>
        /// The time in milliseconds a file was deleted.
        /// </summary>
        [JsonPropertyName("delete_at")]
        public long DeleteAt { get; set; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The extension at the end of the file name.
        /// </summary>
        [JsonPropertyName("extension")]
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// The MIME type of the file.
        /// </summary>
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// If this file is an image, the width of the file.
        /// </summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }

        /// <summary>
        /// If this file is an image, the height of the file.
        /// </summary>
        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// If this file is an image, whether or not it has a preview-sized version.
        /// </summary>
        [JsonPropertyName("has_preview_image")]
        public bool HasPreviewImage { get; set; }

        /// <summary>
        /// Base64 image if file has a preview.
        /// </summary>
        [JsonPropertyName("mini_preview")]
        public string MiniPreview { get; set; } = string.Empty;

        /// <summary>
        /// Is file archived? (Removed)
        /// </summary>
        [JsonPropertyName("archived")]
        public bool Archived { get; set; }
    }

    internal class FileResponse
    {
        [JsonPropertyName("file_infos")]
        public IEnumerable<FileDetails> Files { get; set; } = Enumerable.Empty<FileDetails>();
    }
}