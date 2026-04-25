using Mattermost.Models.Requests.Websocket;
using Mattermost.Models.Responses.Websocket;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost.Extensions
{
    internal static class ClientWebSocketExtensions
    {
        private static int seq = 1;

        internal static Task SendAsync<TObj>(this ClientWebSocket webSocket, TObj obj)
        {
            string json = JsonSerializer.Serialize(obj);
            byte[] data = Encoding.UTF8.GetBytes(json);

            return webSocket.SendAsync(
                new ArraySegment<byte>(data),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        internal static async Task<WebsocketMessage> ReceiveAsync(this ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[ushort.MaxValue * 1024];

            WebSocketReceiveResult response = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken).ConfigureAwait(false);

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

            await webSocket.SendAsync(body).ConfigureAwait(false);

            for (int i = 0; i < tryCount; i++)
            {
                var result = await webSocket.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
                if (result.Seq == body.Seq)
                {
                    return result;
                }
            }

            throw new HttpRequestException($"Request was sent but no response received with seq {body.Seq}.");
        }
    }
}