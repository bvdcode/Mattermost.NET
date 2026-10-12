using Mattermost.Constants;
using Mattermost.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ServerBusyTests
    {
        [TestCase(1)]
        [TestCase(3600)]
        [TestCase(86400)]
        public async Task Set_UsesBodylessNativePost(int seconds)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/server_busy?seconds=" + seconds.ToString(CultureInfo.InvariantCulture)));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SetServerBusyAsync(seconds);
        }

        [Test]
        public async Task Set_DefaultMatchesNativeDuration()
        {
            Assert.That(MattermostApiLimits.DefaultServerBusySeconds, Is.EqualTo(3600));
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?seconds=3600"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SetServerBusyAsync();
        }

        [Test]
        public async Task Clear_UsesBodylessNativeDelete()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/server_busy"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ClearServerBusyAsync();
        }

        [Test]
        public async Task Set_RejectsOutOfRangeBeforeAuthentication([Values(-1, 0, 86401, int.MinValue, int.MaxValue)] int seconds)
        {
            Assert.That(MattermostApiLimits.MaxServerBusySeconds, Is.EqualTo(86400));
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SetServerBusyAsync(seconds));
        }

        [Test]
        public async Task Errors_PreserveNativeStatus([Values(false, true)] bool clear,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"busy request denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, clear));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("busy request denied"));
        }

        [Test]
        public async Task Responses_RejectMalformedJson([Values(false, true)] bool clear,
            [Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, clear));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeAuthentication([Values(false, true)] bool clear)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, clear, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, clear));
        }

        [Test]
        public async Task Cancellation_ReachesRequest([Values(false, true)] bool clear)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                entered.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, clear, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, bool clear, CancellationToken token = default)
        {
            if (clear) { return client.ClearServerBusyAsync(token); }
            return client.SetServerBusyAsync(cancellationToken: token);
        }
    }
}
