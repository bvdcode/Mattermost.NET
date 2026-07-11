using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace Mattermost.Helpers
{
    /// <summary>
    /// This class contains helper methods for building query strings for API requests.
    /// </summary>
    public class QueryHelpers
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Builds a query string for paged API requests.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <returns>A query string for the API request.</returns>
        public static string BuildPagedQuery(int page, int perPage)
        {
            NameValueCollection query = new NameValueCollection();
            AddPaging(query, page, perPage);
            return ToQueryString(query);
        }

        /// <summary>
        /// Builds a query string for retrieving users.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <param name="inTeamId">Team identifier to restrict users to.</param>
        /// <param name="notInTeamId">Team identifier to exclude users from.</param>
        /// <param name="inChannelId">Channel identifier to restrict users to.</param>
        /// <param name="notInChannelId">Channel identifier to exclude users from.</param>
        /// <param name="active">Whether to include only active users.</param>
        /// <param name="inactive">Whether to include only inactive users.</param>
        /// <returns>A query string for the API request.</returns>
        public static string BuildUsersQuery(
            int page,
            int perPage,
            string? inTeamId,
            string? notInTeamId,
            string? inChannelId,
            string? notInChannelId,
            bool? active,
            bool? inactive)
        {
            NameValueCollection query = new NameValueCollection();
            AddPaging(query, page, perPage);
            AddIfNotEmpty(query, "in_team", inTeamId);
            AddIfNotEmpty(query, "not_in_team", notInTeamId);
            AddIfNotEmpty(query, "in_channel", inChannelId);
            AddIfNotEmpty(query, "not_in_channel", notInChannelId);
            AddIfHasValue(query, "active", active);
            AddIfHasValue(query, "inactive", inactive);
            return ToQueryString(query);
        }

        /// <summary>
        /// Builds a query string for retrieving posts from a channel.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <param name="beforePostId">The ID of the post before which to retrieve posts.</param>
        /// <param name="afterPostId">The ID of the post after which to retrieve posts.</param>
        /// <param name="includeDeleted">Whether to include deleted posts.</param>
        /// <param name="since">The date and time since which to retrieve posts, must be in UTC.</param>
        /// <returns>A query string for the API request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when page or perPage is negative.</exception>
        public static string BuildChannelPostsQuery(int page, int perPage, string? beforePostId, string? afterPostId, bool includeDeleted, DateTime? since)
        {
            NameValueCollection query = new NameValueCollection();
            AddPaging(query, page, perPage);
            query.Add("include_deleted", includeDeleted.ToString());
            if (since != null)
            {
                if (since.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentOutOfRangeException(nameof(since), "Value must be in UTC.");
                }
                if (since < UnixEpoch)
                {
                    throw new ArgumentOutOfRangeException(nameof(since), "Value must be greater than or equal to Unix epoch (1970-01-01T00:00:00Z).");
                }
                query.Add(nameof(since), new DateTimeOffset(since.Value).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(beforePostId))
            {
                query.Add("before", beforePostId);
            }
            if (!string.IsNullOrWhiteSpace(afterPostId))
            {
                query.Add("after", afterPostId);
            }
            return ToQueryString(query);
        }

        private static void AddPaging(NameValueCollection query, int page, int perPage)
        {
            if (page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page number cannot be negative.");
            }

            if (perPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), "Items per page cannot be negative or zero.");
            }

            query.Add(nameof(page), page.ToString(CultureInfo.InvariantCulture));
            query.Add("per_page", perPage.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddIfNotEmpty(NameValueCollection query, string name, string? value)
        {
            if (value is null)
            {
                return;
            }

            string trimmedValue = value.Trim();
            if (trimmedValue.Length > 0)
            {
                query.Add(name, Uri.EscapeDataString(trimmedValue));
            }
        }

        private static void AddIfHasValue(NameValueCollection query, string name, bool? value)
        {
            if (value.HasValue)
            {
                query.Add(name, value.Value.ToString().ToLowerInvariant());
            }
        }

        private static string ToQueryString(NameValueCollection query)
        {
            List<string> parts = new List<string>();
            foreach (string? key in query.AllKeys)
            {
                if (key is null)
                {
                    continue;
                }

                parts.Add($"{key}={query[key]}");
            }

            return string.Join("&", parts);
        }
    }
}
