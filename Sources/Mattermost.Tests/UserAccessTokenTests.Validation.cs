using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class UserAccessTokenTests
    {
        [Test]
        public async Task BlankIdentifiers_RejectBeforeHttp(
            [Values("get", "create", "user-list", "search", "disable", "enable")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
        }

        [Test]
        public async Task Creation_RejectsBlankDescription([Values(null, "", " ")] string? description)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUserAccessTokenAsync("me", description!));
        }

        [TestCase(-1, 60, false)]
        [TestCase(0, 0, false)]
        [TestCase(0, -1, false)]
        [TestCase(-1, 60, true)]
        [TestCase(0, 0, true)]
        [TestCase(0, -1, true)]
        public async Task InvalidPaging_RejectsBeforeHttp(int page, int perPage, bool all)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (all) { await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetAllUserAccessTokensAsync(page, perPage)); }
            else { await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserAccessTokensAsync("me", page, perPage)); }
        }

        [Test]
        public async Task NativeErrors_PropagateDetails(
            [Values("create", "user-list", "all-list", "get", "search", "disable", "enable", "count")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"not allowed\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "id"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("not allowed"));
        }

        [Test]
        public async Task NullObjects_AreRejected([Values("get", "create")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "id"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("create", "user-list", "all-list", "get", "search", "disable", "enable", "count")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "id"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("create", "user-list", "all-list", "get", "search", "disable", "enable", "count")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, "id", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, "id"));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("create", "user-list", "all-list", "get", "search", "disable", "enable", "count")] string operation)
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
            Task pending = InvokeAsync(client, operation, "id", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }
    }
}
