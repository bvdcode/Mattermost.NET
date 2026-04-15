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
        Default,

        /// <summary>
        /// Primary style (emphasized button, blue).
        /// </summary>
        Primary,

        /// <summary>
        /// Warning style (cautious action, yellow/orange).
        /// </summary>
        Warning,

        /// <summary>
        /// Success style (positive action, green).
        /// </summary>
        Success,

        /// <summary>
        /// Danger style (destructive action, red).
        /// </summary>
        Danger
    }
}