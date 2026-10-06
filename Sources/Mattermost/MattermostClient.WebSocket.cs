using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Exceptions;
using Mattermost.Extensions;
using Mattermost.Models.Responses.Websocket;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Start receiving messages asynchronously with cancellation token.
        /// </summary>
        /// <returns>Receiver task.</returns>
        public async Task StartReceivingAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            await CheckAuthorizedAsync().ConfigureAwait(false);
            await StopReceivingAsync().ConfigureAwait(false);

            _linkedReceivingTokenSource?.Dispose();
            _linkedReceivingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_receivingTokenSource.Token, cancellationToken);
            CancellationToken mergedToken = _linkedReceivingTokenSource.Token;

            Log("Starting receiving as user @" + (_cachedUserInfo?.Username ?? "Unknown"));
            _receiverTask = Task.Run(async () =>
            {
                while (!mergedToken.IsCancellationRequested)
                {
                    try
                    {
                        if (_ws.State != WebSocketState.Open)
                        {
                            await ConnectAsync(mergedToken).ConfigureAwait(false);
                        }

                        WebsocketMessage response = await _ws.ReceiveAsync(mergedToken).ConfigureAwait(false);
                        await HandleResponseAsync(response, mergedToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Log("WebSocket receiving canceled");
                        OnDisconnected?.Invoke(this, new DisconnectionEventArgs(WebSocketCloseStatus.NormalClosure, "Closed by client", DateTime.UtcNow));
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log("Error in receiving messages", ex);
                        try
                        {
                            await Task.Delay(1_000, mergedToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }, mergedToken);
        }

        /// <summary>
        /// Stop receiving messages.
        /// </summary>
        public async Task StopReceivingAsync()
        {
            CheckDisposed();

            _receivingTokenSource.Cancel();
            _linkedReceivingTokenSource?.Cancel();

            if (_ws.State == WebSocketState.Open
                || _ws.State == WebSocketState.CloseReceived
                || _ws.State == WebSocketState.CloseSent)
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None).ConfigureAwait(false);
                    OnDisconnected?.Invoke(this, new DisconnectionEventArgs(WebSocketCloseStatus.NormalClosure, "Closed by client", DateTime.UtcNow));
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Log("Error while closing WebSocket", ex);
                    throw;
                }
            }

            if (_receiverTask != null)
            {
                try
                {
                    await _receiverTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (_receiverTask.IsCompleted)
                    {
                        _receiverTask.Dispose();
                    }
                    _receiverTask = null;
                }
            }

            _linkedReceivingTokenSource?.Dispose();
            _linkedReceivingTokenSource = null;

            _receivingTokenSource.Dispose();
            _receivingTokenSource = new CancellationTokenSource();

            _ws.Dispose();
            _ws = new ClientWebSocket();
        }

        private Task HandleResponseAsync(WebsocketMessage response, CancellationToken cancellationToken)
        {
            try
            {
                OnEventReceived?.Invoke(this, new WebSocketEventArgs(this, response, cancellationToken));
            }
            catch (Exception ex)
            {
                Log("Error when calling OnEventReceived", ex);
            }

            switch (response.Event)
            {
                case MattermostEvent.Posted:
                    MessageEventArgs messageArgs = new MessageEventArgs(this, response, cancellationToken, _cachedUserInfo?.Id);
                    if (ShouldDispatchMessage(messageArgs))
                    {
                        OnMessageReceived?.Invoke(this, messageArgs);
                    }
                    break;

                case MattermostEvent.StatusChange:
                    UserStatusChangeEventArgs statusArgs = new UserStatusChangeEventArgs(this, response, cancellationToken);
                    OnStatusUpdated?.Invoke(this, statusArgs);
                    break;

                default:
                    Log($"Received event: {response.Event}");
                    break;
            }

            // Handle the case when the server closes the connection.
            if (response.MessageType == WebSocketMessageType.Close)
            {
                OnDisconnected?.Invoke(this, new DisconnectionEventArgs(response.CloseStatus, response.CloseStatusDescription, DateTime.UtcNow));
            }

            return Task.CompletedTask;
        }

        private bool ShouldDispatchMessage(MessageEventArgs messageArgs)
        {
            if (_cachedUserInfo == null)
            {
                return false;
            }

            return Options.ShouldDispatchMessage(messageArgs);
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();

            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new AuthorizationException("Authorization token is not set - call LoginAsync first");
            }

            Uri uri = _websocketUri;
            if (_ws.State != WebSocketState.None)
            {
                try
                {
                    Log("Closing websocket connection from state " + _ws.State);
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken).ConfigureAwait(false);
                    _ws.Dispose();
                }
                catch (Exception ex)
                {
                    Log("Closing websocket connection with error", ex);
                }
            }

            _ws = new ClientWebSocket();
            try
            {
                Log("Opening new websocket connection...");
                await _ws.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(_accessToken))
                {
                    throw new AuthorizationException("Authorization token is not set - call LoginAsync first");
                }

                WebsocketMessage result = await _ws.RequestAsync(WebsocketMethods.Authentication, new { token = _accessToken }).ConfigureAwait(false);
                if (result.Status != MattermostStatus.Ok)
                {
                    throw new AuthorizationException("Authentication error, server response: " + result.Status);
                }

                Log("WebSocket connection established with state " + _ws.State);
                OnConnected?.Invoke(this, new ConnectionEventArgs(uri, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                Log("WebSocket connection failed", ex);
                OnDisconnected?.Invoke(this, new DisconnectionEventArgs(null, ex.Message, DateTime.UtcNow));
            }
        }

        internal static Uri BuildWebsocketUri(Uri serverUri)
        {
            UriBuilder builder = new UriBuilder(serverUri)
            {
                Scheme = string.Equals(serverUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    ? "wss"
                    : "ws",
                Port = serverUri.IsDefaultPort ? -1 : serverUri.Port,
                Query = string.Empty,
                Fragment = string.Empty
            };
            return BuildRequestUri(builder.Uri, Routes.WebSocket);
        }
    }
}
