using System.Text.Json.Serialization;

namespace Mattermost.Models.Requests.Websocket
{
    internal class ActionRequest
    {
        [JsonPropertyName("seq")]
        public int Seq { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;

        [JsonPropertyName("data")]
        public object Data { get; set; } = null!;
    }
}
