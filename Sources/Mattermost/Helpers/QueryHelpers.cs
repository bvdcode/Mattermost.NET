using System;
using System.Linq;
using System.Collections.Specialized;

namespace Mattermost.Helpers
{
    /// <summary>
    /// This class contains helper methods for building query strings for API requests.
    /// </summary>
    public class QueryHelpers
    {
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
            if (page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page number cannot be negative.");
            }
            if (perPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), "Items per page cannot be negative or zero.");
            }
            query.Add(nameof(page), page.ToString());
            query.Add("per_page", perPage.ToString());
            query.Add("include_deleted", includeDeleted.ToString());
            if (since != null)
            {
                if (since.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentOutOfRangeException(nameof(since), "Value must be in UTC.");
                }
                if (since < DateTime.UnixEpoch)
                {
                    throw new ArgumentOutOfRangeException(nameof(since), "Value must be greater than or equal to Unix epoch (1970-01-01T00:00:00Z).");
                }
                query.Add(nameof(since), ((DateTimeOffset)since).ToUnixTimeSeconds().ToString());
            }
            if (!string.IsNullOrWhiteSpace(beforePostId))
            {
                query.Add("before", beforePostId);
            }
            if (!string.IsNullOrWhiteSpace(afterPostId))
            {
                query.Add("after", afterPostId);
            }
            return string.Join("&", query.AllKeys.Select(key => $"{key}={query[key]}"));
        }
    }
}