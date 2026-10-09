using Mattermost.Constants;
using Mattermost.Helpers;
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
        /// <summary>
        /// Get team by specified identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <returns> Team information. </returns>
        public Task<Team> GetTeamAsync(string teamId)
        {
            CheckDisposed();
            ValidateTeamIdentifier(teamId, nameof(teamId));
            return SendRequestAsync<Team>(HttpMethod.Get, Routes.Teams + "/" + Uri.EscapeDataString(teamId.Trim()));
        }

        /// <summary>
        /// Get a page of teams.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of teams per page. </param>
        /// <returns> Teams visible to the current user. </returns>
        public Task<IList<Team>> GetTeamsAsync(int page = 0, int perPage = 60)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            return SendRequestAsync<IList<Team>>(HttpMethod.Get, Routes.Teams + "?" + query);
        }

        /// <summary>
        /// Get teams for a specified user.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Teams the user belongs to. </returns>
        public Task<IReadOnlyList<Team>> GetUserTeamsAsync(string userId)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User identifier cannot be null or empty.", nameof(userId));
            }

            string url = Routes.Users + "/" + Uri.EscapeDataString(userId.Trim()) + "/teams";
            return SendRequestAsync<IReadOnlyList<Team>>(HttpMethod.Get, url);
        }

        /// <summary>
        /// Get team by name.
        /// </summary>
        /// <param name="teamName"> Team name. </param>
        /// <returns> Team information. </returns>
        public Task<Team> GetTeamByNameAsync(string teamName)
        {
            CheckDisposed();
            ValidateTeamIdentifier(teamName, nameof(teamName));
            return SendRequestAsync<Team>(HttpMethod.Get, Routes.Teams + "/name/" + Uri.EscapeDataString(teamName.Trim()));
        }

        /// <inheritdoc />
        public Task<IList<TeamMember>> GetTeamMembersAsync(string teamId, int page = 0, int perPage = 60,
            bool sortByUsername = false, bool excludeDeletedUsers = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamId = EscapeTeamReadIdentifier(teamId, nameof(teamId));
            if (perPage > MattermostApiLimits.MaxTeamMembersPerPage)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), $"At most {MattermostApiLimits.MaxTeamMembersPerPage} members can be requested per page.");
            }
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (sortByUsername)
            {
                query += "&sort=Username";
            }
            if (excludeDeletedUsers)
            {
                query += "&exclude_deleted_users=true";
            }
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Teams + "/" + escapedTeamId + "/members?" + query;
            return SendRequestAsync<IList<TeamMember>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<TeamMember> GetTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamId = EscapeTeamReadIdentifier(teamId, nameof(teamId));
            string escapedUserId = EscapeTeamReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Teams + "/" + escapedTeamId + "/members/" + escapedUserId;
            return SendRequestAsync<TeamMember>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TeamMember>> GetTeamMembersByIdsAsync(string teamId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamId = EscapeTeamReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            List<string> ids = PrepareUserBatch(userIds, nameof(userIds));
            string url = Routes.Teams + "/" + escapedTeamId + "/members/ids";
            return SendRequestAsync<IList<TeamMember>>(HttpMethod.Post, url, ids, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TeamMember>> GetUserTeamMembersAsync(string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedUserId = EscapeTeamReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Users + "/" + escapedUserId + "/teams/members";
            return SendRequestAsync<IList<TeamMember>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<TeamStats> GetTeamStatsAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamId = EscapeTeamReadIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TeamStats>(HttpMethod.Get, Routes.Teams + "/" + escapedTeamId + "/stats",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> TeamExistsAsync(string teamName, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamName = EscapeTeamReadIdentifier(teamName, nameof(teamName));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Teams + "/name/" + escapedTeamName + "/exists";
            Dictionary<string, bool> response = await SendRequestAsync<Dictionary<string, bool>>(HttpMethod.Get, url,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return response["exists"];
        }

        /// <inheritdoc />
        public Task<TeamUnread> GetTeamUnreadAsync(string teamId, string userId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedTeamId = EscapeTeamReadIdentifier(teamId, nameof(teamId));
            string escapedUserId = EscapeTeamReadIdentifier(userId, nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            string url = Routes.Users + "/" + escapedUserId + "/teams/" + escapedTeamId + "/unread";
            return SendRequestAsync<TeamUnread>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TeamUnread>> GetUserTeamsUnreadAsync(string userId, string? excludeTeamId = null,
            bool includeCollapsedThreads = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string escapedUserId = EscapeTeamReadIdentifier(userId, nameof(userId));
            string url = Routes.Users + "/" + escapedUserId + "/teams/unread"
                + "?include_collapsed_threads=" + includeCollapsedThreads.ToString().ToLowerInvariant();
            if (excludeTeamId is string excludedTeamId && !string.IsNullOrWhiteSpace(excludedTeamId))
            {
                url += "&exclude_team=" + Uri.EscapeDataString(excludedTeamId.Trim());
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<TeamUnread>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        private static string EscapeTeamReadIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Identifier cannot be null or empty.", parameterName);
            }
            return Uri.EscapeDataString(value.Trim());
        }

        private static void ValidateTeamIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Team identifier cannot be null or empty.", parameterName);
            }
        }
    }
}
