using Mattermost.Constants;
using Mattermost.Models.Posts;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<Post> CreateEphemeralPostAsync(string userId, string channelId, string message = "", string replyToPostId = "",
            PostProps? props = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(userId) || userId.Trim() == "me")
            {
                throw new ArgumentException("An actual recipient user identifier is required.", nameof(userId));
            }
            if (string.IsNullOrWhiteSpace(channelId)) { throw new ArgumentException("Channel identifier cannot be null or empty.", nameof(channelId)); }
            if (message is null) { throw new ArgumentNullException(nameof(message)); }
            if (message.Length > MattermostApiLimits.MaxPostMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(message), "Message exceeds the maximum supported length.");
            }
            if (replyToPostId is null) { throw new ArgumentNullException(nameof(replyToPostId)); }
            cancellationToken.ThrowIfCancellationRequested();
            var post = new { channel_id = channelId.Trim(), message, root_id = replyToPostId.Trim(), props };
            return SendRequestAsync<Post>(HttpMethod.Post, Routes.Posts + "/ephemeral", new { user_id = userId.Trim(), post }, cancellationToken);
        }
    }
}
