using Mattermost.Constants;
using Mattermost.Models.Channels;
using Mattermost.Models.Teams;
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
        public Task<IList<Channel>> AutocompleteChannelsAsync(string teamId, string name, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get,
                Routes.Teams + "/" + id + "/channels/autocomplete?name=" + Uri.EscapeDataString(name.Trim()),
                cancellationToken: cancellationToken, nullResultFactory: () => new List<Channel>());
        }

        /// <inheritdoc />
        public Task<IList<Channel>> AutocompleteChannelsForSearchAsync(string teamId, string name, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            if (name is null) { throw new ArgumentNullException(nameof(name)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get,
                Routes.Teams + "/" + id + "/channels/search_autocomplete?name=" + Uri.EscapeDataString(name.Trim()),
                cancellationToken: cancellationToken, nullResultFactory: () => new List<Channel>());
        }

        /// <inheritdoc />
        public Task<IList<Channel>> GetRecommendedChannelsAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Channel>>(HttpMethod.Get, Routes.Teams + "/" + id + "/channels/recommended",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<Channel>());
        }

        /// <inheritdoc />
        public Task<IList<Team>> GetChannelMembersCommonTeamsAsync(string channelId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Team>>(HttpMethod.Get, Routes.Channels + "/" + id + "/common_teams",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<Team>());
        }
    }
}
