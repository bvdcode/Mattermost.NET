using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Defines the server-side data sources available for select post actions.
    /// </summary>
    public enum PostActionDataSource
    {
        /// <summary>
        /// Populate the select action with Mattermost users.
        /// </summary>
        [JsonStringEnumMemberName("users")]
        Users,

        /// <summary>
        /// Populate the select action with Mattermost public channels.
        /// </summary>
        [JsonStringEnumMemberName("channels")]
        Channels
    }
}
