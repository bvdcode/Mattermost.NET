using Mattermost.Constants;
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
        public Task<IList<TeamMember>> AddTeamMembersAsync(string teamId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            List<string> users = PrepareStringBatch(userIds, nameof(userIds));
            if (users.Count > MattermostApiLimits.MaxTeamMembersPerRequest)
            {
                throw new ArgumentException("At most 256 users can be added in one request.", nameof(userIds));
            }
            List<object> body = new List<object>();
            foreach (string userId in users) { body.Add(new { team_id = teamId.Trim(), user_id = userId }); }
            return SendRequestAsync<IList<TeamMember>>(HttpMethod.Post, Routes.Teams + "/" + id + "/members/batch", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetTeamMemberRoute(teamId, userId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateTeamMemberRolesAsync(string teamId, string userId, string roles,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetTeamMemberRoute(teamId, userId) + "/roles";
            if (roles is null) { throw new ArgumentNullException(nameof(roles)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, route, new { roles }, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateTeamMemberSchemeRolesAsync(string teamId, string userId, bool schemeUser, bool schemeAdmin,
            bool schemeGuest = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetTeamMemberRoute(teamId, userId) + "/schemeRoles";
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { scheme_user = schemeUser, scheme_admin = schemeAdmin, scheme_guest = schemeGuest };
            return SendRequestAsync(HttpMethod.Put, route, body, cancellationToken);
        }

        private static string GetTeamMemberRoute(string teamId, string userId) => Routes.Teams + "/"
            + EscapeReadIdentifier(teamId, nameof(teamId)) + "/members/" + EscapeReadIdentifier(userId, nameof(userId));
    }
}
