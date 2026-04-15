using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Defines the visual styles available for post actions.
    /// </summary>
    public enum ActionStyle
    {
        /// <summary>
        /// Default style (standard button appearance).
        /// </summary>
        [JsonStringEnumMemberName("default")]
        Default,

        /// <summary>
        /// Primary style (emphasized button, blue).
        /// </summary>
        [JsonStringEnumMemberName("primary")]
        Primary,

        /// <summary>
        /// Warning style (cautious action, yellow/orange).
        /// </summary>
        [JsonStringEnumMemberName("warning")]
        Warning,

        /// <summary>
        /// Success style (positive action, green).
        /// </summary>
        [JsonStringEnumMemberName("success")]
        Success,

        /// <summary>
        /// Danger style (destructive action, red).
        /// </summary>
        [JsonStringEnumMemberName("danger")]
        Danger,
    }
}