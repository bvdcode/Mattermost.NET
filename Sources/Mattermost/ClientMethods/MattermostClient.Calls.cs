using System.Net.Http;
using Mattermost.Constants;
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
            string url = Routes.Plugins + "/com.mattermost.calls/" + channelId;
            var body = new
            {
                enabled = isCallsEnabled
            };
            await SendRequestAsync(HttpMethod.Post, url, body);
        }
    }
}
