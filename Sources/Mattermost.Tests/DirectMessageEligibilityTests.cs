using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class DirectMessageEligibilityTests
    {
        [Test]
        public async Task Get_UsesNativeRouteAndBoolean([Values(false, true)] bool allowed)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/sharedchannels/users/user%2F1/can_dm/other%3F2"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, JsonSerializer.Serialize(new { can_dm = allowed })));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.CanUserDirectMessageAsync(" user/1 ", " other?2 "), Is.EqualTo(allowed));
        }

        [Test]
        public async Task Get_RejectsBlankIdentifiers([Values(false, true)] bool source,
            [Values(null, "", " \t")] string? identifier)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(() => client.CanUserDirectMessageAsync(
                source ? identifier! : "user-1", source ? "user-2" : identifier!));
            Assert.That(error!.ParamName, Is.EqualTo(source ? "userId" : "otherUserId"));
        }

        [TestCase("null")]
        [TestCase("invalid JSON")]
        [TestCase("{}")]
        [TestCase("{\"can_dm\":null}")]
        [TestCase("{\"can_dm\":\"true\"}")]
        [TestCase("{\"CanDm\":true}")]
        public async Task Get_RejectsMalformedResponse(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.CanUserDirectMessageAsync("user-1", "user-2"));
        }

        [Test]
        public async Task Get_PreservesNativeErrors([Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"direct message check denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.CanUserDirectMessageAsync("user-1", "user-2"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("direct message check denied"));
        }

        [Test]
        public async Task Get_RejectsCancellationAndDisposalBeforeAuthentication()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.CanUserDirectMessageAsync("user-1", "user-2", cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.CanUserDirectMessageAsync("user-1", "user-2"));
        }

        [Test]
        public async Task Get_PassesRequestCancellation()
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Expected request cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => client.CanUserDirectMessageAsync("user-1", "user-2", cancellation.Token));
        }
    }
}
