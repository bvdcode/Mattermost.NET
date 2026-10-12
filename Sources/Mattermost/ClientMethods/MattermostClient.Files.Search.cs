using Mattermost.Constants;
using Mattermost.Models.Responses;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<FileSearchResponse> SearchTeamFilesAsync(string teamId, string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Teams + "/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/files/search";
            return SearchFilesRequestAsync(route, terms, isOrSearch, page, perPage, timeZoneOffset,
                includeDeletedChannels, cancellationToken);
        }

        /// <inheritdoc />
        public Task<FileSearchResponse> SearchFilesAsync(string terms, bool isOrSearch = false,
            int page = 0, int perPage = 60, int timeZoneOffset = 0, bool includeDeletedChannels = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            return SearchFilesRequestAsync(Routes.Files + "/search", terms, isOrSearch, page, perPage, timeZoneOffset,
                includeDeletedChannels, cancellationToken);
        }

        private Task<FileSearchResponse> SearchFilesRequestAsync(string route, string terms, bool isOrSearch,
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
            return SendRequestAsync<FileSearchResponse>(HttpMethod.Post, route, body, cancellationToken);
        }
    }
}
