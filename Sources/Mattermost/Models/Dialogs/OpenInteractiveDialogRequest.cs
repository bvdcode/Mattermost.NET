using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Request used to open an interactive dialog.
    /// </summary>
    public class OpenInteractiveDialogRequest
    {
        /// <summary>
        /// Trigger identifier provided by a slash command or interactive action payload.
        /// </summary>
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; } = string.Empty;

        /// <summary>
        /// URL where Mattermost sends the submitted dialog payload.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Dialog definition to display.
        /// </summary>
        [JsonPropertyName("dialog")]
        public InteractiveDialog Dialog { get; set; } = new InteractiveDialog();
    }
}
