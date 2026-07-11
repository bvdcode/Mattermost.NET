using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Get current authorized user information and force update of <see cref="CurrentUserInfo"/>.
        /// </summary>
        /// <returns> Authorized user information. </returns>
        public async Task<User> GetMeAsync()
        {
            CheckDisposed();
            _cachedUserInfo = await SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/me");
            return _cachedUserInfo.MemberwiseClone();
        }

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserAsync(string userId)
        {
            CheckDisposed();
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            return SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/" + userId);
        }

        /// <summary>
        /// Get a page of users.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of users per page. </param>
        /// <param name="inTeamId"> Only users in this team. </param>
        /// <param name="notInTeamId"> Only users not in this team. </param>
        /// <param name="inChannelId"> Only users in this channel. </param>
        /// <param name="notInChannelId"> Only users not in this channel. </param>
        /// <param name="active"> Only active users. </param>
        /// <param name="inactive"> Only inactive users. </param>
        /// <returns> Users matching the query. </returns>
        public Task<IList<User>> GetUsersAsync(
            int page = 0,
            int perPage = 60,
            string? inTeamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool? active = null,
            bool? inactive = null)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildUsersQuery(page, perPage, inTeamId, notInTeamId, inChannelId, notInChannelId, active, inactive);
            return SendRequestAsync<IList<User>>(HttpMethod.Get, Routes.Users + "?" + query);
        }

        /// <summary>
        /// Search users by term.
        /// </summary>
        /// <param name="term"> Search term matched against username, full name, nickname and email. </param>
        /// <param name="teamId"> Only search users on this team. </param>
        /// <param name="notInTeamId"> Only search users not on this team. </param>
        /// <param name="inChannelId"> Only search users in this channel. </param>
        /// <param name="notInChannelId"> Only search users not in this channel. Must specify teamId when using this option. </param>
        /// <param name="groupConstrained"> Only users allowed to join based on group constraints. </param>
        /// <param name="allowInactive"> Include deactivated users in the results. </param>
        /// <param name="withoutTeam"> Search users that are not on a team. </param>
        /// <param name="limit"> Maximum number of users to return. </param>
        /// <returns> Users matching the search term. </returns>
        public Task<IList<User>> SearchUsersAsync(
            string term,
            string? teamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool groupConstrained = false,
            bool allowInactive = false,
            bool withoutTeam = false,
            int? limit = null)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("Search term cannot be null or empty.", nameof(term));
            }

            if (limit.HasValue && limit.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero.");
            }

            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["term"] = term.Trim()
            };
            AddIfNotEmpty(body, "team_id", teamId);
            AddIfNotEmpty(body, "not_in_team_id", notInTeamId);
            AddIfNotEmpty(body, "in_channel_id", inChannelId);
            AddIfNotEmpty(body, "not_in_channel_id", notInChannelId);
            AddIfTrue(body, "group_constrained", groupConstrained);
            AddIfTrue(body, "allow_inactive", allowInactive);
            AddIfTrue(body, "without_team", withoutTeam);
            if (limit.HasValue)
            {
                body.Add("limit", limit.Value);
            }

            return SendRequestAsync<IList<User>>(HttpMethod.Post, Routes.Users + "/search", body);
        }

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserByUsernameAsync(string username)
        {
            CheckDisposed();
            string sanitizedUsername = username.Replace("@", string.Empty).Trim();
            if (string.IsNullOrEmpty(sanitizedUsername))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }
            return SendRequestAsync<User>(HttpMethod.Get, Routes.Users + "/username/" + sanitizedUsername);
        }

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email"> Email address. </param>
        /// <returns> User information. </returns>
        public Task<User> GetUserByEmailAsync(string email)
        {
            CheckDisposed();
            string url = Routes.Users + "/email/" + email.Trim();
            return SendRequestAsync<User>(HttpMethod.Get, url);
        }

        private static void AddIfNotEmpty(IDictionary<string, object> body, string name, string? value)
        {
            if (value is null)
            {
                return;
            }

            string trimmedValue = value.Trim();
            if (trimmedValue.Length > 0)
            {
                body.Add(name, trimmedValue);
            }
        }

        private static void AddIfTrue(IDictionary<string, object> body, string name, bool value)
        {
            if (value)
            {
                body.Add(name, value);
            }
        }
    }
}
