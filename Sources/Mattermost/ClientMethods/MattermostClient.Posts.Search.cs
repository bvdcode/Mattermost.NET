using Mattermost.Constants;
using Mattermost.Helpers;
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
        public Task<PostSearchResponse> SearchTeamPostsAsync(string teamId, string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            return SearchPostsRequestAsync(Routes.Teams + "/" + id + "/posts/search", terms, isOrSearch,
                page, perPage, timeZoneOffset, includeDeletedChannels, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PostSearchResponse> SearchPostsAsync(string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            return SearchPostsRequestAsync(Routes.Posts + "/search", terms, isOrSearch, page, perPage,
                timeZoneOffset, includeDeletedChannels, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelPostsResponse> GetFlaggedPostsAsync(string userId, string? teamId = null,
            string? channelId = null, int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(userId, nameof(userId));
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (teamId is string team && !string.IsNullOrWhiteSpace(team)) { query += "&team_id=" + Uri.EscapeDataString(team.Trim()); }
            if (channelId is string channel && !string.IsNullOrWhiteSpace(channel)) { query += "&channel_id=" + Uri.EscapeDataString(channel.Trim()); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ChannelPostsResponse>(HttpMethod.Get, Routes.Users + "/" + id + "/posts/flagged?" + query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelPostsResponse> GetUnreadPostsAsync(string userId, string channelId,
            int limitBefore = 60, int limitAfter = 60, bool skipFetchThreads = false, bool collapsedThreads = false,
            bool collapsedThreadsExtended = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string user = EscapeReadIdentifier(userId, nameof(userId));
            string channel = EscapeReadIdentifier(channelId, nameof(channelId));
            if (limitBefore < 0 || limitBefore > MattermostApiLimits.MaxPostsAroundUnread)
            {
                throw new ArgumentOutOfRangeException(nameof(limitBefore));
            }
            if (limitAfter < 1 || limitAfter > MattermostApiLimits.MaxPostsAroundUnread)
            {
                throw new ArgumentOutOfRangeException(nameof(limitAfter));
            }
            string query = "limit_before=" + limitBefore.ToString(CultureInfo.InvariantCulture)
                + "&limit_after=" + limitAfter.ToString(CultureInfo.InvariantCulture)
                + "&skipFetchThreads=" + skipFetchThreads.ToString().ToLowerInvariant()
                + "&collapsedThreads=" + collapsedThreads.ToString().ToLowerInvariant()
                + "&collapsedThreadsExtended=" + collapsedThreadsExtended.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ChannelPostsResponse>(HttpMethod.Get, Routes.Users + "/" + user + "/channels/" + channel + "/posts/unread?" + query,
                cancellationToken: cancellationToken);
        }

        private Task<PostSearchResponse> SearchPostsRequestAsync(string route, string terms, bool isOrSearch,
            int page, int perPage, int timeZoneOffset, bool includeDeletedChannels, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(terms)) { throw new ArgumentException("Search terms cannot be blank.", nameof(terms)); }
            if (page < 0) { throw new ArgumentOutOfRangeException(nameof(page)); }
            if (perPage < 1) { throw new ArgumentOutOfRangeException(nameof(perPage)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                terms = terms.Trim(), is_or_search = isOrSearch, page, per_page = perPage,
                time_zone_offset = timeZoneOffset, include_deleted_channels = includeDeletedChannels
            };
            return SendRequestAsync<PostSearchResponse>(HttpMethod.Post, route, body, cancellationToken);
        }
    }
}
