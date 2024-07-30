using System.Text.Json.Serialization;

namespace Mattermost.Models
{
    /// <summary>
    /// User notification properties.
    /// </summary>
    public class NotifyProps
    {
        /// <summary>
        /// Set to "true" to enable channel-wide notifications (@channel, @all, etc.), "false" to disable. Defaults to "true".
        /// </summary>
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        /// <summary>
        /// Set to "all" to receive desktop notifications for all activity, "mention" for mentions and direct messages only, 
        /// and "none" to disable. Defaults to "all".
        /// </summary>
        [JsonPropertyName("desktop")]
        public string? Desktop { get; set; }

        /// <summary>
        /// Set to "true" to enable sound on desktop notifications, "false" to disable. Defaults to "true".
        /// </summary>
        [JsonPropertyName("desktop_sound")]
        public string? DesktopSound { get; set; }

        /// <summary>
        /// Set to "true" to enable email notifications, "false" to disable. Defaults to "true".
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Set to "true" to enable mentions for first name. Defaults to "true" if a first name is set, "false" otherwise.
        /// </summary>
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Set to "true" to enable mentions for mention keys. Defaults to "true" if a first name is set, "false" otherwise.
        /// </summary>
        [JsonPropertyName("mention_keys")]
        public string? MentionKeys { get; set; }

        /// <summary>
        /// Set to "all" to receive push notifications for all activity, "mention" for mentions and direct messages only, 
        /// and "none" to disable. Defaults to "mention".
        /// </summary>
        [JsonPropertyName("push")]
        public string? Push { get; set; }
    }
}