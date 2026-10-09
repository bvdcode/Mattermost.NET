using Mattermost.Exceptions;
using Mattermost.Models.Enums;
using Mattermost.Models.Responses.Websocket.Users;
using Mattermost.Models.Users;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserReadTests
    {
        [TestCase("ids", "/users/ids", " user-1 ", "user-1")]
        [TestCase("usernames", "/users/usernames", " @alice ", "alice")]
        [TestCase("statuses", "/users/status/ids", " user-1 ", "user-1")]
        public async Task Batch_SendsArray_EnumeratesOnce(string operation, string route, string input, string expected)
        {
            int enumerations = 0;
            int requests = 0;
            IEnumerable<string> Values()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return input;
                yield return "second";
            }
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4" + route));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                string[]? values = JsonSerializer.Deserialize<string[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(values, Is.EqualTo(new[] { expected, "second" }));
                return JsonResponse(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = CreateClient(http);

            await InvokeAsync(client, operation, values: Values());

            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase(0L, "")]
        [TestCase(1790000000123L, "?since=1790000000123")]
        [TestCase(long.MaxValue, "?since=9223372036854775807")]
        public async Task GetUsersByIds_ReadsProfiles_UsesInvariantTimestamp(long since, string query)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/users/ids" + query));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[{\"id\":\"second\",\"username\":\"bob\"}]"));
            });
            using MattermostClient client = CreateClient(http);
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
                IList<User> users = await client.GetUsersByIdsAsync(new[] { "first", "second" }, since);
                Assert.That(users, Has.Count.EqualTo(1));
                Assert.That(users[0].Id, Is.EqualTo("second"));
                Assert.That(users[0].Username, Is.EqualTo("bob"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Test]
        public async Task GetUsersByUsernames_ReadsProfiles_PreservesInternalAtCharacter()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("[\"alice@host\"]"));
                return JsonResponse(HttpStatusCode.OK, "[{\"id\":\"user-1\",\"username\":\"alice\"}]");
            });
            using MattermostClient client = CreateClient(http);
            IList<User> users = await client.GetUsersByUsernamesAsync(new[] { " @alice@host " });
            Assert.That(users[0].Username, Is.EqualTo("alice"));
        }

        [TestCase("online", UserStatus.Online)]
        [TestCase("offline", UserStatus.Offline)]
        [TestCase("away", UserStatus.Away)]
        [TestCase("dnd", UserStatus.DoNotDisturb)]
        [TestCase("ooo", UserStatus.OutOfOffice)]
        public async Task StatusMethods_ReadPresence(string status, UserStatus expected)
        {
            string body = "{\"user_id\":\"user-1\",\"status\":\"" + status + "\",\"manual\":true,\"last_activity_at\":1790000000123,\"dnd_end_time\":1790003600}";
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                bool batch = request.Method == HttpMethod.Post;
                if (!batch)
                {
                    Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                    Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/status"));
                    Assert.That(request.Content, Is.Null);
                }
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, batch ? "[" + body + "]" : body));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;
            UserPresence single = await api.GetUserStatusAsync(" user/1?# ");
            IList<UserPresence> batch = await api.GetUsersStatusesByIdsAsync(new[] { "user-1" });
            UserStatusUpdate update = JsonSerializer.Deserialize<UserStatusUpdate>(body)!;
            Assert.That(update.Status, Is.EqualTo(expected));

            foreach (UserPresence presence in new[] { single, batch.Single() })
            {
                Assert.That(presence.UserId, Is.EqualTo("user-1"));
                Assert.That(presence.Status, Is.EqualTo(expected));
                Assert.That(presence.IsManual, Is.True);
                Assert.That(presence.LastActivityAt, Is.EqualTo(1790000000123L));
                Assert.That(presence.DoNotDisturbEndTime, Is.EqualTo(1790003600L));
                using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(presence));
                Assert.That(json.RootElement.GetProperty("status").GetString(), Is.EqualTo(status));
            }
        }

        [Test]
        public async Task GetUserStatus_SupportsMe_AndMissingOptionalMetadata()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/users/me/status"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"user_id\":\"user-1\",\"status\":\"offline\"}"));
            });
            using MattermostClient client = CreateClient(http);
            UserPresence presence = await client.GetUserStatusAsync("me");
            Assert.That(presence.IsManual, Is.False);
            Assert.That(presence.LastActivityAt, Is.Zero);
            Assert.That(presence.DoNotDisturbEndTime, Is.Zero);
        }

        [TestCase("single")]
        [TestCase("statuses")]
        public async Task UnrecognizedStatus_IsRejected(string operation)
        {
            string body = "{\"user_id\":\"user-1\",\"status\":\"unrecognized\"}";
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK,
                operation == "statuses" ? "[" + body + "]" : body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, values: new[] { "user-1" }));
        }

        [TestCaseSource(nameof(InvalidCollections))]
        public void InvalidBatch_DoesNotSendRequests(string[]? values)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            foreach (string operation in new[] { "ids", "usernames", "statuses" })
            {
                ArgumentException exception = Assert.Catch<ArgumentException>(() => InvokeAsync(client, operation, values: values))!;
                Assert.That(exception.ParamName, Is.EqualTo(operation == "usernames" ? "usernames" : "userIds"));
            }
        }

        private static IEnumerable<TestCaseData> InvalidCollections()
        {
            yield return new TestCaseData(new object?[] { null });
            yield return new TestCaseData(new object[] { Array.Empty<string>() });
            yield return new TestCaseData(new object[] { new string[] { null! } });
            yield return new TestCaseData(new object[] { new[] { "first", "" } });
            yield return new TestCaseData(new object[] { new[] { " \t" } });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankUserId_DoesNotSendRequests(string? userId)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetUserStatusAsync(userId!))!.ParamName, Is.EqualTo("userId"));
        }

        [Test]
        public void InvalidOptions_DoNotSendRequests()
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetUsersByIdsAsync(new[] { "user-1" }, -1))!.ParamName, Is.EqualTo("since"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetUsersByUsernamesAsync(new[] { " @@ " }))!.ParamName, Is.EqualTo("usernames"));
        }

        [Test]
        public async Task HttpFailures_ArePropagated(
            [Values("ids", "usernames", "single", "statuses")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Access denied\"}")));
            using MattermostClient client = CreateClient(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, values: new[] { "user-1" })))!;
            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Access denied"));
        }

        [Test]
        public async Task InvalidResponses_AreRejected(
            [Values("ids", "usernames", "single", "statuses")] string operation,
            [Values("null", "invalid JSON")] string response)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, response)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, values: new[] { "user-1" }));
        }

        [TestCase("ids")]
        [TestCase("usernames")]
        [TestCase("single")]
        [TestCase("statuses")]
        public void CancelledAndDisposedCalls_DoNotSendRequests(string operation)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token, new[] { "user-1" }));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => InvokeAsync(client, operation, values: new[] { "user-1" }));
        }

        [Test]
        public async Task Cancellation_ReachesAuthenticationAndRequest(
            [Values("ids", "usernames", "single", "statuses")] string operation, [Values(false, true)] bool duringAuthentication)
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("The request must be cancelled.");
            }));
            using MattermostClient client = CreateClient(http);
            Task pending = InvokeAsync(client, operation, cancellation.Token, new[] { "user-1" });
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default, IEnumerable<string>? values = null)
        {
            switch (operation)
            {
                case "ids": return client.GetUsersByIdsAsync(values!, cancellationToken: token);
                case "usernames": return client.GetUsersByUsernamesAsync(values!, token);
                case "single": return client.GetUserStatusAsync("user-1", token);
                case "statuses": return client.GetUsersStatusesByIdsAsync(values!, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static MattermostClient CreateClient(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);
        private static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
