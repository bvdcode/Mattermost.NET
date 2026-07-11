using System.Text.Json.Serialization;

namespace Mattermost.Models.Dialogs
{
    /// <summary>
    /// Keyboard/input subtypes for text dialog elements.
    /// </summary>
    public enum InteractiveDialogTextSubtype
    {
        /// <summary>
        /// Plain text input.
        /// </summary>
        [JsonStringEnumMemberName("text")]
        Text,

        /// <summary>
        /// Email input.
        /// </summary>
        [JsonStringEnumMemberName("email")]
        Email,

        /// <summary>
        /// Numeric input.
        /// </summary>
        [JsonStringEnumMemberName("number")]
        Number,

        /// <summary>
        /// Password input.
        /// </summary>
        [JsonStringEnumMemberName("password")]
        Password,

        /// <summary>
        /// Telephone input.
        /// </summary>
        [JsonStringEnumMemberName("tel")]
        Tel,

        /// <summary>
        /// URL input.
        /// </summary>
        [JsonStringEnumMemberName("url")]
        Url
    }
}
