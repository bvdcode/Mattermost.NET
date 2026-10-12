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
        public async Task<bool> CanUserDirectMessageAsync(string userId, string otherUserId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string source = EscapeReadIdentifier(userId, nameof(userId));
            string target = EscapeReadIdentifier(otherUserId, nameof(otherUserId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, bool> response = await SendRequestAsync<Dictionary<string, bool>>(HttpMethod.Get,
                Routes.SharedChannels + "/users/" + source + "/can_dm/" + target, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!response.TryGetValue("can_dm", out bool canMessage))
            {
                throw new JsonException("The direct-message eligibility is missing from the response.");
            }
            return canMessage;
        }
    }
}
