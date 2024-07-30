using Mattermost.Models.Posts;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses.Websocket
{
    /// <summary>
    /// Post update information.
    /// </summary>
    public class PostUpdateInfo : PostInfo
    {
        /// <summary>
        /// Post information.
        /// </summary>
        public new Post Post { get; set; } = null!;
    }
}