using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Represents the HTTP POST payload sent by Mattermost to a post action integration URL.
    /// </summary>
    public class PostActionIntegrationRequest
    {
        /// <summary>
        /// The ID of the user who triggered the action.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The username of the user who triggered the action.
        /// </summary>
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the channel containing the post action.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the channel containing the post action.
        /// </summary>
        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the team containing the post action.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// The team domain sent by Mattermost.
        /// </summary>
        [JsonPropertyName("team_domain")]
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the post containing the action.
        /// </summary>
        [JsonPropertyName("post_id")]
        public string PostId { get; set; } = string.Empty;

        /// <summary>
        /// The trigger ID that can be used to open an interactive dialog.
        /// </summary>
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; } = string.Empty;

        /// <summary>
        /// The interactive element type, such as button or select.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The data source used by the action, when one is configured.
        /// </summary>
        [JsonPropertyName("data_source")]
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// The action context configured on the original post action.
        /// </summary>
        [JsonPropertyName("context")]
        public Dictionary<string, JsonElement> Context { get; set; } =
            new Dictionary<string, JsonElement>();
    }
}
