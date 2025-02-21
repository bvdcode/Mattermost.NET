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
        /// <param name="since">The timestamp in milliseconds since the epoch to filter posts.</param>
        /// <returns>A query string for the API request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when page or perPage is negative.</exception>
        public static string BuildChannelPostsQuery(int page, int perPage, string? beforePostId, string? afterPostId, bool includeDeleted, DateTime? since)
        {
            NameValueCollection query = new NameValueCollection();
            if (page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page number cannot be negative.");
            }
            if (perPage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage), "Items per page cannot be negative.");
            }
            query.Add(nameof(page), page.ToString());
            query.Add("per_page", perPage.ToString());
            query.Add("include_deleted", includeDeleted.ToString());
            if (since != null)
            {
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