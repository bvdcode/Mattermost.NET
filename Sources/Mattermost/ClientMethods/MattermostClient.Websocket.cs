using Mattermost.Constants;
using Mattermost.Extensions;
using Mattermost.Models.Requests.Websocket;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        private readonly SemaphoreSlim _typingSendGate = new SemaphoreSlim(1, 1);
        private volatile ClientWebSocket? _authenticatedWebSocket;

        /// <inheritdoc />
        public async Task SendTypingAsync(string channelId, string parentId = "", CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(channelId))
                throw new ArgumentException("The channel identifier cannot be empty.", nameof(channelId));
            if (parentId == null)
                throw new ArgumentNullException(nameof(parentId));

            await _typingSendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CheckDisposed();
                var socket = _authenticatedWebSocket;
                if (socket == null || socket != _ws || socket.State != WebSocketState.Open)
                    throw new InvalidOperationException("An authenticated WebSocket connection is required. Call StartReceivingAsync and wait for OnConnected.");

                // The receiver owns all reads. A second reader could consume a post instead of an acknowledgement.
                await socket.SendAsync(new ActionRequest
                {
                    Seq = ClientWebSocketExtensions.NextSequence(),
                    Action = WebsocketMethods.UserTyping,
                    Data = new { channel_id = channelId, parent_id = parentId }
                }, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _typingSendGate.Release();
            }
        }
    }
}
