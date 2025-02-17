using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Models;
using Mattermost.Constants;
using Mattermost.Extensions;
using Mattermost.Exceptions;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Mattermost.Models.Posts;
using Mattermost.Models.Teams;
using Mattermost.Models.Users;
using System.Collections.Generic;
using Mattermost.Models.Channels;
using Mattermost.Models.Responses.Websocket;
using System.Collections.Specialized;
using System.Web;

namespace Mattermost
{
    /// <summary>
    /// .NET API client for Mattermost servers with websocket polling.
    /// </summary>
    public class MattermostClient : IMattermostClient, IDisposable
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
        private readonly HttpClient _http;
        private readonly Uri _websocketUri;
        private CancellationTokenSource _receivingTokenSource;

        /// <summary>
        /// Create <see cref="MattermostClient"/> with default server address.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient() : this(Routes.DefaultBaseUrl) { }

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
            CheckAuthorized();
            CheckDisposed();
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
        /// <param name="loginId">Username or email.</param>
        /// <param name="password">Password.</param>
        /// <returns>Authorized <see cref="User"/> object.</returns>
        /// <exception cref="AuthorizationException">Throws if credentials are invalid or server response is not successful.</exception>
        public async Task<User> LoginAsync(string loginId, string password)
        {
            CheckDisposed();
            var body = new
            {
                login_id = loginId,
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
        /// Login with specified API key - personal access token, see <see href="https://developers.mattermost.com/integrate/reference/personal-access-token/"/>
        /// </summary>
        /// <param name="apiKey"> API key, ex. bot token or personal access token. </param>
        /// <returns>Autorized <see cref="User"/> object.</returns>
        /// <exception cref="AuthorizationException">Throws if API key is invalid or server response is not successful.</exception>
        public async Task<User> LoginAsync(string apiKey)
        {
            CheckDisposed();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var result = await _http.GetAsync(Routes.Users + "/me");
            if (!result.IsSuccessStatusCode)
            {
                throw new AuthorizationException("Login error, server response: " + result.StatusCode);
            }
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
            CheckAuthorized();
            CheckDisposed();
            var response = await _http.PostAsync(Routes.Users + "/logout", null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new MattermostClientException("Logout error, server response: " + response.StatusCode);
            }
            await StopReceivingAsync();
            _http.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Get current authorized user information and force update of <see cref="CurrentUserInfo"/>.
        /// </summary>
        /// <returns> Authorized user information. </returns>
        public async Task<User> GetMeAsync()
        {
            CheckAuthorized();
            CheckDisposed();
            var response = await _http.GetAsync(Routes.Users + "/me");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            _userInfo = JsonSerializer.Deserialize<User>(json)!;
            return _userInfo;
        }


        /// <summary>
        /// Send message to specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <returns> Created post. </returns>
        [Obsolete("Use CreatePostAsync instead.")]
        public Task<Post> SendMessageAsync(string channelId, string message = "",
            string replyToPostId = "", MessagePriority priority = MessagePriority.Empty,
            IEnumerable<string>? files = null) => CreatePostAsync(channelId, message, replyToPostId, priority, files);

        /// <summary>
        /// Send message to specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="message"> Message text (Markdown supported). </param>
        /// <param name="replyToPostId"> Reply to post (optional) </param>
        /// <param name="priority"> Set message priority </param>
        /// <param name="files"> Attach files to post. </param>
        /// <param name="props"> A general JSON property bag to attach to the post. </param>
        /// <returns> Created post. </returns>
        public async Task<Post> CreatePostAsync(string channelId, string message = "",
            string replyToPostId = "", MessagePriority priority = MessagePriority.Empty,
            IEnumerable<string>? files = null, IDictionary<string, object>? props = null)
        {
            CheckAuthorized();
            CheckDisposed();
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            if (priority != MessagePriority.Empty)
            {
                metadata.Add("priority", new
                {
                    priority = priority.ToString().ToLower(),
                    requested_ack = false
                });
            }

            var body = new
            {
                message,
                channel_id = channelId,
                root_id = replyToPostId,
                metadata,
                file_ids = files,
                props
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Routes.Posts, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Post>(json)!;
        }

        /// <summary>
        /// Update message text for specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <param name="newText"> New message text (Markdown supported). </param>
        /// <param name="props"> A general JSON property bag to attach to the post. </param>
        /// <returns> Updated post. </returns>
        public async Task<Post> UpdatePostAsync(string postId, string newText, IDictionary<string, object>? props = null)
        {
            CheckAuthorized();
            CheckDisposed();
            var body = new
            {
                message = newText,
                props
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(Routes.Posts + "/" + postId + "/patch", content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Post>(json)!;
        }

        /// <summary>
        /// Delete post with specified post identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        public async Task<bool> DeletePostAsync(string postId)
        {
            CheckAuthorized();
            CheckDisposed();
            var response = await _http.DeleteAsync(Routes.Posts + "/" + postId);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of posts per page. </param>
        /// <param name="beforePostId"> A post id to select the posts that came before this one. </param>
        /// <param name="afterPostId"> A post id to select the posts that came after this one. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        public async Task<ChannelPosts> GetPostsForChannelAsync(string channelId, int page = 0, 
            int perPage = 60, string? beforePostId = null, 
            string? afterPostId = null, bool includeDeleted = false
            ) {
            CheckAuthorized();
            CheckDisposed();

            var query = new NameValueCollection {
                { nameof(page), page.ToString() },
                { "per_page", perPage.ToString() },
                { "include_deleted", includeDeleted.ToString() }
            };
            if (beforePostId != null) {
                query.Add("before", beforePostId);
            }
            if (afterPostId != null) {
                query.Add("after", afterPostId);
            }
            var url = Routes.Channels + "/" + channelId + "/posts?" + HttpUtility.ParseQueryString(query.ToString());
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChannelPosts>(json)!;
        }

        /// <summary>
        /// Get a page of posts in a channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="since"> Time to select modified posts after. </param>
        /// <param name="includeDeleted"> Whether to include deleted posts or not. Must have system admin permissions. </param>
        /// <returns> ChannelPosts object with posts. </returns>
        public async Task<ChannelPosts> GetPostsForChannelAsync(string channelId, DateTime since, bool includeDeleted = false) 
        {
            CheckAuthorized();
            CheckDisposed();

            var query = new NameValueCollection {
                { "include_deleted", includeDeleted.ToString() },
                { nameof(since), ((DateTimeOffset)since).ToUnixTimeSeconds().ToString() }
            };
            var url = Routes.Channels + "/" + channelId + "/posts?" + HttpUtility.ParseQueryString(query.ToString());
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChannelPosts>(json)!;
        }

        /// <summary>
        /// Create group channel with specified users.
        /// </summary>
        /// <param name="userIds"> Participant users. </param>
        /// <returns> Created channel info. </returns>
        public async Task<Channel> CreateGroupChannelAsync(params string[] userIds)
        {
            CheckAuthorized();
            CheckDisposed();
            string json = JsonSerializer.Serialize(userIds);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Routes.GroupChannels, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Channel>(json)!;

            //TODO: create channel link
            var team = await GetTeamAsync(result.TeamId);
            result.Link = ServerAddress + team.Name + "/channels/" + result.Id;
            //

            return result;
        }

        /// <summary>
        /// Create simple channel with specified users.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <param name="name"> Channel name. </param>
        /// <param name="displayName"> Channel display name. </param>
        /// <param name="channelType"> Channel type: open or private. </param>
        /// <param name="purpose"> Channel purpose (optional). </param>
        /// <param name="header"> Channel header (optional). </param>
        /// <returns> Created channel info. </returns>
        public async Task<Channel> CreateChannelAsync(string teamId, string name, string displayName,
            ChannelType channelType, string purpose = "", string header = "")
        {
            CheckAuthorized();
            CheckDisposed();
            const int maxChannelDisplayNameLength = 64;
            if (displayName.Length > maxChannelDisplayNameLength)
            {
                throw new ArgumentException("Display name is too long", nameof(displayName));
            }
            var body = new
            {
                team_id = teamId,
                name,
                display_name = displayName,
                purpose,
                header,
                type = channelType.ToChannelChar()
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Routes.Channels, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Channel>(json)!;
            var team = await GetTeamAsync(teamId);
            result.Link = ServerAddress + team.Name + "/channels/" + result.Id;
            return result;
        }

        private async Task<Team> GetTeamAsync(string teamId)
        {
            string url = Routes.Teams + "/" + teamId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Team>(json)!;
        }

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        public async Task<User> GetUserAsync(string userId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Users + "/" + userId;
            string json = await _http.GetStringAsync(url);
            User userInfo = JsonSerializer.Deserialize<User>(json)!;
            return userInfo;
        }

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Users + "/username/" + username.Replace("@", string.Empty).Trim();
            string json = await _http.GetStringAsync(url);
            User userInfo = JsonSerializer.Deserialize<User>(json)!;
            return userInfo;
        }

        /// <summary>
        /// Add user to channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Channel user information. </returns>
        public async Task<ChannelUserInfo> AddUserToChannelAsync(string channelId, string userId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Channels + $"/{channelId}/members";
            var body = new
            {
                user_id = userId
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            response = response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChannelUserInfo>(json)!;
        }

        /// <summary>
        /// Delete user from channel.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        /// <param name="userId"> User identifier. </param>
        /// <returns> True if deleted, otherwise false. </returns>
        public async Task<bool> DeleteUserFromChannelAsync(string channelId, string userId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Channels + $"/{channelId}/members/{userId}";
            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Upload new file.
        /// </summary>
        /// <param name="channelId"> Channel where file will be posted. </param>
        /// <param name="filePath"> File fullname on local device. </param>
        /// <param name="progressChanged"> Uploading progress callback in percents - from 0 to 100. </param>
        /// <returns> Created file details. </returns>
        public async Task<FileDetails> UploadFileAsync(string channelId, string filePath, Action<int>? progressChanged = null)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Files + "?channel_id=" + channelId;
            MultipartFormDataContent content = new MultipartFormDataContent();
            FileInfo fileInfo = new FileInfo(filePath);
            using var fs = fileInfo.OpenRead();
            StreamContent file = new StreamContent(fs);
            content.Add(file, "files", fileInfo.Name);
            CancellationTokenSource cts = new CancellationTokenSource();
            if (progressChanged != null)
            {
                StartProgressTracker(fs, fileInfo, cts.Token, progressChanged);
            }
            var result = await _http.PostAsync(url, content);
            result = result.EnsureSuccessStatusCode();
            cts.Cancel();
            string json = await result.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FileResponse>(json)!.Files.First();
        }

        /// <summary>
        /// Get file details by specified identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File details. </returns>
        public async Task<FileDetails> GetFileDetailsAsync(string fileId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Files + "/" + fileId + "/info";
            string json = await _http.GetStringAsync(url);
            return JsonSerializer.Deserialize<FileDetails>(json)!;
        }

        /// <summary>
        /// Find channel by channel name and team identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier where channel is exists. </param>
        /// <param name="channelName"> Channel name. </param>
        /// <returns> Channel info. </returns>
        public async Task<Channel?> FindChannelByNameAsync(string teamId, string channelName)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Teams + $"/{teamId}/channels/name/{channelName}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Channel>(json);
        }

        /// <summary>
        /// Set call state for channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier where calls must be in specified state. </param>
        /// <param name="isCallsEnabled"> New state. </param>
        public async Task<bool> SetChannelCallStateAsync(string channelId, bool isCallsEnabled)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Plugins + "/com.mattermost.calls/" + channelId;
            var body = new
            {
                enabled = isCallsEnabled
            };
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Archive channel by specified channel identifier.
        /// </summary>
        /// <param name="channelId"> Channel identifier. </param>
        public async Task<bool> ArchiveChannelAsync(string channelId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Channels + "/" + channelId;
            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get post by identifier.
        /// </summary>
        /// <param name="postId"> Post identifier. </param>
        /// <returns> Post information. </returns>
        public async Task<Post> GetPostAsync(string postId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Posts + "/" + postId;
            string json = await _http.GetStringAsync(url);
            return JsonSerializer.Deserialize<Post>(json)!;
        }

        /// <summary>
        /// Get file by identifier.
        /// </summary>
        /// <param name="fileId"> File identifier. </param>
        /// <returns> File bytes. </returns>
        public async Task<byte[]> GetFileAsync(string fileId)
        {
            CheckAuthorized();
            CheckDisposed();
            string url = Routes.Files + "/" + fileId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
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

        #region Private methods

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

        private void StartProgressTracker(FileStream fs, FileInfo fileInfo, CancellationToken token, Action<int> progressChanged)
        {
            _ = Task.Run(async () =>
            {
                int progress = 0;

                while (true)
                {
                    long current = fs.Position;
                    long total = fileInfo.Length;
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
                throw new ApiKeyException("Authorization token is not set - call LoginAsync first");
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
                    throw new ApiKeyException("Authorization token is not set - call LoginAsync first");
                }
                string token = _http.DefaultRequestHeaders.Authorization.Parameter.Replace("Bearer ", string.Empty);
                var result = await _ws.RequestAsync(WebsocketMethods.Authentication, new { token });
                if (result.Status != MattermostStatus.Ok)
                {
                    throw new ApiKeyException("Authentication error, server response: " + result.Status);
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
                throw new ApiKeyException("Authorization token is not set - call LoginAsync first");
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        #endregion
    }
}