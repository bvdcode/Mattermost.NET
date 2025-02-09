using System;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Mattermost.Models.Requests.Websocket;
using Mattermost.Models.Responses.Websocket;

namespace Mattermost.Extensions
{
    internal static class ClientWebSocketExtensions
    {
        private static int seq = 1;

        internal static Task SendAsync<TObj>(this ClientWebSocket webSocket, TObj obj)
        {
            string json = JsonSerializer.Serialize(obj);
            byte[] data = Encoding.UTF8.GetBytes(json);
            return webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal static async Task<WebsocketMessage> ReceiveAsync(this ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[ushort.MaxValue * 1024];
            var response = await webSocket.ReceiveAsync(buffer, cancellationToken);
            Array.Resize(ref buffer, response.Count);
            var result = JsonSerializer.Deserialize<WebsocketMessage>(buffer)!;
            result.Raw = Encoding.UTF8.GetString(buffer);
            result.MessageType = response.MessageType;
            if (result.MessageType == WebSocketMessageType.Close)
            {
                result.UpdateCloseStatusInfo(result.CloseStatus, result.CloseStatusDescription);
            }
            return result;
        }

        internal static async Task<WebsocketMessage> RequestAsync(this ClientWebSocket webSocket, string action, object data)
        {
            const int tryCount = 100;
            var body = new ActionRequest()
            {
                Seq = seq++,
                Action = action,
                Data = data
            };
            await webSocket.SendAsync(body);
            for (int i = 0; i < tryCount; i++)
            {
                var result = await webSocket.ReceiveAsync(CancellationToken.None);
                if (result.Seq == body.Seq)
                {
                    return result;
                }
            }
            throw new HttpRequestException($"Request was sent but no response received with seq {body.Seq}.");
        }
    }
}