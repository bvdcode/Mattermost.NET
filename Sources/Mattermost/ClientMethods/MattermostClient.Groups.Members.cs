using Mattermost.Models.Groups;
using Mattermost.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<GroupMemberList> GetGroupMembersAsync(string groupId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/members?" + QueryHelpers.BuildPagedQuery(page, perPage);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupMemberList>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<GroupMember>> AddGroupMembersAsync(string groupId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default) => UpdateGroupMembersAsync(HttpMethod.Post, groupId, userIds, cancellationToken);

        /// <inheritdoc />
        public Task<IList<GroupMember>> RemoveGroupMembersAsync(string groupId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default) => UpdateGroupMembersAsync(HttpMethod.Delete, groupId, userIds, cancellationToken);

        /// <inheritdoc />
        public Task<GroupStats> GetGroupStatsAsync(string groupId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/stats";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<GroupStats>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        private Task<IList<GroupMember>> UpdateGroupMembersAsync(HttpMethod method, string groupId, IEnumerable<string> userIds,
            CancellationToken cancellationToken)
        {
            CheckDisposed();
            string route = GetGroupRoute(groupId) + "/members";
            List<string> ids = PrepareGroupStringList(userIds, nameof(userIds), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<GroupMember>>(method, route, new { user_ids = ids }, cancellationToken,
                nullResultFactory: () => new List<GroupMember>());
        }
    }
}
