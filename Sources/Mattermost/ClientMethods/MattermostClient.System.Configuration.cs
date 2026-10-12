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
        public Task<IDictionary<string, JsonElement>> PatchServerConfigAsync(IDictionary<string, JsonElement> configuration,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            Dictionary<string, JsonElement> body = PrepareServerConfiguration(configuration, cancellationToken);
            return SendRequestAsync<IDictionary<string, JsonElement>>(HttpMethod.Put, Routes.Config + "/patch", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IDictionary<string, JsonElement>> UpdateServerConfigAsync(IDictionary<string, JsonElement> configuration,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            Dictionary<string, JsonElement> body = PrepareServerConfiguration(configuration, cancellationToken);
            return SendRequestAsync<IDictionary<string, JsonElement>>(HttpMethod.Put, Routes.Config, body, cancellationToken);
        }

        private static Dictionary<string, JsonElement> PrepareServerConfiguration(IDictionary<string, JsonElement> configuration,
            CancellationToken cancellationToken)
        {
            if (configuration is null) { throw new ArgumentNullException(nameof(configuration)); }
            if (configuration.Count == 0) { throw new ArgumentException("At least one configuration section is required.", nameof(configuration)); }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, JsonElement> body = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, JsonElement> section in configuration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(section.Key) || section.Value.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException("Configuration sections must have nonblank names and JSON object values.", nameof(configuration));
                }
                body.Add(section.Key, section.Value.Clone());
            }
            return body;
        }
    }
}
