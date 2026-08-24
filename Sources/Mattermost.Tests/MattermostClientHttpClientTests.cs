using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models;
using Mattermost.Models.Channels;
using Mattermost.Models.Dialogs;
using Mattermost.Models.Posts;
using Mattermost.Models.Teams;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class MattermostClientHttpClientTests
    {
        [Test]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new MattermostClient("https://mattermost.example", (HttpClient)null!));
            Assert.Throws<ArgumentNullException>(() => _ = new MattermostClient(new Uri("https://mattermost.example"), (HttpClient)null!));
            Assert.Throws<ArgumentNullException>(() => _ = new MattermostClient("https://mattermost.example", "api-key", (HttpClient)null!));
            Assert.Throws<ArgumentNullException>(() => _ = new MattermostClient(new Uri("https://mattermost.example"), "api-key", (HttpClient)null!));
        }

        [Test]
        public void Constructor_InvalidServerUri_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _ = new MattermostClient("not-a-valid-uri"));
            Assert.Throws<ArgumentException>(() => _ = new MattermostClient(new Uri("/relative", UriKind.Relative)));
            Assert.Throws<ArgumentException>(() => _ = new MattermostClient(new Uri("ftp://mattermost.example")));
        }

        [Test]
        public async Task CreatePost_PriorityOptions_AreSerialized()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(CreatePostResponse);

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient(
                "https://mattermost.example",
                "api-key",
                externalHttpClient);

            await client.CreatePostAsync(
                "channel-1",
                "Urgent message",
                priority: MessagePriority.Urgent,
                requestedAck: true,
                persistentNotifications: true);
            await client.CreatePostWithRawPropsAsync(
                "channel-1",
                "Important message",
                priority: MessagePriority.Important,
                requestedAck: true);

            IList<RecordedRequest> requests = handler.Requests
                .Where(item => item.Method == HttpMethod.Post && item.RequestUri?.AbsolutePath == "/api/v4/posts")
                .ToList();

            using JsonDocument urgentDocument = JsonDocument.Parse(requests[0].ContentBody ?? "{}");
            JsonElement urgentPriority = urgentDocument.RootElement.GetProperty("metadata").GetProperty("priority");
            Assert.That(urgentPriority.GetProperty("priority").GetString(), Is.EqualTo("urgent"));
            Assert.That(urgentPriority.GetProperty("requested_ack").GetBoolean(), Is.True);
            Assert.That(urgentPriority.GetProperty("persistent_notifications").GetBoolean(), Is.True);

            using JsonDocument importantDocument = JsonDocument.Parse(requests[1].ContentBody ?? "{}");
            JsonElement importantPriority = importantDocument.RootElement.GetProperty("metadata").GetProperty("priority");
            Assert.That(importantPriority.GetProperty("priority").GetString(), Is.EqualTo("important"));
            Assert.That(importantPriority.GetProperty("requested_ack").GetBoolean(), Is.True);
            Assert.That(importantPriority.TryGetProperty("persistent_notifications", out _), Is.False);
        }

        [TestCase(MessagePriority.Empty, true, false)]
        [TestCase(MessagePriority.Important, false, true)]
        public void CreatePost_IncompatiblePriorityOptions_ThrowsArgumentException(
            MessagePriority priority,
            bool requestedAck,
            bool persistentNotifications)
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");

            Assert.Throws<ArgumentException>(() =>
                _ = client.CreatePostAsync(
                    "channel-1",
                    "Invalid priority message",
                    priority: priority,
                    requestedAck: requestedAck,
                    persistentNotifications: persistentNotifications));
        }

        [Test]
        public async Task ExternalHttpClient_GetMe_UsesAbsoluteUriAndDoesNotMutateHttpClientSettings()
        {
            Uri serverUri = new Uri("https://mattermost.example");
            Uri originalBaseAddress = new Uri("https://ignored.example/base/");
            TimeSpan originalTimeout = TimeSpan.FromSeconds(7);
            var originalAuthorization = new AuthenticationHeaderValue("Basic", "ZXh0ZXJuYWw=");

            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsolutePath == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("user-id"));
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler)
            {
                BaseAddress = originalBaseAddress,
                Timeout = originalTimeout
            };
            externalHttpClient.DefaultRequestHeaders.Authorization = originalAuthorization;

            using MattermostClient client = new MattermostClient(serverUri, "api-key", externalHttpClient);
            User user = await client.GetMeAsync();

            Assert.That(user.Id, Is.EqualTo("user-id"));
            Assert.That(externalHttpClient.BaseAddress, Is.EqualTo(originalBaseAddress));
            Assert.That(externalHttpClient.Timeout, Is.EqualTo(originalTimeout));
            Assert.That(externalHttpClient.DefaultRequestHeaders.Authorization?.Scheme, Is.EqualTo(originalAuthorization.Scheme));
            Assert.That(externalHttpClient.DefaultRequestHeaders.Authorization?.Parameter, Is.EqualTo(originalAuthorization.Parameter));

            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests.All(static request => request.RequestUri == new Uri("https://mattermost.example/api/v4/users/me")), Is.True);
            Assert.That(handler.Requests.All(static request => request.Authorization?.Scheme == "Bearer" && request.Authorization.Parameter == "api-key"), Is.True);
        }

        [TestCase("https://mattermost.example/mattermost", "https://mattermost.example/mattermost")]
        [TestCase("https://mattermost.example/corp/mattermost/", "https://mattermost.example/corp/mattermost")]
        public async Task ServerUrlWithSubpath_AuthorizedRequestsPreserveSubpath(string serverUrl, string expectedBaseUrl)
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path is not null && path.EndsWith("/api/v4/users/me", StringComparison.Ordinal))
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("subpath-user"));
                }

                if (path is not null && path.EndsWith("/api/v4/users", StringComparison.Ordinal))
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateUserJson("listed-user") + "]");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient(serverUrl, "api-key", externalHttpClient);

            IList<User> users = await client.GetUsersAsync(1, 2);

            Assert.That(users[0].Id, Is.EqualTo("listed-user"));
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests[0].RequestUri, Is.EqualTo(new Uri(expectedBaseUrl + "/api/v4/users/me")));
            Assert.That(handler.Requests[1].RequestUri, Is.EqualTo(new Uri(expectedBaseUrl + "/api/v4/users?page=1&per_page=2")));
            Assert.That(handler.Requests.All(static request => request.Authorization?.Scheme == "Bearer" && request.Authorization.Parameter == "api-key"), Is.True);
        }

        [TestCase("https://mattermost.example", "wss://mattermost.example/api/v4/websocket")]
        [TestCase("https://mattermost.example/mattermost", "wss://mattermost.example/mattermost/api/v4/websocket")]
        [TestCase("http://mattermost.example:8065/corp/mattermost/", "ws://mattermost.example:8065/corp/mattermost/api/v4/websocket")]
        public void ServerUrlWithSubpath_WebsocketUriPreservesSubpath(string serverUrl, string expectedWebsocketUrl)
        {
            Uri websocketUri = MattermostClient.BuildWebsocketUri(new Uri(serverUrl));

            Assert.That(websocketUri, Is.EqualTo(new Uri(expectedWebsocketUrl)));
        }

        [Test]
        public async Task LoginAndLogout_UsePerRequestAuthorization_AndDoNotMutateDefaultAuthorizationHeader()
        {
            var originalAuthorization = new AuthenticationHeaderValue("Basic", "c2hhcmVkLWNsaWVudA==");

            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/login")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("login-user"), tokenHeader: "login-token");
                }

                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("login-user"));
                }

                if (path == "/api/v4/users/logout")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "{}");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            externalHttpClient.DefaultRequestHeaders.Authorization = originalAuthorization;

            using MattermostClient client = new MattermostClient("https://mattermost.example", externalHttpClient);

            User loggedInUser = await client.LoginAsync("username", "password");
            User currentUser = await client.GetMeAsync();
            await client.LogoutAsync();

            Assert.That(loggedInUser.Id, Is.EqualTo("login-user"));
            Assert.That(currentUser.Id, Is.EqualTo("login-user"));
            Assert.That(externalHttpClient.DefaultRequestHeaders.Authorization?.Scheme, Is.EqualTo(originalAuthorization.Scheme));
            Assert.That(externalHttpClient.DefaultRequestHeaders.Authorization?.Parameter, Is.EqualTo(originalAuthorization.Parameter));

            RecordedRequest loginRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/users/login");
            RecordedRequest meRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/users/me");
            RecordedRequest logoutRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/users/logout");

            Assert.That(loginRequest.Authorization?.Scheme, Is.Not.EqualTo("Bearer"));
            Assert.That(loginRequest.Authorization?.Parameter, Is.Not.EqualTo("login-token"));
            Assert.That(meRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(meRequest.Authorization?.Parameter, Is.EqualTo("login-token"));
            Assert.That(logoutRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(logoutRequest.Authorization?.Parameter, Is.EqualTo("login-token"));
        }

        [Test]
        public async Task GetFileStreamAsync_DisposingReturnedStream_DisposesHttpResponse()
        {
            TrackableHttpResponseMessage? trackedResponse = null;
            byte[] expected = new byte[] { 7, 8, 9 };

            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("stream-user"));
                }

                if (path == "/api/v4/files/stream-file")
                {
                    trackedResponse = new TrackableHttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(expected)
                    };
                    return trackedResponse;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            Stream stream = await client.GetFileStreamAsync("stream-file");
            Assert.That(trackedResponse, Is.Not.Null);
            Assert.That(trackedResponse!.IsDisposed, Is.False);

            using (stream)
            {
                byte[] buffer = new byte[expected.Length];
                int read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                Assert.That(read, Is.EqualTo(expected.Length));
                Assert.That(buffer, Is.EqualTo(expected));
            }

            Assert.That(trackedResponse.IsDisposed, Is.True);
        }

        [Test]
        public async Task ApiKeyConstructor_PrimesAuthorizationViaUsersMe_AndCachesCurrentUser()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                return path switch
                {
                    "/api/v4/users/me" => CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("api-key-user")),
                    "/api/v4/users/target-user" => CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("target-user")),
                    _ => new HttpResponseMessage(HttpStatusCode.NotFound)
                };
            });

            using HttpClient externalHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://should-not-be-used.example/")
            };

            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);
            User user = await client.GetUserAsync("target-user");

            Assert.That(user.Id, Is.EqualTo("target-user"));
            Assert.That(client.CurrentUserInfo.Id, Is.EqualTo("api-key-user"));

            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests[0].RequestUri, Is.EqualTo(new Uri("https://mattermost.example/api/v4/users/me")));
            Assert.That(handler.Requests[1].RequestUri, Is.EqualTo(new Uri("https://mattermost.example/api/v4/users/target-user")));
            Assert.That(handler.Requests.All(static request => request.Authorization?.Scheme == "Bearer" && request.Authorization.Parameter == "api-key"), Is.True);
        }

        [Test]
        public async Task RelativeRoutes_AreResolvedAgainstServerUri_NotAgainstHttpClientBaseAddress()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                return path switch
                {
                    "/api/v4/users/me" => CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("base-check")),
                    "/api/v4/users/relative-user" => CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("relative-user")),
                    _ => new HttpResponseMessage(HttpStatusCode.NotFound)
                };
            });

            using HttpClient externalHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://external-base.example/sub-path/")
            };

            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);
            User user = await client.GetUserAsync("relative-user");

            Assert.That(user.Id, Is.EqualTo("relative-user"));
            Assert.That(handler.Requests[1].RequestUri, Is.EqualTo(new Uri("https://mattermost.example/api/v4/users/relative-user")));
        }

        [Test]
        public async Task LeadingSlashRoute_DoesNotBecomeFileScheme_OnApiKeyAuthorization()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                return path switch
                {
                    "/api/v4/users/me" => CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("slash-route-user")),
                    _ => new HttpResponseMessage(HttpStatusCode.NotFound)
                };
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            User user = await client.GetMeAsync();

            Assert.That(user.Id, Is.EqualTo("slash-route-user"));
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests.All(static request => request.RequestUri?.Scheme == Uri.UriSchemeHttps), Is.True);
            Assert.That(handler.Requests.All(static request => request.RequestUri == new Uri("https://mattermost.example/api/v4/users/me")), Is.True);
        }

        [Test]
        public async Task GetFileAsync_UsesAbsoluteUri_AndPerRequestAuthorization()
        {
            byte[] expected = new byte[] { 1, 2, 3, 4 };

            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("file-user"));
                }

                if (path == "/api/v4/files/file-1")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(expected)
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            byte[] file = await client.GetFileAsync("file-1");

            Assert.That(file, Is.EqualTo(expected));
            RecordedRequest fileRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/files/file-1");
            Assert.That(fileRequest.RequestUri, Is.EqualTo(new Uri("https://mattermost.example/api/v4/files/file-1")));
            Assert.That(fileRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(fileRequest.Authorization?.Parameter, Is.EqualTo("api-key"));
        }

        [Test]
        public async Task UploadFileAsync_UsesMultipartAndPerRequestAuthorization()
        {
            const string expectedUri = "https://mattermost.example/api/v4/files?channel_id=channel-1";

            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("upload-user"));
                }

                if (path == "/api/v4/files")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "{\"file_infos\":[{\"id\":\"uploaded-file\"}]}");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("payload"));

            FileDetails file = await client.UploadFileAsync("channel-1", "payload.txt", stream);

            Assert.That(file.Id, Is.EqualTo("uploaded-file"));

            RecordedRequest uploadRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/files");
            Assert.That(uploadRequest.RequestUri?.ToString(), Is.EqualTo(expectedUri));
            Assert.That(uploadRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(uploadRequest.Authorization?.Parameter, Is.EqualTo("api-key"));
            Assert.That(uploadRequest.ContentType, Is.EqualTo("multipart/form-data"));
        }

        [Test]
        public async Task DiscoveryApis_UseExpectedRoutesQueriesAndPayload()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                string? query = request.RequestUri?.Query;

                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("current-user"));
                }

                if (request.Method == HttpMethod.Get
                    && path == "/api/v4/users"
                    && query == "?page=1&per_page=2&in_team=team-1&in_channel=channel-1&active=true")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateUserJson("listed-user") + "]");
                }

                if (request.Method == HttpMethod.Post && path == "/api/v4/users/search")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateUserJson("searched-user") + "]");
                }

                if (request.Method == HttpMethod.Get
                    && path == "/api/v4/teams"
                    && query == "?page=3&per_page=4")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateTeamJson("team-1", "core") + "]");
                }

                if (request.Method == HttpMethod.Get && path == "/api/v4/users/user%2F1/teams")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateTeamJson("team-1", "core") + "]");
                }

                if (request.Method == HttpMethod.Get && path == "/api/v4/teams/name/core")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateTeamJson("team-1", "core"));
                }

                if (request.Method == HttpMethod.Get
                    && path == "/api/v4/teams/team-1/channels"
                    && query == "?page=5&per_page=6")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateChannelJson("channel-1", "team-1", "off-topic-pub") + "]");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            IList<User> users = await client.GetUsersAsync(1, 2, inTeamId: "team-1", inChannelId: "channel-1", active: true);
            IList<User> searchedUsers = await client.SearchUsersAsync(" user ", teamId: "team-1", inChannelId: "channel-1", allowInactive: true, limit: 5);
            IList<Team> teams = await client.GetTeamsAsync(3, 4);
            IReadOnlyList<Team> userTeams = await client.GetUserTeamsAsync(" user/1 ");
            Team team = await client.GetTeamByNameAsync("core");
            IList<Channel> channels = await client.GetTeamChannelsAsync("team-1", 5, 6);

            Assert.That(users[0].Id, Is.EqualTo("listed-user"));
            Assert.That(searchedUsers[0].Id, Is.EqualTo("searched-user"));
            Assert.That(teams[0].Id, Is.EqualTo("team-1"));
            Assert.That(userTeams[0].Id, Is.EqualTo("team-1"));
            Assert.That(team.Name, Is.EqualTo("core"));
            Assert.That(channels[0].Name, Is.EqualTo("off-topic-pub"));

            RecordedRequest searchRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/users/search");
            using JsonDocument searchBody = JsonDocument.Parse(searchRequest.ContentBody ?? "{}");
            JsonElement bodyRoot = searchBody.RootElement;

            Assert.That(searchRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(searchRequest.Authorization?.Parameter, Is.EqualTo("api-key"));
            Assert.That(bodyRoot.GetProperty("term").GetString(), Is.EqualTo("user"));
            Assert.That(bodyRoot.GetProperty("team_id").GetString(), Is.EqualTo("team-1"));
            Assert.That(bodyRoot.GetProperty("in_channel_id").GetString(), Is.EqualTo("channel-1"));
            Assert.That(bodyRoot.GetProperty("allow_inactive").GetBoolean(), Is.True);
            Assert.That(bodyRoot.GetProperty("limit").GetInt32(), Is.EqualTo(5));

            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/users?page=1&per_page=2&in_team=team-1&in_channel=channel-1&active=true"), Is.True);
            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/teams?page=3&per_page=4"), Is.True);
            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/users/user%2F1/teams"), Is.True);
            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/teams/name/core"), Is.True);
            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/teams/team-1/channels?page=5&per_page=6"), Is.True);
        }

        [Test]
        public void GetUserTeamsAsync_EmptyUserId_ThrowsArgumentException()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");

            ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
                _ = client.GetUserTeamsAsync(" "));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo("userId"));
        }

        [Test]
        public async Task OpenInteractiveDialogAsync_UsesExpectedRoutePayloadAndNoAuthorization()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (request.Method == HttpMethod.Post && path == "/api/v4/actions/dialogs/open")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "{\"status\":\"OK\"}");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            InteractiveDialog dialog = new InteractiveDialog
            {
                CallbackId = "create-ticket",
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Title",
                        Name = "title",
                        Type = InteractiveDialogElementType.Text
                    }
                }
            };

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", externalHttpClient);

            await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialog);

            RecordedRequest dialogRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/actions/dialogs/open");
            using JsonDocument body = JsonDocument.Parse(dialogRequest.ContentBody ?? "{}");
            JsonElement root = body.RootElement;

            Assert.That(dialogRequest.RequestUri, Is.EqualTo(new Uri("https://mattermost.example/api/v4/actions/dialogs/open")));
            Assert.That(dialogRequest.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(dialogRequest.Authorization, Is.Null);
            Assert.That(root.GetProperty("trigger_id").GetString(), Is.EqualTo("trigger-1"));
            Assert.That(root.GetProperty("url").GetString(), Is.EqualTo("https://example.com/dialog/submit"));
            Assert.That(root.GetProperty("dialog").GetProperty("callback_id").GetString(), Is.EqualTo("create-ticket"));
            Assert.That(root.GetProperty("dialog").GetProperty("elements")[0].GetProperty("type").GetString(), Is.EqualTo("text"));
        }

        [Test]
        public async Task OpenInteractiveDialogAsync_ErrorResponse_ThrowsMattermostClientException()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (request.Method == HttpMethod.Post && path == "/api/v4/actions/dialogs/open")
                {
                    return CreateJsonResponse(
                        HttpStatusCode.BadRequest,
                        "{\"id\":\"api.context.invalid_param.app_error\",\"message\":\"invalid trigger\",\"detailed_error\":\"\",\"request_id\":\"request-1\",\"status_code\":400}");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            InteractiveDialog dialog = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Title",
                        Name = "title",
                        Type = InteractiveDialogElementType.Text
                    }
                }
            };

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", externalHttpClient);

            MattermostClientException? exception = Assert.ThrowsAsync<MattermostClientException>(
                async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialog));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Is.EqualTo("invalid trigger"));
            Assert.That(exception.ResponseJson, Does.Contain("\"status_code\":400"));
            Assert.That(exception.RequestUri, Is.EqualTo("https://mattermost.example/api/v4/actions/dialogs/open"));
            Assert.That(exception.RequestMethod, Is.EqualTo("POST"));
        }

        [Test]
        public void OpenInteractiveDialogAsync_InvalidRequiredFields_ThrowsArgumentException()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");
            InteractiveDialog dialog = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Title",
                        Name = "title",
                        Type = InteractiveDialogElementType.Text
                    }
                }
            };
            InteractiveDialog dialogWithInvalidElement = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Title",
                        Name = "title"
                    }
                }
            };
            InteractiveDialog dialogWithActionButtonWithoutConfiguration = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Open Child",
                        Name = "open_child",
                        Type = InteractiveDialogElementType.ActionButton
                    }
                }
            };
            InteractiveDialog dialogWithActionButtonWithoutUrl = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Open Child",
                        Name = "open_child",
                        Type = InteractiveDialogElementType.ActionButton,
                        ActionButton = new InteractiveDialogActionButton()
                    }
                }
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("", "https://example.com/dialog/submit", dialog));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("trigger-1", "", dialog));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", new InteractiveDialog()));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialogWithInvalidElement));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialogWithActionButtonWithoutConfiguration));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialogWithActionButtonWithoutUrl));
        }

        [Test]
        public void OpenInteractiveDialogAsync_DynamicSelectWithoutDataSourceUrl_ThrowsArgumentException()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");
            InteractiveDialog dialog = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Assignee",
                        Name = "assignee",
                        Type = InteractiveDialogElementType.Select,
                        DataSource = InteractiveDialogDataSource.Dynamic
                    }
                }
            };

            ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialog));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(nameof(InteractiveDialogElement.DataSourceUrl)));
        }

        [Test]
        public void OpenInteractiveDialogAsync_RefreshWithoutSourceUrl_ThrowsArgumentException()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");
            InteractiveDialog dialog = new InteractiveDialog
            {
                Title = "Create Ticket",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Ticket Type",
                        Name = "ticket_type",
                        Type = InteractiveDialogElementType.Select,
                        DataSource = InteractiveDialogDataSource.Users,
                        Refresh = true
                    }
                }
            };

            ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await client.OpenInteractiveDialogAsync("trigger-1", "https://example.com/dialog/submit", dialog));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.ParamName, Is.EqualTo(nameof(InteractiveDialog.SourceUrl)));
        }

        [Test]
        public async Task PostInteractions_UseExpectedRoutesAndPerRequestAuthorization()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(CreatePostInteractionsResponse);

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            Reaction reaction = await client.AddReactionAsync("post-1", ":white_check_mark:");
            IList<Reaction> reactions = await client.GetReactionsAsync("post-1");
            await client.RemoveReactionAsync("post-1", "white_check_mark");
            await client.PinPostAsync("post-1");
            await client.UnpinPostAsync("post-1");

            RecordedRequest addReactionRequest = handler.Requests.Single(request => request.RequestUri?.AbsolutePath == "/api/v4/reactions");
            AssertPostInteractionResults(reaction, reactions);
            AssertAddReactionRequest(addReactionRequest);
            AssertPostInteractionRoutes(handler.Requests);
        }

        private static HttpResponseMessage CreatePostInteractionsResponse(HttpRequestMessage request)
        {
            string requestKey = request.Method.Method + " " + request.RequestUri?.AbsolutePath;
            switch (requestKey)
            {
                case "GET /api/v4/users/me":
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("current-user"));
                case "POST /api/v4/reactions":
                    return CreateJsonResponse(HttpStatusCode.Created, CreateReactionJson("current-user", "post-1", "white_check_mark"));
                case "GET /api/v4/posts/post-1/reactions":
                    return CreateJsonResponse(HttpStatusCode.OK, "[" + CreateReactionJson("current-user", "post-1", "white_check_mark") + "]");
                case "DELETE /api/v4/users/current-user/posts/post-1/reactions/white_check_mark":
                case "POST /api/v4/posts/post-1/pin":
                case "POST /api/v4/posts/post-1/unpin":
                    return CreateJsonResponse(HttpStatusCode.OK, "{}");
                default:
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private static HttpResponseMessage CreatePostResponse(HttpRequestMessage request)
        {
            string requestKey = request.Method.Method + " " + request.RequestUri?.AbsolutePath;
            switch (requestKey)
            {
                case "GET /api/v4/users/me":
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("current-user"));
                case "POST /api/v4/posts":
                    return CreateJsonResponse(HttpStatusCode.Created, "{}");
                default:
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private static void AssertPostInteractionResults(Reaction reaction, IList<Reaction> reactions)
        {
            Assert.That(reaction.UserId, Is.EqualTo("current-user"));
            Assert.That(reactions, Has.Count.EqualTo(1));
            Assert.That(reactions[0].EmojiName, Is.EqualTo("white_check_mark"));
        }

        private static void AssertAddReactionRequest(RecordedRequest addReactionRequest)
        {
            using JsonDocument addReactionBody = JsonDocument.Parse(addReactionRequest.ContentBody ?? "{}");
            JsonElement bodyRoot = addReactionBody.RootElement;

            Assert.That(addReactionRequest.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(addReactionRequest.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(addReactionRequest.Authorization?.Parameter, Is.EqualTo("api-key"));
            Assert.That(bodyRoot.GetProperty("user_id").GetString(), Is.EqualTo("current-user"));
            Assert.That(bodyRoot.GetProperty("post_id").GetString(), Is.EqualTo("post-1"));
            Assert.That(bodyRoot.GetProperty("emoji_name").GetString(), Is.EqualTo("white_check_mark"));
        }

        private static void AssertPostInteractionRoutes(IList<RecordedRequest> requests)
        {
            AssertRecordedRoute(requests, HttpMethod.Get, "/api/v4/posts/post-1/reactions");
            AssertRecordedRoute(requests, HttpMethod.Delete, "/api/v4/users/current-user/posts/post-1/reactions/white_check_mark");
            AssertRecordedRoute(requests, HttpMethod.Post, "/api/v4/posts/post-1/pin");
            AssertRecordedRoute(requests, HttpMethod.Post, "/api/v4/posts/post-1/unpin");
        }

        private static void AssertRecordedRoute(IList<RecordedRequest> requests, HttpMethod method, string path)
        {
            Assert.That(requests.Any(request => request.Method == method && request.RequestUri?.AbsolutePath == path), Is.True);
        }

        [Test]
        public async Task GetReactionsAsync_ErrorResponse_ThrowsMattermostClientException()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("current-user"));
                }

                if (request.Method == HttpMethod.Get && path == "/api/v4/posts/missing-post/reactions")
                {
                    return CreateJsonResponse(
                        HttpStatusCode.NotFound,
                        "{\"id\":\"api.context.permissions.app_error\",\"message\":\"missing post\",\"detailed_error\":\"\",\"request_id\":\"request-1\",\"status_code\":404}");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            MattermostClientException? exception = Assert.ThrowsAsync<MattermostClientException>(async () => await client.GetReactionsAsync("missing-post"));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(exception.Message, Is.EqualTo("missing post"));
            Assert.That(exception.ResponseJson, Does.Contain("\"status_code\":404"));
            Assert.That(exception.RequestUri, Is.EqualTo("https://mattermost.example/api/v4/posts/missing-post/reactions"));
            Assert.That(exception.RequestMethod, Is.EqualTo("GET"));
        }

        [Test]
        public async Task GetReactionsAsync_NullSuccessBody_ReturnsEmptyList()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(request =>
            {
                string? path = request.RequestUri?.AbsolutePath;
                if (path == "/api/v4/users/me")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, CreateUserJson("current-user"));
                }

                if (request.Method == HttpMethod.Get && path == "/api/v4/posts/post-without-reactions/reactions")
                {
                    return CreateJsonResponse(HttpStatusCode.OK, "null");
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            using HttpClient externalHttpClient = new HttpClient(handler);
            using MattermostClient client = new MattermostClient("https://mattermost.example", "api-key", externalHttpClient);

            IList<Reaction> reactions = await client.GetReactionsAsync("post-without-reactions");

            Assert.That(reactions, Is.Empty);
            Assert.That(handler.Requests.Any(request => request.RequestUri?.ToString() == "https://mattermost.example/api/v4/posts/post-without-reactions/reactions"), Is.True);
        }

        [Test]
        public void ReactionEmojiName_Null_ThrowsArgumentException()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");

            Assert.ThrowsAsync<ArgumentException>(async () => await client.AddReactionAsync("post-1", null!));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.RemoveReactionAsync("post-1", null!));
        }

        [Test]
        public async Task Dispose_DoesNotDisposeExternalHttpClient()
        {
            RecordingHttpMessageHandler handler = new RecordingHttpMessageHandler(_ => CreateJsonResponse(HttpStatusCode.OK, "{}"));
            using HttpClient externalHttpClient = new HttpClient(handler);
            MattermostClient client = new MattermostClient("https://mattermost.example", externalHttpClient);

            client.Dispose();

            Assert.That(handler.IsDisposed, Is.False);
            using HttpResponseMessage response = await externalHttpClient.GetAsync("https://outside.example/ping");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Dispose_DisposesInternallyCreatedHttpClient()
        {
            MattermostClient client = new MattermostClient("https://mattermost.example");
            HttpClient internalHttpClient = GetInternalHttpClient(client);

            client.Dispose();

            Assert.ThrowsAsync<ObjectDisposedException>(async () => await internalHttpClient.GetAsync("https://mattermost.example/ping"));
        }

        [Test]
        public void InternalHttpClient_DefaultTimeout_IsPreserved()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");
            HttpClient internalHttpClient = GetInternalHttpClient(client);

            Assert.That(internalHttpClient.Timeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        private static HttpClient GetInternalHttpClient(MattermostClient client)
        {
            FieldInfo? field = typeof(MattermostClient).GetField("_http", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new AssertionException("Cannot access MattermostClient._http field.");
            }

            object? value = field.GetValue(client);
            if (value is not HttpClient httpClient)
            {
                throw new AssertionException("MattermostClient._http field has unexpected value.");
            }

            return httpClient;
        }

        private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json, string? tokenHeader = null)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(tokenHeader))
            {
                response.Headers.Add("Token", tokenHeader);
            }

            return response;
        }

        private static string CreateUserJson(string id)
        {
            return "{" +
            "\"id\":\"" + id + "\"," +
            "\"create_at\":0," +
            "\"update_at\":0," +
            "\"delete_at\":0," +
            "\"username\":\"user-" + id + "\"," +
            "\"email\":\"" + id + "@example.com\"," +
            "\"locale\":\"en\"," +
            "\"timezone\":{}," +
            "\"last_password_update\":0," +
            "\"last_picture_update\":0," +
            "\"is_bot\":false" +
            "}";
        }

        private static string CreateTeamJson(string id, string name)
        {
            return "{" +
            "\"id\":\"" + id + "\"," +
            "\"create_at\":0," +
            "\"update_at\":0," +
            "\"delete_at\":0," +
            "\"display_name\":\"" + name + "\"," +
            "\"name\":\"" + name + "\"," +
            "\"description\":\"\"," +
            "\"email\":\"\"," +
            "\"type\":\"O\"," +
            "\"allowed_domains\":\"\"," +
            "\"invite_id\":\"\"," +
            "\"allow_open_invite\":true," +
            "\"policy_id\":\"\"" +
            "}";
        }

        private static string CreateChannelJson(string id, string teamId, string name)
        {
            return "{" +
            "\"id\":\"" + id + "\"," +
            "\"create_at\":0," +
            "\"update_at\":0," +
            "\"delete_at\":0," +
            "\"team_id\":\"" + teamId + "\"," +
            "\"type\":\"O\"," +
            "\"display_name\":\"" + name + "\"," +
            "\"name\":\"" + name + "\"," +
            "\"header\":\"\"," +
            "\"purpose\":\"\"," +
            "\"last_post_at\":0," +
            "\"total_msg_count\":0," +
            "\"creator_id\":\"current-user\"" +
            "}";
        }

        private static string CreateReactionJson(string userId, string postId, string emojiName)
        {
            return "{" +
            "\"user_id\":\"" + userId + "\"," +
            "\"post_id\":\"" + postId + "\"," +
            "\"emoji_name\":\"" + emojiName + "\"," +
            "\"create_at\":1" +
            "}";
        }

        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

            public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            public List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

            public bool IsDisposed { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RecordedRequest recordedRequest = await RecordedRequest.CreateAsync(request).ConfigureAwait(false);
                Requests.Add(recordedRequest);
                HttpResponseMessage response = _responseFactory(request);
                return response;
            }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class TrackableHttpResponseMessage : HttpResponseMessage
        {
            public TrackableHttpResponseMessage(HttpStatusCode statusCode)
                : base(statusCode)
            {
            }

            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class RecordedRequest
        {
            private RecordedRequest(
                HttpMethod method,
                Uri? requestUri,
                AuthenticationHeaderValue? authorization,
                string? contentType,
                string? contentBody)
            {
                Method = method;
                RequestUri = requestUri;
                Authorization = authorization;
                ContentType = contentType;
                ContentBody = contentBody;
            }

            public HttpMethod Method { get; }

            public Uri? RequestUri { get; }

            public AuthenticationHeaderValue? Authorization { get; }

            public string? ContentType { get; }

            public string? ContentBody { get; }

            public static async Task<RecordedRequest> CreateAsync(HttpRequestMessage request)
            {
                AuthenticationHeaderValue? authorization = request.Headers.Authorization == null
                    ? null
                    : new AuthenticationHeaderValue(request.Headers.Authorization.Scheme, request.Headers.Authorization.Parameter);
                string? contentBody = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);

                return new RecordedRequest(
                    request.Method,
                    request.RequestUri,
                    authorization,
                    request.Content?.Headers.ContentType?.MediaType,
                    contentBody);
            }
        }
    }
}
