using Mattermost.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task TestSiteUrlAsync(string siteUrl, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(siteUrl) || !Uri.TryCreate(siteUrl.Trim(), UriKind.Absolute, out Uri target)
                || (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Site URL must be an absolute HTTP or HTTPS URL.", nameof(siteUrl));
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.SiteUrlTest, new { site_url = siteUrl.Trim() }, cancellationToken);
        }

        /// <inheritdoc />
        public Task TestFileStoreConnectionAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Post, Routes.FileStoreTest, cancellationToken: cancellationToken);
        }
    }
}
