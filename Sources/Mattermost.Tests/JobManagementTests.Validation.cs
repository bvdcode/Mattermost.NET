using Mattermost.Enums;
using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class JobManagementTests
    {
        [Test]
        public async Task Create_RejectsBlankTypeBeforeHttp([Values(null, "", " ")] string? type)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateJobAsync(type!));
        }

        [Test]
        public async Task Create_RejectsNullMapValueBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Dictionary<string, string> data = new Dictionary<string, string> { ["file"] = null! };
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateJobAsync("import_process", data));
        }

        [Test]
        public async Task Identifier_RejectsBlankBeforeHttp([Values(false, true)] bool status, [Values(null, "", " ")] string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<ArgumentException>(() => status ? client.UpdateJobStatusAsync(id!, JobStatusUpdate.CancelRequested) : client.CancelJobAsync(id!));
        }

        [Test]
        public async Task Status_RejectsInvalidEnumBeforeHttp([Values(-1, int.MaxValue)] int value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateJobStatusAsync("job1", (JobStatusUpdate)value));
        }

        [Test]
        public async Task Create_RejectsMalformedResponse([Values("null", "[]", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Created, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.CreateJobAsync("active_users"));
        }

        [Test]
        public async Task Errors_KeepNativePermissionsAndTransitions([Values("create", "cancel", "status")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"job unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("job unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("create", "cancel", "status")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values("create", "cancel", "status")] string operation)
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
            Task pending = InvokeAsync(client, operation, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "create": return client.CreateJobAsync("active_users", cancellationToken: token);
                case "cancel": return client.CancelJobAsync("job1", token);
                case "status": return client.UpdateJobStatusAsync("job1", JobStatusUpdate.CancelRequested, cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
