using Mattermost.Constants;
using Mattermost.Events;
using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System;
using System.Net.Http;
using System.Net.WebSockets;
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
            _websocketUri = BuildWebsocketUri(initialization.ServerUri);

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

        private void Log(string message)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message));
        }

        private void Log(string message, Exception ex)
        {
            OnLogMessage?.Invoke(this, new LogEventArgs(message + $" (Exception: {ex.Message})"));
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
