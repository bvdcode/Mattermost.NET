using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Data sources available for interactive dialog select elements.
    /// </summary>
    public enum InteractiveDialogDataSource
    {
        /// <summary>
        /// Populate options from users.
        /// </summary>
        [JsonStringEnumMemberName("users")]
        Users,

        /// <summary>
        /// Populate options from public channels.
        /// </summary>
        [JsonStringEnumMemberName("channels")]
        Channels,

        /// <summary>
        /// Populate options from a dynamic lookup URL.
        /// </summary>
        [JsonStringEnumMemberName("dynamic")]
        Dynamic
    }
}
