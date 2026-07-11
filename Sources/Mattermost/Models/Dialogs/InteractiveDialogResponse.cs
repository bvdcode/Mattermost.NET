using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Response an integration can return for dialog validation, refresh, or multi-step flows.
    /// </summary>
    public class InteractiveDialogResponse
    {
        /// <summary>
        /// Response type, such as ok or form.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>
        /// Field-specific validation errors.
        /// </summary>
        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// Generic error shown to the user.
        /// </summary>
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }

        /// <summary>
        /// Replacement dialog form used by refresh and multi-step responses.
        /// </summary>
        [JsonPropertyName("form")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InteractiveDialog? Form { get; set; }
    }
}
