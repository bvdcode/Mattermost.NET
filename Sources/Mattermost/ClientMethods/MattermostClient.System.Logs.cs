using Mattermost.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public async Task<string> AddClientLogAsync(string message, bool error = false, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (message is null) { throw new ArgumentNullException(nameof(message)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { level = error ? "ERROR" : "DEBUG", message };
            JsonElement response = await SendRequestAsync<JsonElement>(HttpMethod.Post, Routes.ServerLogs, body, cancellationToken).ConfigureAwait(false);
            if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("message", out JsonElement text)
                || text.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("The client log response must contain a message string.");
            }
            return text.GetString()!;
        }

        /// <inheritdoc />
        public Task<IDictionary<string, IList<JsonElement>>> QueryServerLogsAsync(int page = 0,
            int logsPerPage = MattermostApiLimits.MaxServerLogsPerPage, IEnumerable<string>? serverNames = null,
            IEnumerable<string>? logLevels = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (page < 0) { throw new ArgumentOutOfRangeException(nameof(page)); }
            if (logsPerPage <= 0 || logsPerPage > MattermostApiLimits.MaxServerLogsPerPage) { throw new ArgumentOutOfRangeException(nameof(logsPerPage)); }
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                server_names = PrepareLogFilters(serverNames, nameof(serverNames), cancellationToken),
                log_levels = PrepareLogFilters(logLevels, nameof(logLevels), cancellationToken),
                date_from = from?.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture) ?? string.Empty,
                date_to = to?.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture) ?? string.Empty
            };
            string route = Routes.ServerLogs + "/query?page=" + page.ToString(CultureInfo.InvariantCulture)
                + "&logs_per_page=" + logsPerPage.ToString(CultureInfo.InvariantCulture);
            return SendRequestAsync<IDictionary<string, IList<JsonElement>>>(HttpMethod.Post, route, body, cancellationToken);
        }

        private static List<string> PrepareLogFilters(IEnumerable<string>? values, string parameterName, CancellationToken cancellationToken)
        {
            List<string> result = new List<string>();
            if (values is null) { return result; }
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("Log filters cannot contain blank values.", parameterName); }
                string trimmed = value.Trim();
                if (seen.Add(trimmed)) { result.Add(trimmed); }
            }
            return result;
        }
    }
}
