using Mattermost.Constants;
using Mattermost.Models.Responses;
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
        public Task<IDictionary<string, JsonElement>> GetServerConfigAsync(bool removeMasked = false, bool removeDefaults = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            string route = Routes.Config + "?remove_masked=" + removeMasked.ToString().ToLowerInvariant()
                + "&remove_defaults=" + removeDefaults.ToString().ToLowerInvariant();
            return SendRequestAsync<IDictionary<string, JsonElement>>(HttpMethod.Get, route, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IDictionary<string, JsonElement>> GetEnvironmentConfigAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IDictionary<string, JsonElement>>(HttpMethod.Get, Routes.Config + "/environment",
                cancellationToken: cancellationToken, nullResultFactory: () => new Dictionary<string, JsonElement>());
        }

        /// <inheritdoc />
        public Task<IList<AppliedSchemaMigration>> GetAppliedSchemaMigrationsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<AppliedSchemaMigration>>(HttpMethod.Get, Routes.System + "/schema/version",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> GetFirstAdminOnboardingCompleteAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.System + "/onboarding/complete",
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("value", out JsonElement value)
                || value.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("The onboarding status must contain a native string value.");
            }
            switch (value.GetString())
            {
                case "true": return true;
                case "false": return false;
                default: throw new JsonException("The onboarding status must be true or false.");
            }
        }

        /// <inheritdoc />
        public Task<ServerRelease> GetLatestServerReleaseAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<ServerRelease>(HttpMethod.Get, Routes.LatestVersion, cancellationToken: cancellationToken);
        }
    }
}
