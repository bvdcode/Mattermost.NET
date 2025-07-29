using System.Text;
using System.Net.Http;
using System.Text.Json;
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
        public async Task<bool> SetChannelCallStateAsync(string channelId, bool isCallsEnabled)
        {
            CheckDisposed();
            CheckAuthorized();
            string url = Routes.Plugins + "/com.mattermost.calls/" + channelId;
            var body = new
            {
                enabled = isCallsEnabled
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
    }
}
