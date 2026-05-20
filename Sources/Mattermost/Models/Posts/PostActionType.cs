using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Defines the interactive element types available for post actions.
    /// </summary>
    public enum PostActionType
    {
        /// <summary>
        /// Button action.
        /// </summary>
        [JsonStringEnumMemberName("button")]
        Button,

        /// <summary>
        /// Select menu action.
        /// </summary>
        [JsonStringEnumMemberName("select")]
        Select
    }
}
