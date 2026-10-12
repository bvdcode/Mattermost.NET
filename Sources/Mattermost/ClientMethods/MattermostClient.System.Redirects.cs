using Mattermost.Constants;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public async Task<string> GetRedirectLocationAsync(string url, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri target)
                || (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("URL must be an absolute HTTP or HTTPS URL.", nameof(url));
            }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> response = await SendRequestAsync<Dictionary<string, string>>(HttpMethod.Get,
                Routes.RedirectLocation + "?url=" + Uri.EscapeDataString(url.Trim()), cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.TryGetValue("location", out string? location) || location is null)
            {
                throw new JsonException("The redirect location is missing from the response.");
            }
            return location;
        }
    }
}
