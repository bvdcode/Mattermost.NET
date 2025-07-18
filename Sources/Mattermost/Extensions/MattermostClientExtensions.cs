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
            int page = 0;
            while (true)
            {
                var postsResponse = await client.GetChannelPostsAsync(channelId, page++, includeDeleted: true);
                if (postsResponse.Posts.Count == 0)
                {
                    break;
                }
                foreach (var item in postsResponse.Posts.Values)
                {
                    if (allPosts.Any(x => x.Id == item.Id))
                    {
                        continue;
                    }
                    allPosts.Add(item);
                }
                if (page > 100000) // Prevent infinite loop in case of an error
                {
                    throw new Exception("Too many pages - possible infinite loop detected.");
                }
            }

            List<Post> result = new List<Post>();
            foreach (var rootPost in allPosts.Where(x => string.IsNullOrWhiteSpace(x.RootId)).OrderBy(x => x.CreatedAt))
            {
                result.Add(rootPost);
                result.AddRange(allPosts.Where(x => x.RootId == rootPost.Id).OrderBy(x => x.CreatedAt));
            }
            return result;
        }
    }
}
