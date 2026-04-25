using Mattermost.Models;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

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
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
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

        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

            public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            public List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

            public bool IsDisposed { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(RecordedRequest.Create(request));
                HttpResponseMessage response = _responseFactory(request);
                return Task.FromResult(response);
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
                string? contentType)
            {
                Method = method;
                RequestUri = requestUri;
                Authorization = authorization;
                ContentType = contentType;
            }

            public HttpMethod Method { get; }

            public Uri? RequestUri { get; }

            public AuthenticationHeaderValue? Authorization { get; }

            public string? ContentType { get; }

            public static RecordedRequest Create(HttpRequestMessage request)
            {
                AuthenticationHeaderValue? authorization = request.Headers.Authorization == null
                    ? null
                    : new AuthenticationHeaderValue(request.Headers.Authorization.Scheme, request.Headers.Authorization.Parameter);

                return new RecordedRequest(
                    request.Method,
                    request.RequestUri,
                    authorization,
                    request.Content?.Headers.ContentType?.MediaType);
            }
        }
    }
}
