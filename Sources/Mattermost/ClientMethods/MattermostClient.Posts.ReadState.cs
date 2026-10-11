using Mattermost.Constants;
using Mattermost.Models.Channels;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<ChannelReadState> MarkPostUnreadAsync(string userId, string postId, bool collapsedThreadsSupported = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/posts/"
                + EscapeReadIdentifier(postId, nameof(postId)) + "/set_unread";
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { collapsed_threads_supported = collapsedThreadsSupported };
            return SendRequestAsync<ChannelReadState>(HttpMethod.Post, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task SetPostReminderAsync(string userId, string postId, DateTimeOffset remindAt, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/posts/"
                + EscapeReadIdentifier(postId, nameof(postId)) + "/reminder";
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { target_time = remindAt.ToUnixTimeSeconds() };
            return SendRequestAsync(HttpMethod.Post, route, body, cancellationToken);
        }
    }
}
