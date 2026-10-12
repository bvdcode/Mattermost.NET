using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Notices;
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
        public Task MarkProductNoticesViewedAsync(IEnumerable<string> noticeIds, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            List<string> ids = PrepareStringBatch(noticeIds, nameof(noticeIds));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Put, Routes.System + "/notices/view", ids, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<ProductNotice>> GetProductNoticesAsync(string teamId, ProductNoticeClientType client,
            string clientVersion, string? locale = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string route = Routes.System + "/notices/" + EscapeReadIdentifier(teamId, nameof(teamId));
            if (clientVersion is null || string.IsNullOrWhiteSpace(clientVersion))
            {
                throw new ArgumentException("Client version cannot be blank.", nameof(clientVersion));
            }
            string platform;
            switch (client)
            {
                case ProductNoticeClientType.Web: platform = "web"; break;
                case ProductNoticeClientType.Desktop: platform = "desktop"; break;
                case ProductNoticeClientType.MobileIos: platform = "mobile-ios"; break;
                case ProductNoticeClientType.MobileAndroid: platform = "mobile-android"; break;
                default: throw new ArgumentOutOfRangeException(nameof(client));
            }
            cancellationToken.ThrowIfCancellationRequested();
            route += "?client=" + platform + "&clientVersion=" + Uri.EscapeDataString(clientVersion.Trim());
            if (locale is object && !string.IsNullOrWhiteSpace(locale))
            {
                route += "&locale=" + Uri.EscapeDataString(locale.Trim());
            }
            return SendRequestAsync<IList<ProductNotice>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }
    }
}
