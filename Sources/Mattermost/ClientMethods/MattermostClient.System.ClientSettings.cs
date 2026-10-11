using Mattermost.Constants;
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
        public Task<IDictionary<string, string>> GetClientConfigAsync(bool authenticated = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IDictionary<string, string>>(HttpMethod.Get, Routes.ClientConfig,
                cancellationToken: cancellationToken, requiresAuthorization: authenticated);
        }

        /// <inheritdoc />
        public Task<IDictionary<string, string>> GetClientLicenseAsync(bool authenticated = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IDictionary<string, string>>(HttpMethod.Get, Routes.License + "/client",
                cancellationToken: cancellationToken, requiresAuthorization: authenticated);
        }

        /// <inheritdoc />
        public async Task<long> GetLicenseLoadMetricAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.License + "/load_metric",
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("load", out JsonElement load)
                || load.ValueKind != JsonValueKind.Number || !load.TryGetInt64(out long metric))
            {
                throw new JsonException("License load metric is missing or invalid.");
            }
            return metric;
        }
    }
}
