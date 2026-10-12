using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Responses;
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
        public Task<ChannelListResponse> GetAllChannelsAsync(int page = 0, int perPage = 60, bool includeDeleted = false,
            bool excludeDefaultChannels = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string query = BuildChannelPageQuery(page, perPage) + "&include_total_count=true&include_deleted="
                + includeDeleted.ToString().ToLowerInvariant() + "&exclude_default_channels=" + excludeDefaultChannels.ToString().ToLowerInvariant();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ChannelListResponse>(HttpMethod.Get, Routes.Channels + "?" + query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<ChannelListResponse> SearchAllChannelsAsync(string term, int page = 0, int perPage = 60,
            IEnumerable<string>? teamIds = null, ChannelType? channelType = null, bool includeDeleted = false,
            bool excludeDefaultChannels = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (term is null) { throw new ArgumentNullException(nameof(term)); }
            BuildChannelPageQuery(page, perPage);
            bool publicChannels = false;
            bool privateChannels = false;
            if (channelType is ChannelType type)
            {
                switch (type)
                {
                    case ChannelType.Public: publicChannels = true; break;
                    case ChannelType.Private: privateChannels = true; break;
                    default: throw new ArgumentOutOfRangeException(nameof(channelType), "Only public or private channels can be selected.");
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            List<string>? selectedTeams = null;
            if (teamIds is object)
            {
                selectedTeams = new List<string>();
                HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (string team in teamIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateTeamIdentifier(team, nameof(teamIds));
                    string id = team.Trim();
                    if (seen.Add(id)) { selectedTeams.Add(id); }
                }
            }
            var body = new
            {
                term = term.Trim(), page, per_page = perPage, team_ids = selectedTeams,
                @public = publicChannels, @private = privateChannels, include_deleted = includeDeleted,
                exclude_default_channels = excludeDefaultChannels
            };
            return SendRequestAsync<ChannelListResponse>(HttpMethod.Post, Routes.Channels + "/search?system_console=true", body, cancellationToken);
        }
    }
}
