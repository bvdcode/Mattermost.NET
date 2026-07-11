using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Interactive dialog element types.
    /// </summary>
    public enum InteractiveDialogElementType
    {
        /// <summary>
        /// Single-line text input.
        /// </summary>
        [JsonStringEnumMemberName("text")]
        Text,

        /// <summary>
        /// Multi-line text input.
        /// </summary>
        [JsonStringEnumMemberName("textarea")]
        Textarea,

        /// <summary>
        /// Select menu input.
        /// </summary>
        [JsonStringEnumMemberName("select")]
        Select,

        /// <summary>
        /// Boolean checkbox input.
        /// </summary>
        [JsonStringEnumMemberName("bool")]
        Bool,

        /// <summary>
        /// Radio option input.
        /// </summary>
        [JsonStringEnumMemberName("radio")]
        Radio,

        /// <summary>
        /// Date picker input.
        /// </summary>
        [JsonStringEnumMemberName("date")]
        Date,

        /// <summary>
        /// Date and time picker input.
        /// </summary>
        [JsonStringEnumMemberName("datetime")]
        DateTime,

        /// <summary>
        /// File upload input.
        /// </summary>
        [JsonStringEnumMemberName("file")]
        File,

        /// <summary>
        /// Dialog action button.
        /// </summary>
        [JsonStringEnumMemberName("action_button")]
        ActionButton
    }
}
