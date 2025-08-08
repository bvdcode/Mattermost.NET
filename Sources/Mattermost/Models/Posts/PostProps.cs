using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Posts
{
    /// <summary>
    /// Post properties that can be attached to a post.
    /// </summary>
    public class PostProps
    {
        /// <summary>
        /// Attached metadata for the post, such as file attachments or links.
        /// </summary>
        [JsonPropertyName("attachments")]
        public IList<PostPropsAttachment> Attachments { get; set; } = new List<PostPropsAttachment>();
    }
}
