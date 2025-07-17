using System;
using System.Linq;
using System.Threading.Tasks;
using Mattermost.Models.Posts;
using System.Collections.Generic;

namespace Mattermost.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="IMattermostClient"/> interface to retrieve all posts in a channel.
    /// </summary>
    public static class MattermostClientExtensions
    {
        /// <summary>
        /// Retrieves all posts in a channel, including deleted posts.
        /// </summary>
        /// <param name="client">The Mattermost client.</param>
        /// <param name="channelId">The identifier of the channel.</param>
        /// <returns>A collection of <see cref="Post"/> objects representing all posts in the channel.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a duplicate post is found.</exception>
        public static async Task<IEnumerable<Post>> GetAllChannelPostsAsync(this IMattermostClient client, string channelId)
        {
            List<Post> allPosts = new List<Post>();
            string? lastPostId = null;
            int page = 0;
            do
            {
                var postsResponse = await client.GetChannelPostsAsync(channelId, page++, perPage: 1000, beforePostId: lastPostId, includeDeleted: true);
                if (postsResponse.Posts == null || !postsResponse.Posts.Any())
                {
                    break;
                }
                foreach (var item in postsResponse.Posts)
                {
                    if (allPosts.Any(x => x.Id == item.Value.Id))
                    {
                        throw new InvalidOperationException($"Duplicate post found: {item.Value.Id} in channel {channelId}");
                    }
                    allPosts.Add(item.Value);
                }
                lastPostId = postsResponse.Order.First();
            } while (lastPostId != null);
            return allPosts;
        }
    }
}
