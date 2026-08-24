using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Teams;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        private static void ValidateTeamIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Team identifier cannot be null or empty.", parameterName);
            }
        }
    }
}
