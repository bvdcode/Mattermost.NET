using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Helpers;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

        /// <summary>
        /// Add current user's reaction to a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="emojiName"> Emoji name without surrounding colons. </param>
        /// <returns> Created reaction information. </returns>
        public async Task<Reaction> AddReactionAsync(string postId, string emojiName)
        {
            CheckDisposed();
            ValidatePostId(postId);
            string sanitizedEmojiName = SanitizeEmojiName(emojiName);
            await CheckAuthorizedAsync().ConfigureAwait(false);
            var body = new
            {
                user_id = CurrentUserInfo.Id,
                post_id = postId,
                emoji_name = sanitizedEmojiName
            };

            return await SendRequestAsync<Reaction>(HttpMethod.Post, Routes.Reactions, body).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a reaction from a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="emojiName"> Emoji name without surrounding colons. </param>
        /// <param name="userId"> User identifier. Defaults to current user. </param>
        public async Task RemoveReactionAsync(string postId, string emojiName, string? userId = null)
        {
            CheckDisposed();
            ValidatePostId(postId);
            string sanitizedEmojiName = SanitizeEmojiName(emojiName);
            await CheckAuthorizedAsync().ConfigureAwait(false);

            string reactionUserId = userId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(reactionUserId))
            {
                reactionUserId = CurrentUserInfo.Id;
            }

            string url = Routes.Users
                + "/" + Uri.EscapeDataString(reactionUserId)
                + "/posts/" + Uri.EscapeDataString(postId)
                + "/reactions/" + Uri.EscapeDataString(sanitizedEmojiName);

            await SendRequestAsync(HttpMethod.Delete, url).ConfigureAwait(false);
        }

        /// <summary>
        /// Get reactions for a post.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Reactions for the post. </returns>
        public Task<IList<Reaction>> GetReactionsAsync(string postId)
        {
            CheckDisposed();
            ValidatePostId(postId);
            string url = Routes.Posts + "/" + Uri.EscapeDataString(postId) + "/reactions";
            return SendRequestAsync<IList<Reaction>>(
                HttpMethod.Get,
                url,
                nullResultFactory: () => new List<Reaction>());
        }

        /// <summary>
        /// Pin a post to its channel.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        public Task PinPostAsync(string postId)
        {
            CheckDisposed();
            ValidatePostId(postId);
            return SendRequestAsync(HttpMethod.Post, Routes.Posts + "/" + Uri.EscapeDataString(postId) + "/pin");
        }

        /// <summary>
        /// Unpin a post from its channel.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        public Task UnpinPostAsync(string postId)
        {
            CheckDisposed();
            ValidatePostId(postId);
            return SendRequestAsync(HttpMethod.Post, Routes.Posts + "/" + Uri.EscapeDataString(postId) + "/unpin");
        }

        private static void ValidatePostId(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
            {
                throw new ArgumentException("Post ID cannot be null or empty.", nameof(postId));
            }
        }

        private static string SanitizeEmojiName(string emojiName)
        {
            if (string.IsNullOrWhiteSpace(emojiName))
            {
                throw new ArgumentException("Emoji name cannot be null or empty.", nameof(emojiName));
            }

            string sanitizedEmojiName = emojiName.Trim().Trim(':');
            if (string.IsNullOrWhiteSpace(sanitizedEmojiName))
            {
                throw new ArgumentException("Emoji name cannot be null or empty.", nameof(emojiName));
            }

            return sanitizedEmojiName;
        }
    }
}
