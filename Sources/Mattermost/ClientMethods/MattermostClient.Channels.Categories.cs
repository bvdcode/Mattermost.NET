using Mattermost.Constants;
using Mattermost.Models.Channels;
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
        public Task<OrderedSidebarCategories> GetSidebarCategoriesAsync(string userId, string teamId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OrderedSidebarCategories>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<SidebarCategory> GetSidebarCategoryAsync(string userId, string teamId, string categoryId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId) + "/" + EscapeReadIdentifier(categoryId, nameof(categoryId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<SidebarCategory>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<string>> GetSidebarCategoryOrderAsync(string userId, string teamId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId) + "/order";
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<string>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<SidebarCategory> CreateSidebarCategoryAsync(SidebarCategory category,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (category is null) { throw new ArgumentNullException(nameof(category)); }
            string route = GetSidebarCategoriesRoute(category.UserId, category.TeamId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<SidebarCategory>(HttpMethod.Post, route, CopySidebarCategory(category), cancellationToken);
        }

        /// <inheritdoc />
        public Task<SidebarCategory> UpdateSidebarCategoryAsync(SidebarCategory category,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (category is null) { throw new ArgumentNullException(nameof(category)); }
            string route = GetSidebarCategoriesRoute(category.UserId, category.TeamId) + "/"
                + EscapeReadIdentifier(category.Id, nameof(category));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<SidebarCategory>(HttpMethod.Put, route, CopySidebarCategory(category), cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<SidebarCategory>> UpdateSidebarCategoriesAsync(string userId, string teamId,
            IEnumerable<SidebarCategory> categories, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId);
            if (categories is null) { throw new ArgumentNullException(nameof(categories)); }
            cancellationToken.ThrowIfCancellationRequested();
            List<SidebarCategory> body = new List<SidebarCategory>();
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SidebarCategory category in categories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (category is null || string.IsNullOrWhiteSpace(category.Id))
                {
                    throw new ArgumentException("Each category requires an identifier.", nameof(categories));
                }
                if (!ids.Add(category.Id))
                {
                    throw new ArgumentException("Category identifiers must be unique.", nameof(categories));
                }
                GetSidebarCategoriesRoute(category.UserId, category.TeamId);
                if (category.UserId.Trim() != userId.Trim() || category.TeamId.Trim() != teamId.Trim())
                {
                    throw new ArgumentException("Each category must belong to the supplied user and team.", nameof(categories));
                }
                body.Add(CopySidebarCategory(category));
            }
            if (body.Count == 0) { throw new ArgumentException("At least one category is required.", nameof(categories)); }
            return SendRequestAsync<IList<SidebarCategory>>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<string>> UpdateSidebarCategoryOrderAsync(string userId, string teamId,
            IEnumerable<string> categoryIds, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId) + "/order";
            if (categoryIds is null) { throw new ArgumentNullException(nameof(categoryIds)); }
            cancellationToken.ThrowIfCancellationRequested();
            List<string> body = new List<string>();
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string categoryId in categoryIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                EscapeReadIdentifier(categoryId, nameof(categoryIds));
                string id = categoryId.Trim();
                if (!ids.Add(id)) { throw new ArgumentException("Category identifiers must be unique.", nameof(categoryIds)); }
                body.Add(id);
            }
            if (body.Count == 0) { throw new ArgumentException("At least one category identifier is required.", nameof(categoryIds)); }
            return SendRequestAsync<IList<string>>(HttpMethod.Put, route, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteSidebarCategoryAsync(string userId, string teamId, string categoryId,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = GetSidebarCategoriesRoute(userId, teamId) + "/" + EscapeReadIdentifier(categoryId, nameof(categoryId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, route, cancellationToken: cancellationToken);
        }

        private static string GetSidebarCategoriesRoute(string userId, string teamId) => Routes.Users + "/"
            + EscapeReadIdentifier(userId, nameof(userId)) + "/teams/" + EscapeReadIdentifier(teamId, nameof(teamId))
            + "/channels/categories";

        private static SidebarCategory CopySidebarCategory(SidebarCategory category)
        {
            IList<string>? channelIds = null;
            if (category.ChannelIds is IList<string> channels) { channelIds = new List<string>(channels); }
            return new SidebarCategory
            {
                Id = category.Id, UserId = category.UserId.Trim(), TeamId = category.TeamId.Trim(),
                SortOrder = category.SortOrder, Sorting = category.Sorting, Type = category.Type,
                DisplayName = category.DisplayName, Muted = category.Muted, Collapsed = category.Collapsed,
                ChannelIds = channelIds
            };
        }
    }
}
