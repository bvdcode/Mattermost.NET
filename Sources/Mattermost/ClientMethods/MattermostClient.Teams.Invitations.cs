using Mattermost.Constants;
using Mattermost.Models.Teams;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<TeamMember> JoinTeamFromInviteAsync(string? inviteId = null, string? token = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if ((inviteId is null) == (token is null))
            {
                throw new ArgumentException("Specify either an invitation identifier or an email invitation token, but not both.");
            }
            string query;
            if (inviteId is string id)
            {
                query = "invite_id=" + EscapeReadIdentifier(id, nameof(inviteId));
            }
            else
            {
                query = "token=" + EscapeReadIdentifier(token!, nameof(token));
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TeamMember>(HttpMethod.Post, Routes.Teams + "/members/invite?" + query,
                cancellationToken: cancellationToken);
        }
    }
}
