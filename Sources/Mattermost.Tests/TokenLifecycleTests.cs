using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TokenLifecycleTests
    {
        [Test]
        public async Task Revoke_UsesOnlyTheNativeTokenIdentifier()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/tokens/revoke"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EqualTo(new[] { "token_id" }));
                Assert.That(body.RootElement.GetProperty("token_id").GetString(), Is.EqualTo("token/1"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.RevokeUserAccessTokenAsync(" token/1 ");
        }

        [TestCase(0L)]
        [TestCase(4294967297L)]
        public async Task Rotate_UsesNativeExpiryAndReadsTheNewSecret(long expiry)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/tokens/rotate"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "token_id", "expires_at" }));
                Assert.That(body.RootElement.GetProperty("token_id").GetString(), Is.EqualTo("token/1"));
                Assert.That(body.RootElement.GetProperty("expires_at").GetInt64(), Is.EqualTo(expiry));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"token/1\",\"token\":\"new-secret\",\"user_id\":\"user-1\","
                    + "\"description\":\"Rotation test\",\"is_active\":true,\"expires_at\":" + expiry + "}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserAccessToken result = await client.RotateUserAccessTokenAsync(" token/1 ", expiry);
            Assert.That(result.Id, Is.EqualTo("token/1"));
            Assert.That(result.Token, Is.EqualTo("new-secret"));
            Assert.That(result.UserId, Is.EqualTo("user-1"));
            Assert.That(result.Description, Is.EqualTo("Rotation test"));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.ExpiresAt, Is.EqualTo(expiry));
        }

        [Test]
        public async Task Lifecycle_RejectsBlankIdentifiers([Values(false, true)] bool rotate, [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, rotate, id!));
        }

        [Test]
        public async Task Rotate_RejectsNegativeExpiry()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.RotateUserAccessTokenAsync("token-1", -1));
        }

        [Test]
        public async Task Errors_ArePreserved([Values(false, true)] bool rotate,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"token operation failed\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, rotate, "token-1"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("token operation failed"));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("invalid JSON")]
        public async Task Rotate_RejectsInvalidResponses(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.RotateUserAccessTokenAsync("token-1"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool rotate)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, rotate, "token-1", cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, rotate, "token-1"));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool rotate)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                entered.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, rotate, "token-1", cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, bool rotate, string id, CancellationToken token = default)
        {
            if (rotate) { return client.RotateUserAccessTokenAsync(id, cancellationToken: token); }
            return client.RevokeUserAccessTokenAsync(id, token);
        }
    }
}
