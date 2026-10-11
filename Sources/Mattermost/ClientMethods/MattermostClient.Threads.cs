using Mattermost.Constants;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<UserThreadsResponse> GetUserThreadsAsync(string userId, string teamId, int perPage = 60,
            string? beforeThreadId = null, string? afterThreadId = null, long since = 0, bool unread = false,
            bool extended = false, bool deleted = false, bool totalsOnly = false, bool threadsOnly = false,
            bool excludeDirect = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadsRoute(userId, teamId);
            if (perPage < 1) { throw new ArgumentOutOfRangeException(nameof(perPage)); }
            if (since < 0) { throw new ArgumentOutOfRangeException(nameof(since)); }
            if (!string.IsNullOrWhiteSpace(beforeThreadId) && !string.IsNullOrWhiteSpace(afterThreadId))
            {
                throw new ArgumentException("Before and after cursors are mutually exclusive.");
            }
            if (totalsOnly && threadsOnly) { throw new ArgumentException("Totals-only and threads-only are mutually exclusive."); }
            string query = "per_page=" + perPage.ToString(CultureInfo.InvariantCulture)
                + "&since=" + since.ToString(CultureInfo.InvariantCulture)
                + "&unread=" + unread.ToString().ToLowerInvariant()
                + "&extended=" + extended.ToString().ToLowerInvariant()
                + "&deleted=" + deleted.ToString().ToLowerInvariant()
                + "&totalsOnly=" + totalsOnly.ToString().ToLowerInvariant()
                + "&threadsOnly=" + threadsOnly.ToString().ToLowerInvariant()
                + "&excludeDirect=" + excludeDirect.ToString().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(beforeThreadId)) { query += "&before=" + Uri.EscapeDataString(beforeThreadId!.Trim()); }
            if (!string.IsNullOrWhiteSpace(afterThreadId)) { query += "&after=" + Uri.EscapeDataString(afterThreadId!.Trim()); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserThreadsResponse>(HttpMethod.Get, route + "?" + query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserThread> GetUserThreadAsync(string userId, string teamId, string threadId, bool extended = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadRoute(userId, teamId, threadId) + "?extended=" + extended.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserThread>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task FollowThreadAsync(string userId, string teamId, string threadId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadRoute(userId, teamId, threadId) + "/following";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UnfollowThreadAsync(string userId, string teamId, string threadId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadRoute(userId, teamId, threadId) + "/following";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserThread> UpdateThreadReadAsync(string userId, string teamId, string threadId, long timestamp,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadRoute(userId, teamId, threadId);
            if (timestamp <= 0) { throw new ArgumentOutOfRangeException(nameof(timestamp)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserThread>(HttpMethod.Put, route + "/read/" + timestamp.ToString(CultureInfo.InvariantCulture),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserThread> MarkThreadUnreadAsync(string userId, string teamId, string threadId, string postId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadRoute(userId, teamId, threadId) + "/set_unread/" + EscapeReadIdentifier(postId, nameof(postId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<UserThread>(HttpMethod.Post, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task MarkAllThreadsReadAsync(string userId, string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetUserThreadsRoute(userId, teamId) + "/read";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, cancellationToken: cancellationToken);
        }

        private static string GetUserThreadsRoute(string userId, string teamId)
        {
            return Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId))
                + "/teams/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/threads";
        }

        private static string GetUserThreadRoute(string userId, string teamId, string threadId) =>
            GetUserThreadsRoute(userId, teamId) + "/" + EscapeReadIdentifier(threadId, nameof(threadId));
    }
}
