using Mattermost.Constants;
using Mattermost.Enums;
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
        public Task<Team> CreateTeamAsync(string name, string displayName, TeamType type,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateTeamIdentifier(name, nameof(name));
            ValidateTeamIdentifier(displayName, nameof(displayName));
            string teamType;
            switch (type)
            {
                case TeamType.Open: teamType = "O"; break;
                case TeamType.InviteOnly: teamType = "I"; break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { name = name.Trim(), display_name = displayName.Trim(), type = teamType };
            return SendRequestAsync<Team>(HttpMethod.Post, Routes.Teams, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Team> PatchTeamAsync(string teamId, string? displayName = null, string? description = null,
            string? companyName = null, string? allowedDomains = null, bool? allowOpenInvite = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "display_name", displayName);
            AddPatchValue(body, "description", description);
            AddPatchValue(body, "company_name", companyName);
            AddPatchValue(body, "allowed_domains", allowedDomains);
            AddPatchValue(body, "allow_open_invite", allowOpenInvite);
            return SendRequestAsync<Team>(HttpMethod.Put, Routes.Teams + "/" + id + "/patch", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<Team>> SearchTeamsAsync(string term, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (term is null) { throw new ArgumentNullException(nameof(term)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<Team>>(HttpMethod.Post, Routes.Teams + "/search", new { term = term.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TeamMember> AddTeamMemberAsync(string teamId, string userId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(teamId, nameof(teamId));
            ValidateTeamIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { team_id = teamId.Trim(), user_id = userId.Trim() };
            return SendRequestAsync<TeamMember>(HttpMethod.Post, Routes.Teams + "/" + id + "/members", body, cancellationToken);
        }
    }
}
