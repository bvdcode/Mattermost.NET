using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Represents an attachment in a post, which can include text, images, and actions.
    /// </summary>
    public class PostPropsAttachment
    {
        /// <summary>
        /// An optional line of text that will be shown above the attachment. Supports @mentions.
        /// </summary>
        [JsonPropertyName("pretext")]
        public string? Pretext { get; set; }

        /// <summary>
        /// The text to be included in the attachment. It can be formatted using Markdown. 
        /// For long texts, the message is collapsed and a “Show More” link is added to expand the message. Supports @mentions.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        ///  A hex color code that will be used as the left border color for the attachment. 
        ///  If not specified, it will default to match the channel sidebar header background color.
        /// </summary>
        [JsonPropertyName("color")]
        public string? Color { get; set; }

        /// <summary>
        /// A required plain-text summary of the attachment.
        /// This is used in notifications, and in clients that don’t support formatted text (e.g. IRC).
        /// </summary>
        [JsonPropertyName("fallback")]
        public string Fallback { get; set; } = string.Empty;

        /// <summary>
        /// An optional name used to identify the author. It will be included in a small section at the top of the attachment.
        /// </summary>
        [JsonPropertyName("author_name")]
        public string? AuthorName { get; set; }

        /// <summary>
        /// An optional URL used to display a 16x16 pixel icon beside the author_name.
        /// </summary>
        [JsonPropertyName("author_icon")]
        public string? AuthorIcon { get; set; }

        /// <summary>
        /// An optional URL used to hyperlink the author_name. If no author_name is specified, this field does nothing.
        /// </summary>
        [JsonPropertyName("author_link")]
        public string? AuthorLink { get; set; }

        /// <summary>
        /// An optional title displayed below the author information in the attachment.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// An optional URL used to hyperlink the title. If no title is specified, this field does nothing.
        /// </summary>
        [JsonPropertyName("title_link")]
        public string? TitleLink { get; set; }

        /// <summary>
        /// An optional URL to an image file (GIF, JPEG, PNG, BMP, or SVG) that is displayed inside a message attachment.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Fields can be included as an optional array within attachments, and are used to display information in a table format inside the attachment.
        /// </summary>
        [JsonPropertyName("fields")]
        public IList<PostPropsField> Fields { get; set; } = new List<PostPropsField>();

        /// <summary>
        /// Actions can be included as an optional array within attachments, and are used to display buttons inside the attachment.
        /// </summary>
        [JsonPropertyName("actions")]
        public IList<PostPropsAction> Actions { get; set; } = new List<PostPropsAction>();
    }
}
