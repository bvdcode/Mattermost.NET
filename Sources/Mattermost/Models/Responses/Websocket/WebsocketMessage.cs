using System;
using System.Text.Json;
using Mattermost.Enums;
using System.Net.WebSockets;
using System.Text.Json.Serialization;

namespace Mattermost.Models.Responses.Websocket
{
    internal class WebsocketMessage
    {
        public WebSocketMessageType MessageType { get; set; }
        public WebSocketCloseStatus? CloseStatus { get; private set; }
        public string? CloseStatusDescription { get; private set; }

        public void UpdateCloseStatusInfo(WebSocketCloseStatus? status, string? description)
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
            return JsonSerializer.Deserialize<TObj>(Json) ?? default!;
        }

        private MattermostEvent GetEvent()
        {
            string eventText = (EventText ?? string.Empty)
                .Replace("_", string.Empty);
            bool parsed = Enum.TryParse<MattermostEvent>(eventText, true, out var result);
            return parsed ? result : MattermostEvent.Unknown;
        }

        private MattermostStatus GetStatus()
        {
            bool parsed = Enum.TryParse<MattermostStatus>(StatusText, true, out var result);
            return parsed ? result : MattermostStatus.Unknown;
        }
    }
}