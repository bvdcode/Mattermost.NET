using System;
using System.Linq;
using System.Collections.Specialized;

namespace Mattermost.Helpers
{
    internal class QueryHelpers
    {
        internal static string BuildChannelPostsQuery(int page, int perPage, string? beforePostId, string? afterPostId, bool includeDeleted, DateTime? since)
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