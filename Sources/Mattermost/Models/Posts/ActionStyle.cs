using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Defines the visual styles available for post actions.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActionStyle
    {
        /// <summary>
        /// Default style (standard button appearance).
        /// </summary>
        [EnumMember(Value = "default")]
        Default,

        /// <summary>
        /// Primary style (emphasized button, blue).
        /// </summary>
        [EnumMember(Value = "primary")]
        Primary,

        /// <summary>
        /// Warning style (cautious action, yellow/orange).
        /// </summary>
        [EnumMember(Value = "warning")]
        Warning,

        /// <summary>
        /// Success style (positive action, green).
        /// </summary>
        [EnumMember(Value = "success")]
        Success,

        /// <summary>
        /// Danger style (destructive action, red).
        /// </summary>
        [EnumMember(Value = "danger")]
        Danger
    }
}