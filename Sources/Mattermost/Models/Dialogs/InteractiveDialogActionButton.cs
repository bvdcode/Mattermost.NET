using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Action button configuration for an interactive dialog element.
    /// </summary>
    public class InteractiveDialogActionButton
    {
        /// <summary>
        /// URL called when the user clicks the action button.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Context forwarded to the action button URL.
        /// </summary>
        [JsonPropertyName("context")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object>? Context { get; set; }
    }
}
