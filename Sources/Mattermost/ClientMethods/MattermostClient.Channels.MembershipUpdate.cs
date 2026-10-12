using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Responses;
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
        public async Task<IList<ChannelMembersUpdateResult>> SetChannelMembersAsync(string channelId, IEnumerable<string> members,
            IEnumerable<string>? channelAdmins = null, int batchSize = MattermostApiLimits.DefaultChannelMemberBatchSize,
            int batchDelayMilliseconds = MattermostApiLimits.DefaultChannelMemberBatchDelayMilliseconds,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string channel = EscapeReadIdentifier(channelId, nameof(channelId));
            if (batchSize < 1 || batchSize > MattermostApiLimits.MaxChannelMemberBatchSize)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }
            if (batchDelayMilliseconds < 0 || batchDelayMilliseconds > MattermostApiLimits.MaxChannelMemberBatchDelayMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(batchDelayMilliseconds));
            }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["members"] = CopyChannelMembershipIdentifiers(members, nameof(members), cancellationToken)
            };
            if (channelAdmins is object)
            {
                body.Add("channel_admins", CopyChannelMembershipIdentifiers(channelAdmins, nameof(channelAdmins), cancellationToken));
            }
            string route = Routes.Channels + "/" + channel + "/members?batch_size=" + batchSize.ToString(CultureInfo.InvariantCulture)
                + "&batch_delay_ms=" + batchDelayMilliseconds.ToString(CultureInfo.InvariantCulture);
            using HttpResponseMessage response = await SendHttpRequestAsync(HttpMethod.Put, route, body,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) { throw CreateRequestException(response, content, HttpMethod.Put, route); }
            List<ChannelMembersUpdateResult> results = new List<ChannelMembersUpdateResult>();
            foreach (string line in content.Split('\n'))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object) { throw new JsonException("A membership batch must be a JSON object."); }
                if (root.TryGetProperty("error", out JsonElement terminalError))
                {
                    if (terminalError.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(terminalError.GetString()))
                    {
                        throw new JsonException("The terminal membership error must contain a message.");
                    }
                    throw new MattermostClientException(terminalError.GetString()!)
                    {
                        StatusCode = response.StatusCode, ResponseJson = content,
                        RequestMethod = HttpMethod.Put.Method, RequestUri = BuildRequestUri(route).ToString()
                    };
                }
                if (!root.TryGetProperty("added", out JsonElement added) || added.ValueKind != JsonValueKind.Array
                    || !root.TryGetProperty("removed", out JsonElement removed) || removed.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("A membership batch must contain added and removed arrays.");
                }
                ChannelMembersUpdateResult result = JsonSerializer.Deserialize<ChannelMembersUpdateResult>(line)!;
                if (result.Promoted is null || result.Demoted is null || result.Errors is null)
                {
                    throw new JsonException("Membership result collections cannot be null.");
                }
                results.Add(result);
            }
            return results;
        }

        private static List<string> CopyChannelMembershipIdentifiers(IEnumerable<string> identifiers, string parameterName,
            CancellationToken cancellationToken)
        {
            if (identifiers is null) { throw new ArgumentNullException(parameterName); }
            List<string> result = new List<string>();
            foreach (string identifier in identifiers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(identifier)) { throw new ArgumentException("User identifiers cannot be blank.", parameterName); }
                result.Add(identifier.Trim());
            }
            return result;
        }
    }
}
