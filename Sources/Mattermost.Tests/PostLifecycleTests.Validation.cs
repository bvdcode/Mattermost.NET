using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class PostLifecycleTests
    {
        [Test]
        public async Task BlankPostIds_RejectBeforeHttp(
            [Values("history", "info", "restore", "unread", "reminder")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
        }

        [Test]
        public async Task BlankSecondaryIds_RejectBeforeHttp(
            [Values("restore", "unread", "reminder")] string operation,
            [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => InvokeSecondaryAsync(client, operation, id!));
        }

        [Test]
        public async Task NativeErrors_PropagateDetails(
            [Values("history", "info", "restore", "unread", "reminder")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"not available\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "post"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("not available"));
        }

        [Test]
        public async Task MalformedJson_IsRejected([Values("history", "info", "restore", "unread", "reminder")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "post"));
        }

        [Test]
        public async Task IncompatibleShapes_AreRejected([Values("history", "info", "restore", "unread")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "\"not a result\"")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "post"));
        }

        [Test]
        public async Task NullObjects_AreRejected([Values("info", "restore", "unread")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, "post"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("history", "info", "restore", "unread", "reminder")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, "post", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, "post"));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("history", "info", "restore", "unread", "reminder")] string operation)
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
            Task pending = InvokeAsync(client, operation, "post", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeSecondaryAsync(MattermostClient client, string operation, string id)
        {
            switch (operation)
            {
                case "restore": return client.RestorePostVersionAsync("post", id);
                case "unread": return client.MarkPostUnreadAsync(id, "post");
                case "reminder": return client.SetPostReminderAsync(id, "post", DateTimeOffset.UnixEpoch);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
