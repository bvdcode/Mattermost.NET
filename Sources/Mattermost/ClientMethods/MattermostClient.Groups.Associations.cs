using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Groups;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<Group>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Users + "/" + EscapeReadIdentifier(userId, nameof(userId)) + "/groups";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Group>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<GroupAssociationList> GetChannelGroupsAsync(string channelId, int page = 0, int perPage = 60,
            string? query = null, bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Channels + "/" + EscapeReadIdentifier(channelId, nameof(channelId)) + "/groups?"
                + BuildGroupAssociationQuery(page, perPage, query, includeMemberCount, filterAllowReference, paginate);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupAssociationList>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<GroupAssociationList> GetTeamGroupsAsync(string teamId, int page = 0, int perPage = 60,
            string? query = null, bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Teams + "/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/groups?"
                + BuildGroupAssociationQuery(page, perPage, query, includeMemberCount, filterAllowReference, paginate);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupAssociationList>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<TeamChannelGroups> GetTeamChannelGroupsAsync(string teamId, int page = 0, int perPage = 60,
            string? query = null, bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.Teams + "/" + EscapeReadIdentifier(teamId, nameof(teamId)) + "/groups_by_channels?"
                + BuildGroupAssociationQuery(page, perPage, query, includeMemberCount, filterAllowReference, paginate);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TeamChannelGroups>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        private static string BuildGroupAssociationQuery(int page, int perPage, string? query, bool includeMemberCount,
            bool filterAllowReference, bool paginate)
        {
            string result = QueryHelpers.BuildPagedQuery(page, perPage)
                + "&include_member_count=" + includeMemberCount.ToString().ToLowerInvariant()
                + "&filter_allow_reference=" + filterAllowReference.ToString().ToLowerInvariant()
                + "&paginate=" + paginate.ToString().ToLowerInvariant();
            if (query is string text) { result += "&q=" + System.Uri.EscapeDataString(text.Trim()); }
            return result;
        }
    }
}
