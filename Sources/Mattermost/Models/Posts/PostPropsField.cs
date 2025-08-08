using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Fields can be included as an optional array within attachments, and are used to display information in a table format inside the attachment.
    /// </summary>
    public class PostPropsField
    {
        /// <summary>
        /// A title shown in the table above the value. As of v5.14 a title will render emojis properly.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The text value of the field. It can be formatted using Markdown. Supports @mentions.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Optionally set to true or false (boolean) to indicate whether the value is short enough to be displayed beside other values.
        /// </summary>
        [JsonPropertyName("short")]
        public bool Short { get; set; }
    }
}
