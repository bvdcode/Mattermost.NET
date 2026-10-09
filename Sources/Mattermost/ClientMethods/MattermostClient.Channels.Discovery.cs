using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Channels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Channel>> GetDeletedChannelsAsync(string teamId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            string query = BuildChannelPageQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, Routes.Teams + "/" + id + "/channels/deleted?" + query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetPrivateChannelsAsync(string teamId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            string query = BuildChannelPageQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, Routes.Teams + "/" + id + "/channels/private?" + query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetPublicChannelsByIdsAsync(string teamId, IEnumerable<string> channelIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareStringBatch(channelIds, nameof(channelIds));
            return SendRequestAsync<IList<Channel>>(HttpMethod.Post, Routes.Teams + "/" + id + "/channels/ids", ids, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Channel>> SearchTeamChannelsAsync(string teamId, string term, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            if (term is null) { throw new ArgumentNullException(nameof(term)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Post, Routes.Teams + "/" + id + "/channels/search",
                new { term = term.Trim() }, cancellationToken);
        }

        private static string BuildChannelPageQuery(int page, int perPage)
        {
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (perPage > MattermostApiLimits.MaxChannelsPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), "Items per page cannot exceed 200.");
            }
            return query;
        }
    }
}
