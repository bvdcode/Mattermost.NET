using Mattermost.Constants;
using Mattermost.Models.Responses;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<ServerLimits> GetServerLimitsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ServerLimits>(HttpMethod.Get, Routes.ServerLimits, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<long> GetPostsUsageAsync(CancellationToken cancellationToken = default)
        {
            return ReadUsageCounterAsync(Routes.Usage + "/posts", "count", cancellationToken);
        }

        /// <inheritdoc />
        public Task<long> GetStorageUsageAsync(CancellationToken cancellationToken = default)
        {
            return ReadUsageCounterAsync(Routes.Usage + "/storage", "bytes", cancellationToken);
        }

        /// <inheritdoc />
        public Task<TeamsUsage> GetTeamsUsageAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<TeamsUsage>(HttpMethod.Get, Routes.Usage + "/teams", cancellationToken: cancellationToken);
        }

        private async Task<long> ReadUsageCounterAsync(string route, string fieldName, CancellationToken cancellationToken)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, route, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty(fieldName, out JsonElement counter)
                || counter.ValueKind != JsonValueKind.Number || !counter.TryGetInt64(out long value))
            {
                throw new JsonException("The usage response must contain a valid 64-bit counter.");
            }
            return value;
        }
    }
}
