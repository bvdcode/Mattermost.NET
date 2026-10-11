using Mattermost.Exceptions;
using Mattermost.Models.Enums;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserStatusWriteTests
    {
        [TestCase(UserStatus.Online, "online")]
        [TestCase(UserStatus.Offline, "offline")]
        [TestCase(UserStatus.Away, "away")]
        [TestCase(UserStatus.DoNotDisturb, "dnd")]
        public async Task Presence_UsesWireNamesAndUnixSeconds(UserStatus status, string wireName)
        {
            const long expiry = 4200000000;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23/status"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                Assert.That(body.RootElement.GetProperty("user_id").GetString(), Is.EqualTo("user/1?#"));
                Assert.That(body.RootElement.GetProperty("status").GetString(), Is.EqualTo(wireName));
                Assert.That(body.RootElement.GetProperty("dnd_end_time").GetInt64(), Is.EqualTo(expiry));
                return ApiTestHttp.Json(HttpStatusCode.OK,
                    JsonSerializer.Serialize(new { user_id = "user/1?#", status = wireName, manual = true, dnd_end_time = expiry }));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IMattermostClient api = client;
            UserPresence result = await api.UpdateUserStatusAsync(" user/1?# ", status, expiry);
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.UserId, Is.EqualTo("user/1?#"));
            Assert.That(result.IsManual, Is.True);
            Assert.That(result.DoNotDisturbEndTime, Is.EqualTo(expiry));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Presence_MeAliasUsesActualPayloadId(bool alreadyAuthenticated)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/users/me/status"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("user_id").GetString(), Is.EqualTo("user-1"));
                Assert.That(body.RootElement.GetProperty("dnd_end_time").GetInt64(), Is.Zero);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"user_id\":\"user-1\",\"status\":\"away\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (alreadyAuthenticated) { await client.GetMeAsync(); }
            await client.UpdateUserStatusAsync(" me ", UserStatus.Away);
        }

        [TestCase(CustomStatusDuration.Never, "")]
        [TestCase(CustomStatusDuration.ThirtyMinutes, "thirty_minutes")]
        [TestCase(CustomStatusDuration.OneHour, "one_hour")]
        [TestCase(CustomStatusDuration.FourHours, "four_hours")]
        [TestCase(CustomStatusDuration.Today, "today")]
        [TestCase(CustomStatusDuration.ThisWeek, "this_week")]
        [TestCase(CustomStatusDuration.DateAndTime, "date_and_time")]
        public async Task CustomStatus_PreservesIdentityAndUsesExplicitExpiry(CustomStatusDuration duration, string wireName)
        {
            foreach (bool removeRecent in new[] { false, true })
            {
                DateTimeOffset? expiry = null;
                if (duration != CustomStatusDuration.Never)
                {
                    expiry = new DateTimeOffset(2200, 1, 2, 3, 4, 5, TimeSpan.FromHours(3)).AddTicks(1234567);
                }
                using HttpClient http = ApiTestHttp.Create(async (request, token) =>
                {
                    string suffix = "/status/custom";
                    if (removeRecent) { suffix += "/recent/delete"; }
                    Assert.That(request.Method, Is.EqualTo(removeRecent ? HttpMethod.Post : HttpMethod.Put));
                    Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23" + suffix));
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    Assert.That(body.RootElement.GetProperty("emoji").GetString(), Is.EqualTo(" :rocket: "));
                    Assert.That(body.RootElement.GetProperty("text").GetString(), Is.EqualTo("  Working ✨  "));
                    Assert.That(body.RootElement.GetProperty("duration").GetString(), Is.EqualTo(wireName));
                    if (expiry is DateTimeOffset expiration)
                    {
                        DateTimeOffset actual = body.RootElement.GetProperty("expires_at").GetDateTimeOffset();
                        Assert.That(actual, Is.EqualTo(expiration));
                        Assert.That(actual.Offset, Is.EqualTo(TimeSpan.Zero));
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(4));
                    }
                    else
                    {
                        Assert.That(body.RootElement.TryGetProperty("expires_at", out _), Is.False);
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                    }
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
                });
                using MattermostClient client = ApiTestHttp.Client(http);
                IMattermostClient api = client;
                if (removeRecent)
                {
                    await api.DeleteRecentUserCustomStatusAsync(" user/1?# ", " :rocket: ", "  Working ✨  ", duration, expiry);
                }
                else
                {
                    await api.UpdateUserCustomStatusAsync(" user/1?# ", " :rocket: ", "  Working ✨  ", duration, expiry);
                }
            }
        }

        [Test]
        public async Task RecentStatus_AllowsExpiredEntriesAndExplicitZeroExpiry()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.DeleteRecentUserCustomStatusAsync("me", "rocket", "old", CustomStatusDuration.DateAndTime,
                DateTimeOffset.FromUnixTimeSeconds(1));
            await client.DeleteRecentUserCustomStatusAsync("me", "rocket", "old", expiresAt: DateTimeOffset.MinValue);
        }

        [TestCase("", "Text only")]
        [TestCase("rocket", "")]
        public async Task CustomStatus_AllowsEmojiOrTextAlone(string emoji, string text)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateUserCustomStatusAsync("me", emoji, text);
            await client.DeleteRecentUserCustomStatusAsync("me", emoji, text);
        }

        [Test]
        public async Task ClearCustomStatus_UsesDeleteWithoutBody()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23/status/custom"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ClearUserCustomStatusAsync(" user/1?# ");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations)
            {
                ArgumentException? exception = await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
                Assert.That(exception!.ParamName, Is.EqualTo("userId"));
            }
        }

        [TestCase(UserStatus.Unknown)]
        [TestCase(UserStatus.OutOfOffice)]
        [TestCase((UserStatus)100)]
        public async Task UnsupportedPresence_IsRejected(UserStatus status)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateUserStatusAsync("me", status));
        }

        [Test]
        public async Task InvalidPresenceExpiry_IsRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateUserStatusAsync("me", UserStatus.DoNotDisturb, -1));
        }

        [Test]
        public async Task InvalidCustomStatus_IsRejected([Values(false, true)] bool removeRecent)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Task Call(string? emoji, string? text, CustomStatusDuration duration, DateTimeOffset? expiry)
            {
                if (removeRecent) { return client.DeleteRecentUserCustomStatusAsync("me", emoji!, text!, duration, expiry); }
                return client.UpdateUserCustomStatusAsync("me", emoji!, text!, duration, expiry);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(() => Call(null, "text", CustomStatusDuration.Never, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Call("rocket", null, CustomStatusDuration.Never, null));
            await Assert.ThrowsAsync<ArgumentException>(() => Call("", "", CustomStatusDuration.Never, null));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Call("rocket", "text", (CustomStatusDuration)100, null));
            await Assert.ThrowsAsync<ArgumentException>(() => Call("rocket", "text", CustomStatusDuration.Never, DateTimeOffset.UtcNow));
            await Assert.ThrowsAsync<ArgumentException>(() => Call("rocket", "text", CustomStatusDuration.OneHour, null));
            await Assert.ThrowsAsync<ArgumentException>(() => Call("rocket", "text", CustomStatusDuration.OneHour, DateTimeOffset.MinValue));
        }

        [Test]
        public async Task ApiErrors_PreserveDetails(
            [Values("presence", "custom", "clear", "recent")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(exception!.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("rejected"));
            Assert.That(exception.RequestUri, Does.Contain("/users/me/status"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected(
            [Values("presence", "custom", "clear", "recent")] string operation, [Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("presence", "custom", "clear", "recent")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, token: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesAuthenticationAndRequest(
            [Values("presence", "custom", "clear", "recent")] string operation, [Values(false, true)] bool duringAuthentication)
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource source = new CancellationTokenSource();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                entered.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, operation, token: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static readonly string[] Operations = { "presence", "custom", "clear", "recent" };

        private static Task InvokeAsync(IMattermostClient client, string operation, string id = "me", CancellationToken token = default)
        {
            switch (operation)
            {
                case "presence": return client.UpdateUserStatusAsync(id, UserStatus.Away, cancellationToken: token);
                case "custom": return client.UpdateUserCustomStatusAsync(id, "rocket", "Working", cancellationToken: token);
                case "clear": return client.ClearUserCustomStatusAsync(id, token);
                case "recent": return client.DeleteRecentUserCustomStatusAsync(id, "rocket", "Working", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
