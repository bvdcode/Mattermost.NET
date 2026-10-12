using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ImportManagementTests
    {
        [Test]
        public async Task List_UsesNativeGetAndPreservesNames([Values(false, true)] bool empty)
        {
            string[] names = empty ? Array.Empty<string>() : new[] { "upload_fixture.zip", " spaced + имя.zip", "other.jsonl" };
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/imports"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, JsonSerializer.Serialize(names)));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetImportsAsync(), Is.EqualTo(names));
        }

        [Test]
        public async Task Delete_EscapesExactLeafNameWithoutTrimming([Values("upload_fixture.zip", " spaced + имя.zip", "%2e%2e%2fexport.zip")] string name)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/imports/" + Uri.EscapeDataString(name)));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.DeleteImportAsync(name);
        }

        [Test]
        public async Task Delete_RejectsInvalidOrNonLeafNames([Values(null, "", " ", ".", "..", ".zip", "other.jsonl", "file.ZIP",
            "file.zip ", "../export.zip", "..\\export.zip", "/file.zip", "a/file.zip", "a\\file.zip", "bad\0file.zip", "bad\nfile.zip")] string? name)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteImportAsync(name!));
        }

        [Test]
        public async Task List_RejectsMalformedValues([Values("null", "{}", "invalid JSON", "[1]")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetImportsAsync());
        }

        [Test]
        public async Task NativeErrors_ArePreserved([Values(false, true)] bool delete,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"import unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, delete));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("import unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool delete)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, delete, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, delete));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool delete)
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
            Task pending = InvokeAsync(client, delete, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, bool delete, CancellationToken token = default)
        {
            if (delete) { return client.DeleteImportAsync("fixture.zip", token); }
            return client.GetImportsAsync(token);
        }
    }
}
