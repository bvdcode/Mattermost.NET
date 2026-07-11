using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Exceptions;
using Mattermost.Extensions;
using Mattermost.Models;
using Mattermost.Models.Responses.Websocket;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// .NET API client for Mattermost servers with websocket polling.
    /// </summary>
    public partial class MattermostClient : IMattermostClient, IDisposable
    {
        /// <summary>
        /// Called when client is connected to server WebSocket after
        /// <see cref="StartReceivingAsync(CancellationToken)"/> method.
        /// </summary>
        public event EventHandler<ConnectionEventArgs>? OnConnected;

        /// <summary>
        /// Called when client is disconnected from server WebSocket after
        /// <see cref="StopReceivingAsync()"/> method or when server closes connection.
        /// </summary>
        public event EventHandler<DisconnectionEventArgs>? OnDisconnected;

        /// <summary>
        /// Event called when new message received.
        /// You have to call <see cref="StartReceivingAsync(CancellationToken)"/> method to start receiving messages.
        /// </summary>
        public event EventHandler<MessageEventArgs>? OnMessageReceived;

        /// <summary>
        /// Event called when log message created.
        /// </summary>
        public event EventHandler<LogEventArgs>? OnLogMessage;

        /// <summary>
        /// Event called when user status updated.
        /// You have to call <see cref="StartReceivingAsync(CancellationToken)"/> method to start receiving status updates.
        /// </summary>
        public event EventHandler<UserStatusChangeEventArgs>? OnStatusUpdated;

        /// <summary>
        /// Event called when any event received.
        /// </summary>
        public event EventHandler<WebSocketEventArgs>? OnEventReceived;

        /// <summary>
        /// Specifies whether the client is connected to the server with WebSocket.
        /// </summary>
        public bool IsConnected => !_disposed && _ws.State == WebSocketState.Open;

        /// <summary>
        /// User information.
        /// </summary>
        public User CurrentUserInfo
        {
            get
            {
                CheckDisposed();
                return _cachedUserInfo?.MemberwiseClone() ??
                    throw new AuthorizationException("You must call any method that requires authorization, " +
                        "such as GetMeAsync if you use API key; if you want to use username and password, " +
                        "please call LoginAsync method first. This property (CurrentUserInfo) just returns user information " +
                        "which is set after successful authorization or GetMeAsync invocation.");
            }
        }

        /// <summary>
        /// Base server address.
        /// </summary>
        public Uri ServerAddress => _serverUri;

        /// <summary>
        /// Client behavior configuration.
        /// </summary>
        public MattermostClientOptions Options { get; } = new MattermostClientOptions();

        /// <summary>
        /// Extension methods needed for this client, hidden from public.
        /// </summary>
        internal HttpClient HttpClient => _http;

        private bool _disposed;
        private ClientWebSocket _ws;
        private Task? _receiverTask;
        private User? _cachedUserInfo;
        private readonly Uri _serverUri;
        private readonly string? _apiKey;
        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;
        private readonly Uri _websocketUri;
        private string? _accessToken;
        private const int DefaultHttpClientTimeoutSeconds = 60;
        private CancellationTokenSource _receivingTokenSource;
        private CancellationTokenSource? _linkedReceivingTokenSource;

        /// <summary>
        /// Create <see cref="MattermostClient"/> with default server address.
        /// </summary>
        public MattermostClient()
            : this(new ClientInitialization(ParseServerUri(Routes.DefaultBaseUrl), null, null))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address.
        /// </summary>
        /// <param name="serverUrl">Server URL with HTTP(S) scheme.</param>
        public MattermostClient(string serverUrl)
            : this(new ClientInitialization(ParseServerUri(serverUrl), null, null))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address.
        /// </summary>
        /// <param name="serverUri">Server URI with HTTP(S) scheme.</param>
        public MattermostClient(Uri serverUri)
            : this(new ClientInitialization(ValidateServerUri(serverUri, nameof(serverUri)), null, null))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and API key.
        /// </summary>
        /// <param name="serverUrl">Server URL with HTTP(S) scheme.</param>
        /// <param name="apiKey">API key, ex. bot token or personal access token.</param>
        public MattermostClient(string serverUrl, string apiKey)
            : this(new ClientInitialization(ParseServerUri(serverUrl), ValidateApiKey(apiKey), null))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and API key.
        /// </summary>
        /// <param name="serverUri">Server URI with HTTP(S) scheme.</param>
        /// <param name="apiKey">API key, ex. bot token or personal access token.</param>
        public MattermostClient(Uri serverUri, string apiKey)
            : this(new ClientInitialization(ValidateServerUri(serverUri, nameof(serverUri)), ValidateApiKey(apiKey), null))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and external HTTP transport.
        /// </summary>
        /// <param name="serverUrl">Server URL with HTTP(S) scheme.</param>
        /// <param name="httpClient">External HTTP client instance.</param>
        public MattermostClient(string serverUrl, HttpClient httpClient)
            : this(new ClientInitialization(ParseServerUri(serverUrl), null, ValidateHttpClient(httpClient)))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address and external HTTP transport.
        /// </summary>
        /// <param name="serverUri">Server URI with HTTP(S) scheme.</param>
        /// <param name="httpClient">External HTTP client instance.</param>
        public MattermostClient(Uri serverUri, HttpClient httpClient)
            : this(new ClientInitialization(ValidateServerUri(serverUri, nameof(serverUri)), null, ValidateHttpClient(httpClient)))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address, API key and external HTTP transport.
        /// </summary>
        /// <param name="serverUrl">Server URL with HTTP(S) scheme.</param>
        /// <param name="apiKey">API key, ex. bot token or personal access token.</param>
        /// <param name="httpClient">External HTTP client instance.</param>
        public MattermostClient(string serverUrl, string apiKey, HttpClient httpClient)
            : this(new ClientInitialization(ParseServerUri(serverUrl), ValidateApiKey(apiKey), ValidateHttpClient(httpClient)))
        {
        }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address, API key and external HTTP transport.
        /// </summary>
        /// <param name="serverUri">Server URI with HTTP(S) scheme.</param>
        /// <param name="apiKey">API key, ex. bot token or personal access token.</param>
        /// <param name="httpClient">External HTTP client instance.</param>
        public MattermostClient(Uri serverUri, string apiKey, HttpClient httpClient)
            : this(new ClientInitialization(ValidateServerUri(serverUri, nameof(serverUri)), ValidateApiKey(apiKey), ValidateHttpClient(httpClient)))
        {
        }

        private MattermostClient(ClientInitialization initialization)
        {
            _receivingTokenSource = new CancellationTokenSource();
            _serverUri = initialization.ServerUri;
            _apiKey = initialization.ApiKey;
            _ws = new ClientWebSocket();
            _websocketUri = GetWebsocketUri(initialization.ServerUri);

            if (initialization.HttpClient == null)
            {
                _http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(DefaultHttpClientTimeoutSeconds)
                };
                _ownsHttpClient = true;
            }
            else
            {
                _http = initialization.HttpClient;
                _ownsHttpClient = false;
            }
        }

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

                        var response = await _ws.ReceiveAsync(mergedToken).ConfigureAwait(false);
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
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new AuthorizationException("You cannot use API key and login with username/password at the same time");
            }

            CheckDisposed();
            var body = new
            {
                login_id = username,
                password
            };

            using HttpResponseMessage result = await SendHttpRequestAsync(
                HttpMethod.Post,
                Routes.Users + "/login",
                payload: body,
                requiresAuthorization: false).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new AuthorizationException("Login error, server response: " + result.StatusCode);
            }

            if (!result.Headers.TryGetValues("Token", out IEnumerable<string>? tokenValues))
            {
                throw new AuthorizationException("Token not found in response headers");
            }

            string? token = null;
            foreach (string value in tokenValues)
            {
                token = value;
                break;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AuthorizationException("Token not found in response headers");
            }

            _accessToken = token;
            _cachedUserInfo = await result.GetResponseAsync<User>().ConfigureAwait(false);
            return _cachedUserInfo.MemberwiseClone();
        }

        /// <summary>
        /// Logout from server.
        /// </summary>
        /// <returns>Task representing logout operation.</returns>
        /// <exception cref="MattermostClientException">Throws if server response is not successful.</exception>
        public async Task LogoutAsync()
        {
            CheckDisposed();
            await CheckAuthorizedAsync().ConfigureAwait(false);

            using HttpResponseMessage response = await SendHttpRequestAsync(
                HttpMethod.Post,
                Routes.Users + "/logout").ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new MattermostClientException("Logout error, server response: " + response.StatusCode);
            }

            await StopReceivingAsync().ConfigureAwait(false);
            _accessToken = null;
            _cachedUserInfo = null;
        }

        /// <summary>
        /// Dispose client resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _receivingTokenSource.Cancel();
                _linkedReceivingTokenSource?.Cancel();
            }
            catch
            {
            }

            try
            {
                if (_ws.State == WebSocketState.Open
                    || _ws.State == WebSocketState.CloseReceived
                    || _ws.State == WebSocketState.CloseSent)
                {
                    _ws.Abort();
                }
            }
            catch
            {
            }

            _ws.Dispose();
            _receivingTokenSource.Dispose();
            _linkedReceivingTokenSource?.Dispose();

            if (_receiverTask != null && _receiverTask.IsCompleted)
            {
                _receiverTask.Dispose();
            }

            if (_ownsHttpClient)
            {
                _http.Dispose();
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
                    var messageArgs = new MessageEventArgs(this, response, cancellationToken, _cachedUserInfo?.Id);
                    if (ShouldDispatchMessage(messageArgs))
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

                    await Task.Delay(100).ConfigureAwait(false);
                    if (token.IsCancellationRequested || result >= 100)
                    {
                        break;
                    }
                }
            }, token);
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();

            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new AuthorizationException("Authorization token is not set - call LoginAsync first");
            }

            Uri uri = BuildRequestUri(_websocketUri, Routes.WebSocket);
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

                var result = await _ws.RequestAsync(WebsocketMethods.Authentication, new { token = _accessToken }).ConfigureAwait(false);
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

        private static Uri GetWebsocketUri(Uri serverUri)
        {
            UriBuilder builder = new UriBuilder(serverUri)
            {
                Scheme = string.Equals(serverUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                    ? "wss"
                    : "ws",
                Port = serverUri.IsDefaultPort ? -1 : serverUri.Port,
                Path = "/",
                Query = string.Empty,
                Fragment = string.Empty
            };
            return builder.Uri;
        }

        private static Uri ParseServerUri(string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("Server URL cannot be null or empty.", nameof(serverUrl));
            }

            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out Uri? serverUri))
            {
                throw new ArgumentException("Server URL must be a valid absolute URI.", nameof(serverUrl));
            }

            return ValidateServerUri(serverUri, nameof(serverUrl));
        }

        private static Uri ValidateServerUri(Uri serverUri, string parameterName)
        {
            if (serverUri == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (!serverUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Server URI must be absolute.", parameterName);
            }

            bool isValidScheme = string.Equals(serverUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(serverUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
            if (!isValidScheme)
            {
                throw new ArgumentException("Scheme must be 'http' or 'https'.", parameterName);
            }

            return serverUri;
        }

        private static string ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ApiKeyException("API key is empty");
            }

            return apiKey;
        }

        private static HttpClient ValidateHttpClient(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            return httpClient;
        }

        private sealed class ClientInitialization
        {
            public ClientInitialization(Uri serverUri, string? apiKey, HttpClient? httpClient)
            {
                ServerUri = serverUri;
                ApiKey = apiKey;
                HttpClient = httpClient;
            }

            public Uri ServerUri { get; }

            public string? ApiKey { get; }

            public HttpClient? HttpClient { get; }
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message));
        }

        private void Log(string message, Exception ex)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message + $" (Exception: {ex.Message})"));
        }

        private Task CheckAuthorizedAsync()
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                string? apiKey = _apiKey;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    return LoginWithApiKeyAsync(apiKey!);
                }

                throw new AuthorizationException("Authorization token is not set - call LoginAsync first or use constructor with API key (Personal Access Token)");
            }

            return Task.CompletedTask;
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

            using HttpResponseMessage result = await SendHttpRequestAsync(
                HttpMethod.Get,
                Routes.Users + "/me",
                requiresAuthorization: false,
                authorizationTokenOverride: apiKey).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new ApiKeyException("Login with API key error, server response: " + result.StatusCode);
            }

            _cachedUserInfo = await result.GetResponseAsync<User>().ConfigureAwait(false);
            _accessToken = apiKey;
            return _cachedUserInfo;
        }

        private Task SendRequestAsync(HttpMethod method, string requestUri, object? payload = null, CancellationToken cancellationToken = default) =>
            SendRequestAsync<object>(method, requestUri, payload, cancellationToken);

        private async Task<TResult> SendRequestAsync<TResult>(
            HttpMethod method,
            string requestUri,
            object? payload = null,
            CancellationToken cancellationToken = default,
            Func<TResult>? nullResultFactory = null)
        {
            using HttpResponseMessage response = await SendHttpRequestAsync(
                method,
                requestUri,
                payload,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                MattermostClientException exception;
                try
                {
                    var details = JsonSerializer.Deserialize<MattermostApiErrorDetails>(json)
                        ?? throw new JsonException("Failed to deserialize error result: " + json);
                    exception = new MattermostClientException(details.Message);
                }
                catch (Exception)
                {
                    exception = new MattermostClientException("Unknown error, server response: " + response.StatusCode);
                }

                exception.StatusCode = response.StatusCode;
                exception.ResponseJson = json;
                exception.RequestUri = BuildRequestUri(requestUri).ToString();
                exception.RequestMethod = method.Method;
                throw exception;
            }

            object? result = JsonSerializer.Deserialize<TResult>(json);
            if (result != null)
            {
                return (TResult)result;
            }

            if (nullResultFactory != null)
            {
                return nullResultFactory();
            }

            throw new JsonException("Failed to deserialize result: " + json);
        }

        private async Task<HttpResponseMessage> SendHttpRequestAsync(
            HttpMethod method,
            string route,
            object? payload = null,
            HttpContent? content = null,
            bool requiresAuthorization = true,
            string? authorizationTokenOverride = null,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (requiresAuthorization)
            {
                await CheckAuthorizedAsync().ConfigureAwait(false);
            }

            using HttpRequestMessage request = new HttpRequestMessage(method, BuildRequestUri(route));
            bool hasExternalContent = content != null;
            if (hasExternalContent)
            {
                request.Content = content;
            }
            else if (payload != null)
            {
                string jsonPayload = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            string? accessToken = authorizationTokenOverride;
            if (requiresAuthorization && string.IsNullOrWhiteSpace(accessToken))
            {
                accessToken = _accessToken;
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            try
            {
                return await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // External content ownership belongs to the caller.
                if (hasExternalContent)
                {
                    request.Content = null;
                }
            }
        }

        private Uri BuildRequestUri(string route)
        {
            return BuildRequestUri(_serverUri, route);
        }

        private static Uri BuildRequestUri(Uri baseUri, string route)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException("Route cannot be null or empty.", nameof(route));
            }

            bool hasExplicitScheme = route.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal) >= 0;
            if (hasExplicitScheme && Uri.TryCreate(route, UriKind.Absolute, out Uri? absoluteUri))
            {
                return absoluteUri;
            }

            return new Uri(baseUri, route);
        }
    }
}
