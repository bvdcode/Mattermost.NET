using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Interactive dialog definition.
    /// </summary>
    public class InteractiveDialog
    {
        /// <summary>
        /// Identifier echoed back when the dialog is submitted.
        /// </summary>
        [JsonPropertyName("callback_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CallbackId { get; set; }

        /// <summary>
        /// Dialog title.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Markdown-formatted introductory text.
        /// </summary>
        [JsonPropertyName("introduction_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IntroductionText { get; set; }

        /// <summary>
        /// Icon URL displayed in the dialog.
        /// </summary>
        [JsonPropertyName("icon_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Dialog input elements.
        /// </summary>
        [JsonPropertyName("elements")]
        public IList<InteractiveDialogElement> Elements { get; set; } = new List<InteractiveDialogElement>();

        /// <summary>
        /// Submit button label.
        /// </summary>
        [JsonPropertyName("submit_label")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SubmitLabel { get; set; }

        /// <summary>
        /// Whether Mattermost should notify the integration when the user cancels the dialog.
        /// </summary>
        [JsonPropertyName("notify_on_cancel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NotifyOnCancel { get; set; }

        /// <summary>
        /// State echoed back with dialog submission payloads.
        /// </summary>
        [JsonPropertyName("state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? State { get; set; }

        /// <summary>
        /// URL used for field refresh requests and multi-step form responses.
        /// </summary>
        [JsonPropertyName("source_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SourceUrl { get; set; }
    }
}
