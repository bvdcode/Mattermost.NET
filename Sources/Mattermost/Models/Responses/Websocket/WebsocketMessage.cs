using System;
using System.Text.Json;
using Mattermost.Enums;
using System.Text.Json.Serialization;
using System.Net.WebSockets;

namespace Mattermost.Models.Responses
{
    internal class WebsocketMessage
    {
        public WebSocketMessageType MessageType { get; set; }

        public WebSocketCloseStatus? CloseStatus { get; private set; }
        public string CloseStatusDescription { get; private set; }

        public void UpdateCloseStatusInfo(WebSocketCloseStatus status, string description)
        {
            CloseStatus = status;
            CloseStatusDescription = description;
        }

        [JsonPropertyName("status")]
        public string StatusText { get; set; } = null!;

        [JsonPropertyName("event")]
        public string? EventText { get; set; }

        [JsonPropertyName("seq_reply")]
        public int Seq { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public MattermostEvent Event => GetEvent();

        public MattermostStatus Status => GetStatus();

        public string Raw { get; set; } = string.Empty;

        public string Json
        {
            get
            {
                if (Data == null)
                {
                    return string.Empty;
                }
                string json = JsonSerializer.Serialize(Data);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return string.Empty;
                }
                return json;
            }
        }

        internal TObj GetData<TObj>()
        {
            return JsonSerializer.Deserialize<TObj>(Json)!;
        }

        private MattermostEvent GetEvent()
        {
            bool parsed = Enum.TryParse<MattermostEvent>(EventText, true, out var result);
            return parsed ? result : MattermostEvent.Unknown;
        }

        private MattermostStatus GetStatus()
        {
            bool parsed = Enum.TryParse<MattermostStatus>(StatusText, true, out var result);
            return parsed ? result : MattermostStatus.Unknown;
        }
    }
}