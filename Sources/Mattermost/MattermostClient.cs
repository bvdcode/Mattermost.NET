using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Constants;
using Mattermost.Extensions;
using Mattermost.Exceptions;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Mattermost.Models.Users;
using Mattermost.Models.Responses.Websocket;

namespace Mattermost
{
    /// <summary>
    /// .NET API client for Mattermost servers with websocket polling.
    /// </summary>
    public partial class MattermostClient : IMattermostClient, IDisposable
    {
        /// <summary>
        /// Called when client is connected to server WebSocket after
        /// <see cref="StartReceivingAsync()"/> method.
        /// </summary>
        public event EventHandler<ConnectionEventArgs>? OnConnected;

        /// <summary>
        /// Called when client is disconnected from server WebSocket after 
        /// <see cref="StopReceivingAsync()"/> method or when server closes connection.
        /// </summary>
        public event EventHandler<DisconnectionEventArgs>? OnDisconnected;

        /// <summary>
        /// Event called when new message received.
        /// You have to call <see cref="StartReceivingAsync()"/> method to start receiving messages.
        /// </summary>
        public event EventHandler<MessageEventArgs>? OnMessageReceived;

        /// <summary>
        /// Event called when log message created.
        /// </summary>
        public event EventHandler<LogEventArgs>? OnLogMessage;

        /// <summary>
        /// Event called when user status updated.
        /// You have to call <see cref="StartReceivingAsync()"/> method to start receiving status updates.
        /// </summary>
        public event EventHandler<UserStatusChangeEventArgs>? OnStatusUpdated;

        /// <summary>
        /// Event called when any event received.
        /// </summary>
        public event EventHandler<WebSocketEventArgs>? OnEventReceived;

        /// <summary>
        /// Specifies whether the client is connected to the server with WebSocket.
        /// </summary>
        public bool IsConnected => _ws.State == WebSocketState.Open;

        /// <summary>
        /// User information.
        /// </summary>
        public User CurrentUserInfo => _userInfo ?? throw new InvalidOperationException("You must login first");

        /// <summary>
        /// Base server address.
        /// </summary>
        public Uri ServerAddress => _serverUri;

        /// <summary>
        /// Extension methods needed for this client, hidden from public.
        /// </summary>
        internal HttpClient HttpClient => _http;

        private bool _disposed;
        private User? _userInfo;
        private ClientWebSocket _ws;
        private Task? _receiverTask;
        private readonly Uri _serverUri;
        private readonly string? _apiKey;
        private readonly HttpClient _http;
        private readonly Uri _websocketUri;
        private CancellationTokenSource _receivingTokenSource;

        /// <summary>
        /// Create <see cref="MattermostClient"/> with default server address.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient() : this(Routes.DefaultBaseUrl) { }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and API key.
        /// </summary>
        /// <param name="serverUrl"> Server URL with HTTP(S) scheme. </param>
        /// <param name="apiKey"> API key, ex. bot token or personal access token. </param>
        public MattermostClient(string serverUrl, string apiKey) : this(new Uri(serverUrl), apiKey) { }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and API key.
        /// </summary>
        /// <param name="serverUri"> Server URI with HTTP(S) scheme. </param>
        /// <param name="apiKey"> API key, ex. bot token or personal access token. </param>
        public MattermostClient(Uri serverUri, string apiKey) : this(serverUri)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApiKeyException("API key is empty");
            }
            _apiKey = apiKey;
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address JWT access token.
        /// </summary>
        /// <param name="serverUrl"> Server URL with HTTP(S) scheme. </param>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient(string serverUrl) : this(new Uri(serverUrl)) { }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address JWT access token.
        /// </summary>
        /// <param name="serverUri"> Server URI with HTTP(S) scheme. </param>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient(Uri serverUri)
        {
            _receivingTokenSource = new CancellationTokenSource();
            CheckUrl(serverUri);
            _ws = new ClientWebSocket();
            _websocketUri = GetWebsocketUri(serverUri);
            _userInfo = new User();
            _serverUri = serverUri;
            _http = new HttpClient() { BaseAddress = _serverUri, Timeout = TimeSpan.FromMinutes(60) };
        }

