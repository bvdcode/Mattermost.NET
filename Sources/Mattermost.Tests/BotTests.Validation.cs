using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class BotTests
    {
        [Test]
        public async Task BlankIdentifiers_RejectBeforeHttp(
            [Values("get", "create", "patch", "disable", "enable", "assign")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
        }

        [TestCase(-1, 60)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        public async Task InvalidPaging_RejectsBeforeHttp(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetBotsAsync(page, perPage));
        }

        [Test]
        public async Task Patch_RejectsBlankUsername([Values("", " ")] string name)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.PatchBotAsync("bot", username: name));
        }

        [Test]
        public async Task Patch_RejectsNoChanges()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.PatchBotAsync("bot"));
        }

        [Test]
        public async Task Assignment_RejectsBlankOwner([Values(null, "", " ")] string? owner)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.AssignBotAsync("bot", owner!));
        }

        [Test]
        public async Task NativeErrors_PropagateDetails(
            [Values("list", "get", "create", "patch", "disable", "enable", "assign")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"not allowed\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "bot"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("not allowed"));
        }

        [Test]
        public async Task NullObjects_AreRejected([Values("get", "create", "patch", "disable", "enable", "assign")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "bot"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("list", "get", "create", "patch", "disable", "enable", "assign")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "bot"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("list", "get", "create", "patch", "disable", "enable", "assign")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, "bot", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, "bot"));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("list", "get", "create", "patch", "disable", "enable", "assign")] string operation)
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
            Task pending = InvokeAsync(client, operation, "bot", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }
    }
}
