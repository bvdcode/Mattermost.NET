using Mattermost.Constants;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Set call state for channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier where calls must be in specified state. </param>
        /// <param name="isCallsEnabled"> New state. </param>
        public async Task SetChannelCallStateAsync(string channelId, bool isCallsEnabled)
        {
            CheckDisposed();
            string url = Routes.CallsPlugin + "/" + channelId;
            var body = new
            {
                enabled = isCallsEnabled
            };
            await SendRequestAsync(HttpMethod.Post, url, body);
        }

        /// <summary>
        /// Check whether a call is active in the specified channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <returns> True when the channel has an active call; otherwise false. </returns>
        public async Task<bool> GetCallActiveAsync(string channelId)
        {
            CheckDisposed();
            string escapedChannelId = EscapeCallsIdentifier(channelId, nameof(channelId));
            string url = Routes.CallsPlugin + "/calls/" + escapedChannelId + "/active";
            Dictionary<string, bool> response = await SendRequestAsync<Dictionary<string, bool>>(
                HttpMethod.Get,
                url).ConfigureAwait(false);
            return response["active"];
        }

        /// <summary>
        /// End the active call in the specified channel for all participants.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        public async Task EndCallAsync(string channelId)
        {
            CheckDisposed();
            string escapedChannelId = EscapeCallsIdentifier(channelId, nameof(channelId));
            string url = Routes.CallsPlugin + "/calls/" + escapedChannelId + "/host/end";
            await SendRequestAsync(HttpMethod.Post, url).ConfigureAwait(false);
        }

        private static string EscapeCallsIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Identifier cannot be null or empty.", parameterName);
            }

            return Uri.EscapeDataString(value.Trim());
        }
    }
}