        /// <summary>
        /// Start receiving messages asynchronously.
        /// </summary>
        /// <returns> Receiver task. </returns>
        /// <exception cref="ApiKeyException"></exception>
        public Task StartReceivingAsync() => StartReceivingAsync(CancellationToken.None);

        /// <summary>
        /// Start receiving messages asynchronously with cancellation token.
        /// </summary>
        /// <returns> Receiver task. </returns>
        /// <exception cref="ApiKeyException"></exception>
        public async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckAuthorized();
            await StopReceivingAsync();
            _ws = new ClientWebSocket();
            _receivingTokenSource = new CancellationTokenSource();
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(_receivingTokenSource.Token, cancellationToken).Token;

            Log("Starting receiving as user @" + _userInfo?.Username ?? "Unknown");
            _receiverTask = Task.Run(async () =>
            {
                while (!mergedToken.IsCancellationRequested)
                {
                    try
                    {
                        if (_ws.State != WebSocketState.Open)
                        {
                            await ConnectAsync(mergedToken);
                        }
                        var response = await _ws.ReceiveAsync(mergedToken);
                        await HandleResponseAsync(response, mergedToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Log("WebSocket receiving canceled");
                        // Trigger OnDisconnected event
                        OnDisconnected?.Invoke(this, new DisconnectionEventArgs(WebSocketCloseStatus.NormalClosure, "Closed by client", DateTime.UtcNow));
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log("Error in receiving messages", ex);
                        await Task.Delay(1_000);
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
            _receivingTokenSource?.Cancel();

            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);

                    // Trigger OnDisconnected event
                    OnDisconnected?.Invoke(this, new DisconnectionEventArgs(WebSocketCloseStatus.NormalClosure, "Closed by client", DateTime.UtcNow));
                    _ws.Dispose();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as appropriate
                    Log("Error while closing WebSocket", ex);
                    throw;
                }
            }

            if (_receiverTask != null)
            {
                await _receiverTask;  // Wait for the receiving task to complete
                _receiverTask.Dispose();
            }
        }

        /// <summary>
        /// Login with specified login identifier and password.
        /// </summary>
        /// <param name="username">Username or email.</param>
        /// <param name="password">Password.</param>
        /// <returns>Authorized <see cref="User"/> object.</returns>
        /// <exception cref="AuthorizationException">Throws if credentials are invalid or server response is not successful.</exception>
        /// <exception cref="ArgumentException">Throws if username or password is empty.</exception>
        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username or password is empty");
            }
            CheckDisposed();
            var body = new
            {
                login_id = username,
                password
            };
            const string url = Routes.Users + "/login";
            var result = await _http.PostAsJsonAsync(url, body);
            if (!result.IsSuccessStatusCode)
            {
                throw new AuthorizationException("Login error, server response: " + result.StatusCode);
            }
            string token = result.Headers.GetValues("Token").FirstOrDefault()
                ?? throw new AuthorizationException("Token not found in response headers");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _userInfo = result.GetResponse<User>();
            return _userInfo;
        }

        /// <summary>
        /// Logout from server.
        /// </summary>
        /// <returns> Task representing logout operation. </returns>
        /// <exception cref="MattermostClientException">Throws if server response is not successful.</exception>
        public async Task LogoutAsync()
        {
            CheckDisposed();
            CheckAuthorized();
            var response = await _http.PostAsync(Routes.Users + "/logout", null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new MattermostClientException("Logout error, server response: " + response.StatusCode);
            }
            await StopReceivingAsync();
            _http.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Dispose client resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _ws.Dispose();
                _http.Dispose();
                _disposed = true;
                _receivingTokenSource.Dispose();
                if (_receiverTask != null && _receiverTask.IsCompleted)
                {
                    _receiverTask.Dispose();
                }
            }
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
                    var messageArgs = new MessageEventArgs(this, response, cancellationToken);
                    if (_userInfo != null && messageArgs.Message.Post.UserId != _userInfo.Id)
                    {
                        OnMessageReceived?.Invoke(this, messageArgs);
                    }
                    break;

                case MattermostEvent.StatusChange:
                    var statusArgs = new UserStatusChangeEventArgs(this, response, cancellationToken);
                    OnStatusUpdated?.Invoke(this, statusArgs);
                    break;

                default:
                    Log($"Received event: {response.Event}");
                    break;
            }

            // Handle the case when the server closes the connection
            if (response.MessageType == WebSocketMessageType.Close)
            {
                OnDisconnected?.Invoke(this, new DisconnectionEventArgs(response.CloseStatus, response.CloseStatusDescription, DateTime.UtcNow));
            }

            return Task.CompletedTask;
        }

        private void StartProgressTracker(Stream fs, CancellationToken token, Action<int> progressChanged)
        {
            _ = Task.Run(async () =>
            {
                int progress = 0;

                while (!token.IsCancellationRequested)
                {
                    long current = fs.Position;
                    long total = fs.Length;
                    int result = (int)((double)current * 100 / total);
                    if (result != progress)
                    {
                        progress = result;
                        progressChanged?.Invoke(result);
                    }
                    await Task.Delay(100);
                    if (token.IsCancellationRequested || result >= 100)
                    {
                        break;
                    }
                }
            }, token);
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_http.DefaultRequestHeaders.Authorization == null)
            {
                throw new AuthorizationException("Authorization token is not set - call LoginAsync first");
            }
            var uri = new Uri(_websocketUri + Routes.WebSocket);
            if (_ws.State != WebSocketState.None)
            {
                try
                {
                    Log("Closing websocket connection from state " + _ws.State);
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken);
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
                await _ws.ConnectAsync(uri, cancellationToken);
                if (_http.DefaultRequestHeaders.Authorization == null
                    || _http.DefaultRequestHeaders.Authorization.Scheme != "Bearer"
                    || string.IsNullOrWhiteSpace(_http.DefaultRequestHeaders.Authorization.Parameter))
                {
                    throw new AuthorizationException("Authorization token is not set - call LoginAsync first");
                }
                string token = _http.DefaultRequestHeaders.Authorization.Parameter.Replace("Bearer ", string.Empty);
                var result = await _ws.RequestAsync(WebsocketMethods.Authentication, new { token });
                if (result.Status != MattermostStatus.Ok)
                {
                    throw new AuthorizationException("Authentication error, server response: " + result.Status);
                }
                Log("WebSocket connection established with state " + _ws.State);

                // Trigger OnConnected event
                OnConnected?.Invoke(this, new ConnectionEventArgs(uri, DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                // Handle connection error
                Log($"WebSocket connection failed", ex);
                OnDisconnected?.Invoke(this, new DisconnectionEventArgs(null, ex.Message, DateTime.UtcNow));
            }
        }

        private Uri GetWebsocketUri(Uri serverUri)
        {
            string serverUrl = serverUri.ToString();
            string websockerUrl = serverUrl
                .Replace("https://", "wss://")
                .Replace("http://", "ws://");
            return new Uri(websockerUrl);
        }

        private void CheckUrl(Uri serverUri)
        {
            string url = serverUri.ToString();
            if (!url.Contains("http"))
            {
                throw new ArgumentException("Scheme must be 'http' or 'https'");
            }
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message));
        }

        private void Log(string message, Exception ex)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message + $" (Exception: {ex.Message}"));
        }

        private void CheckAuthorized()
        {
            if (_http.DefaultRequestHeaders.Authorization == null
                || _http.DefaultRequestHeaders.Authorization.Scheme != "Bearer"
                || string.IsNullOrWhiteSpace(_http.DefaultRequestHeaders.Authorization.Parameter))
            {
                if (!string.IsNullOrWhiteSpace(_apiKey))
                {
                    LoginWithApiKeyAsync(_apiKey).Wait();
                }
                else
                {
                    throw new AuthorizationException("Authorization token is not set - call LoginAsync first or use constructor with API key");
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private async Task<User> LoginWithApiKeyAsync(string apiKey)
        {
            CheckDisposed();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApiKeyException("API key is empty");
            }
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var result = await _http.GetAsync(Routes.Users + "/me");
            if (!result.IsSuccessStatusCode)
            {
                throw new ApiKeyException("Login with API key error, server response: " + result.StatusCode);
            }
            _userInfo = result.GetResponse<User>();
            return _userInfo;
        }
    }
}