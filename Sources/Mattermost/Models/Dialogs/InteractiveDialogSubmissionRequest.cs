using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Payload sent to an integration when an interactive dialog is submitted, refreshed, or looked up.
    /// </summary>
    public class InteractiveDialogSubmissionRequest
    {
        /// <summary>
        /// Payload type, such as dialog_submission, refresh, or dialog_lookup.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Source URL for refresh or lookup requests.
        /// </summary>
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }

        /// <summary>
        /// Callback identifier from the dialog definition.
        /// </summary>
        [JsonPropertyName("callback_id")]
        public string CallbackId { get; set; } = string.Empty;

        /// <summary>
        /// Dialog state from the dialog definition.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Channel identifier.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Team identifier.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = string.Empty;

        /// <summary>
        /// Submitted field values.
        /// </summary>
        [JsonPropertyName("submission")]
        public Dictionary<string, JsonElement> Submission { get; set; } = new Dictionary<string, JsonElement>();

        /// <summary>
        /// Uploaded file identifiers.
        /// </summary>
        [JsonPropertyName("file_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? FileIds { get; set; }

        /// <summary>
        /// Whether the user cancelled the dialog.
        /// </summary>
        [JsonPropertyName("cancelled")]
        public bool Cancelled { get; set; }
    }
}
