using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Response returned by dynamic select lookup endpoints.
    /// </summary>
    public class InteractiveDialogLookupResponse
    {
        /// <summary>
        /// Dynamic options returned to Mattermost.
        /// </summary>
        [JsonPropertyName("items")]
        public IList<InteractiveDialogOption> Items { get; set; } = new List<InteractiveDialogOption>();
    }
}
