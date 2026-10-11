using Mattermost.Constants;
using Mattermost.Enums;
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
        public Task<Team> UpdateTeamAsync(Team team, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (team is null) { throw new ArgumentNullException(nameof(team)); }
            string id = EscapeReadIdentifier(team.Id, nameof(team));
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                id = team.Id.Trim(), name = team.Name, display_name = team.DisplayName,
                description = team.Description, email = team.Email, company_name = team.CompanyName,
                type = team.Type, allowed_domains = team.AllowedDomains, allow_open_invite = team.AllowOpenInvite,
                last_team_icon_update = team.LastIconUpdate, group_constrained = team.GroupConstrained
            };
            return SendRequestAsync<Team>(HttpMethod.Put, Routes.Teams + "/" + id, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteTeamAsync(string teamId, bool permanent = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.Teams + "/" + id + "?permanent=" + permanent.ToString().ToLowerInvariant(),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Team> RestoreTeamAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Team>(HttpMethod.Post, Routes.Teams + "/" + id + "/restore", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Team> UpdateTeamPrivacyAsync(string teamId, TeamType privacy, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            string value;
            switch (privacy)
            {
                case TeamType.Open: value = "O"; break;
                case TeamType.InviteOnly: value = "I"; break;
                default: throw new ArgumentOutOfRangeException(nameof(privacy));
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Team>(HttpMethod.Put, Routes.Teams + "/" + id + "/privacy", new { privacy = value }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Team> RegenerateTeamInviteIdAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Team>(HttpMethod.Post, Routes.Teams + "/" + id + "/regenerate_invite_id", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Team> GetTeamInviteInfoAsync(string inviteId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(inviteId, nameof(inviteId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Team>(HttpMethod.Get, Routes.Teams + "/invite/" + id, cancellationToken: cancellationToken,
                requiresAuthorization: false);
        }
    }
}
