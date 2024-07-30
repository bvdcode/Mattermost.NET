﻿using System;
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
using Mattermost.Models.Responses;

namespace Mattermost
{
    /// <summary>
    /// .NET API client for Mattermost servers with websocket polling.
    /// </summary>
    public class MattermostClient : IMattermostClient
    {
        /// <summary>
        /// Event called when new message received.
        /// </summary>
        public event EventHandler<MessageEventArgs>? OnMessageReceived;

        /// <summary>
        /// Event callen when log message created.
        /// </summary>
        public event EventHandler<LogEventArgs>? OnLogMessage;

        /// <summary>
        /// Bot user information.
        /// </summary>
        public User CurrentUserInfo => _botUserInfo;

        /// <summary>
        /// Base server address.
        /// </summary>
        public Uri ServerAddress => _serverUri;

        private User _botUserInfo;
        private readonly Uri _serverUri;
        private readonly string _apiToken;
        private readonly HttpClient _http;
        private readonly Uri _websocketUri;
        private ClientWebSocket _ws;

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified JWT access token.
        /// </summary>
        /// <param name="apiToken"> JWT API token. </param>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient(string apiToken) : this(Routes.DefaultBaseUrl, apiToken) { }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address JWT access token.
        /// </summary>
        /// <param name="serverUrl"> Server URL with HTTP(S) scheme. </param>
        /// <param name="apiToken"> JWT API token. </param>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient(string serverUrl, string apiToken) : this(new Uri(serverUrl), apiToken) { }

        /// <summary>
        /// Create <see cref="MattermostClient"/> with specified server address JWT access token.
        /// </summary>
        /// <param name="serverUri"> Server URI with HTTP(S) scheme. </param>
        /// <param name="apiToken"> JWT API token. </param>
        /// <exception cref="ArgumentException"></exception>
        public MattermostClient(Uri serverUri, string apiToken)
        {
            CheckUrl(serverUri);
            _ws = new ClientWebSocket();
            _websocketUri = GetWebsocketUri(serverUri);
            _botUserInfo = new User();
            _serverUri = serverUri;
            _apiToken = apiToken;
            _http = new HttpClient() { BaseAddress = _serverUri, Timeout = TimeSpan.FromMinutes(60) };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
        }

        /// <summary>
        /// Create receiver <see cref="Task"/> with websocket polling.
        /// </summary>
        /// <returns> Receiver task. </returns>
        /// <exception cref="ApiKeyException"></exception>
        public Task StartReceivingAsync() => StartReceivingAsync(CancellationToken.None);

        /// <summary>
        /// Create receiver <see cref="Task"/> with websocket polling.
        /// </summary>
        /// <returns> Receiver task. </returns>
        /// <exception cref="ApiKeyException"></exception>
        public async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            _botUserInfo = await GetBotUserInfoAsync();
            OnLogMessage?.Invoke(this, new LogEventArgs("Receiving started as user " + _botUserInfo.Username));
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_ws.State != WebSocketState.Open)
                    {
                        await ConnectAsync(cancellationToken);
                    }
                    var response = await _ws.ReceiveAsync(cancellationToken);
                    await HandleResponseAsync(response, cancellationToken);
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke(this, new LogEventArgs(ex.Message));
                    await Task.Delay(60_000);
                }
            }
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
        public async Task<Post> SendMessageAsync(string channelId, string message = "",
            string replyToPostId = "", MessagePriority priority = MessagePriority.Empty,
            IEnumerable<string>? files = null)
        {
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
                file_ids = files
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
        /// <returns> Updated post. </returns>
        public async Task<Post> UpdatePostAsync(string postId, string newText)
        {
            var body = new
            {
                message = newText
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
            var response = await _http.DeleteAsync(Routes.Posts + "/" + postId);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Create group channel with specified users.
        /// </summary>
        /// <param name="userIds"> Participant users. </param>
        /// <returns> Created channel info. </returns>
        public async Task<Channel> CreateGroupChannelAsync(params string[] userIds)
        {
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
        public async Task<Channel?> FindChannelByName(string teamId, string channelName)
        {
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
            string url = Routes.Files + "/" + fileId;
            var response = await _http.GetAsync(url);
            response = response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        #region Private methods

        private Task HandleResponseAsync(WebsocketMessage response, CancellationToken cancellationToken)
        {
            switch (response.Event)
            {
                case MattermostEvent.Posted:
                    {
                        var args = new MessageEventArgs(this, response, cancellationToken);
                        if (args.Message.Post.UserId != _botUserInfo.Id)
                        {
                            OnMessageReceived?.Invoke(this, args);
                        }
                        break;
                    }
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
                    if (token.IsCancellationRequested || result == 100)
                    {
                        break;
                    }
                }
            }, token);
        }

        private async Task<User> GetBotUserInfoAsync()
        {
            return await GetUserAsync("me");
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var uri = new Uri(_websocketUri + Routes.WebSocket);
            if (_ws.State != WebSocketState.None)
            {
                try
                {
                    OnLogMessage?.Invoke(this, new LogEventArgs("Closing websocket connection from state " + _ws.State));
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken);
                    _ws.Dispose();
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke(this, new LogEventArgs("Closing websocket connection with error: " + ex.Message));
                }
            }
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(uri, cancellationToken);
            OnLogMessage?.Invoke(this, new LogEventArgs("Opened new websocket connection with state " + _ws.State));
            var result = await _ws.RequestAsync(WebsocketMethods.Authentication, new { token = _apiToken });
            if (result.Status != MattermostStatus.Ok)
            {
                throw new ApiKeyException("Authentication error, server response: " + result.Status);
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

        #endregion
    }
}