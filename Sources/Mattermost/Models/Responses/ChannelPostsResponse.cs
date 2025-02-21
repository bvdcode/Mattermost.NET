using Mattermost.Models.Posts;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses
{
    /// <summary>
    /// Result of GetPostsForChannel query.
    /// </summary>
    public class ChannelPostsResponse
    {
        /// <summary>
        /// List of post IDs in the order they were created.
        /// </summary>
        [JsonPropertyName("order")]
        public IEnumerable<string> Order { get; set; } = new List<string>();

        /// <summary>
        /// List of posts by id.
        /// </summary>
        [JsonPropertyName("posts")]
        public IDictionary<string, Post> Posts { get; set; } = new Dictionary<string, Post>();

        /// <summary>
        /// The ID of next post. Not omitted when empty or not relevant.
        /// </summary>
        [JsonPropertyName("next_post_id")]
        public string? NextPostId { get; set; }

        /// <summary>
        /// The ID of previous post. Not omitted when empty or not relevant.
        /// </summary>
        [JsonPropertyName("prev_post_id")]
        public string? PreviousPostId { get; set; }

        /// <summary>
        /// Whether there are more items after this page.
        /// </summary>
        [JsonPropertyName("has_next")]
        public bool HasNext { get; set; }
    }
}
