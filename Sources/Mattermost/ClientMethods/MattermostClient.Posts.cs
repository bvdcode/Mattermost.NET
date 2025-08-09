using System;
using System.Net.Http;
using Mattermost.Enums;
using Mattermost.Helpers;
using Mattermost.Constants;
using System.Threading.Tasks;
using Mattermost.Models.Posts;
using System.Collections.Generic;
using Mattermost.Models.Responses;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Send message to specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="rawProps"> A general JSON property bag to attach to the post. </param>
        /// <returns> Created post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        public Task<Post> CreatePostWithRawPropsAsync(string channelId, string message = "",
            string replyToPostId = "", MessagePriority priority = MessagePriority.Empty,
            IEnumerable<string>? files = null, IDictionary<string, object>? rawProps = null)
        {
            CheckDisposed();
            if (message.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(message),
                    $"The message length exceeds the maximum number of characters allowed ({message.Length} > {MattermostApiLimits.MaxPostMessageLength})");
            }

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            if (priority != MessagePriority.Empty)
            {
                metadata.Add("priority", new
                {
                    priority = priority.ToString().ToLower(),
                    requested_ack = false
                });
            }

            var body = new
            {
                message,
                channel_id = channelId,
                root_id = replyToPostId,
                metadata,
                file_ids = files,
                props = rawProps
            };
            return SendRequestAsync<Post>(HttpMethod.Post, Routes.Posts, body);
        }

        /// <summary>
        /// Send message to specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="props"> Props object to attach to the post. </param>
        /// <returns> Created post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        public Task<Post> CreatePostAsync(string channelId, string message = "",
            string replyToPostId = "", MessagePriority priority = MessagePriority.Empty,
            IEnumerable<string>? files = null, PostProps? props = null)
        {
            CheckDisposed();
            if (message.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(message),
                    $"The message length exceeds the maximum number of characters allowed ({message.Length} > {MattermostApiLimits.MaxPostMessageLength})");
            }
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            if (priority != MessagePriority.Empty)
            {
                metadata.Add("priority", new
                {
                    priority = priority.ToString().ToLower(),
                    requested_ack = false
                });
            }

            var body = new
            {
                message,
                channel_id = channelId,
                root_id = replyToPostId,
                metadata,
                file_ids = files,
                props
            };
            return SendRequestAsync<Post>(HttpMethod.Post, Routes.Posts, body);
        }

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="rawProps"> A general JSON property bag to attach to the post. </param>
        /// <returns> Updated post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        public Task<Post> UpdatePostWithRawPropsAsync(string postId, string newText, IDictionary<string, object>? rawProps = null)
        {
            CheckDisposed();
            if (newText.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(newText),
                    $"The message length exceeds the maximum number of characters allowed ({newText.Length} > {MattermostApiLimits.MaxPostMessageLength})");
            }
            var body = new
            {
                message = newText,
                props = rawProps
            };
            return SendRequestAsync<Post>(HttpMethod.Put, Routes.Posts + "/" + postId + "/patch", body);
        }

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="props"> Props object to attach to the post. </param>
        /// <returns> Updated post. </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when message length exceed maximum limit of characters, see <see cref="MattermostApiLimits.MaxPostMessageLength"/>.</exception>
        public Task<Post> UpdatePostAsync(string postId, string newText, PostProps? props = null)
        {
            CheckDisposed();
            if (newText.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(newText),
                    $"The message length exceeds the maximum number of characters allowed ({newText.Length} > {MattermostApiLimits.MaxPostMessageLength})");
            }
            var body = new
            {
                message = newText,
                props
            };
            return SendRequestAsync<Post>(HttpMethod.Put, Routes.Posts + "/" + postId + "/patch", body);
        }

        /// <summary>
        /// Delete post with specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        public Task DeletePostAsync(string postId)
        {
            CheckDisposed();
            return SendRequestAsync(HttpMethod.Delete, Routes.Posts + "/" + postId);
        }

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of posts per page. </param>
        /// <param name="beforePostId"> A post id to select the posts that came before this one. </param>
        /// <param name="afterPostId"> A post id to select the posts that came after this one. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <param name="since"> Time to select modified posts after. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        public Task<ChannelPostsResponse> GetChannelPostsAsync(string channelId, int page = 0,
            int perPage = 60, string? beforePostId = null, string? afterPostId = null,
            bool includeDeleted = false, DateTime? since = null)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildChannelPostsQuery(page, perPage, beforePostId, afterPostId, includeDeleted, since);
            string url = $"{Routes.Channels}/{channelId}/posts?{query}";
            return SendRequestAsync<ChannelPostsResponse>(HttpMethod.Get, url);
        }

        /// <summary>
        /// Get posts related to specified post identifier in thread format.
        /// </summary>
        /// <param name="postId"> Post identifier to get thread posts. </param>
        /// <param name="fromPostId"> Post identifier to start from. </param>
        /// <returns> Collection of posts in thread format. </returns>
        public Task<ChannelPostsResponse> GetThreadPostsAsync(string postId, string? fromPostId = null)
        {
            CheckDisposed();
            string url = $"{Routes.Posts}/{postId}/thread";
            if (!string.IsNullOrEmpty(fromPostId))
            {
                url += $"?fromPost={fromPostId}";
            }
            return SendRequestAsync<ChannelPostsResponse>(HttpMethod.Get, url);
        }

        /// <summary>
        /// Get post by identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Post information. </returns>
        public Task<Post> GetPostAsync(string postId)
        {
            CheckDisposed();
            return SendRequestAsync<Post>(HttpMethod.Get, Routes.Posts + "/" + postId);
        }
    }
}
