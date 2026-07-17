using Mattermost.Models.Posts;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.SlashCommands
{
    /// <summary>
    /// Represents the message content of a slash command response.
    /// </summary>
    public class SlashCommandResponseItem
    {
        /// <summary>
        /// The Markdown-formatted response text.
        /// </summary>
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        /// <summary>
        /// Message attachments used for rich response formatting.
        /// </summary>
        [JsonPropertyName("attachments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<PostPropsAttachment>? Attachments { get; set; }

        /// <summary>
        /// The visibility of the response.
        /// </summary>
        [JsonPropertyName("response_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SlashCommandResponseType? ResponseType { get; set; }

        /// <summary>
        /// The username displayed for the response.
        /// </summary>
        [JsonPropertyName("username")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Username { get; set; }

        /// <summary>
        /// The identifier of the channel where the response should be posted.
        /// </summary>
        [JsonPropertyName("channel_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChannelId { get; set; }

        /// <summary>
        /// The URL of the profile image displayed for the response.
        /// </summary>
        [JsonPropertyName("icon_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IconUrl { get; set; }

        /// <summary>
        /// The custom post type used for the response.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>
        /// Whether Mattermost should skip Slack-compatible parsing of the response text.
        /// </summary>
        [JsonPropertyName("skip_slack_parsing")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SkipSlackParsing { get; set; }

        /// <summary>
        /// Additional metadata stored with the response post.
        /// </summary>
        [JsonPropertyName("props")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object>? Props { get; set; }
    }
}
