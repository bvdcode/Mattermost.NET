using Mattermost.Constants;
using Mattermost.Models.Plugins;
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
        public Task<PluginsResponse> GetPluginsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<PluginsResponse>(HttpMethod.Get, Routes.CorePlugins, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<PluginManifest>> GetWebappPluginsAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<PluginManifest>>(HttpMethod.Get, Routes.CorePlugins + "/webapp", cancellationToken: cancellationToken,
                nullResultFactory: () => new List<PluginManifest>());
        }

        /// <inheritdoc />
        public Task<IList<PluginStatus>> GetPluginStatusesAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<PluginStatus>>(HttpMethod.Get, Routes.CorePlugins + "/statuses", cancellationToken: cancellationToken,
                nullResultFactory: () => new List<PluginStatus>());
        }

        /// <inheritdoc />
        public Task<IList<MarketplacePlugin>> GetMarketplacePluginsAsync(string? filter = null, bool localOnly = false,
            bool remoteOnly = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (localOnly && remoteOnly) { throw new ArgumentException("Local-only and remote-only filters cannot both be true."); }
            string route = Routes.CorePlugins + "/marketplace?local_only=" + (localOnly ? "true" : "false")
                + "&remote_only=" + (remoteOnly ? "true" : "false");
            if (filter is string text) { route += "&filter=" + Uri.EscapeDataString(text.Trim()); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<MarketplacePlugin>>(HttpMethod.Get, route, cancellationToken: cancellationToken,
                nullResultFactory: () => new List<MarketplacePlugin>());
        }

        /// <inheritdoc />
        public async Task<bool> GetMarketplaceVisitedByAdminAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Get, Routes.CorePlugins + "/marketplace/first_admin_visit",
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("value", out JsonElement value)
                || value.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("The Marketplace visit status must contain a native string value.");
            }
            switch (value.GetString())
            {
                case "true": return true;
                case "false": return false;
                default: throw new JsonException("The Marketplace visit status must be true or false.");
            }
        }
    }
}
