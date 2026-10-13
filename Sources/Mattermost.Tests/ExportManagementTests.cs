using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ExportManagementTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task List_PreservesExactNamesAndEmptyResults(bool empty)
        {
            string[] names = empty ? Array.Empty<string>() : new[] { "fixture_export.zip", " spaced + имя.zip", "other.jsonl" };
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/exports"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, JsonSerializer.Serialize(names)));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IMattermostSystemClient api = client;
            Assert.That(await api.GetExportsAsync(), Is.EqualTo(names));
        }

        [TestCase(false, "fixture_export.zip")]
        [TestCase(false, " spaced + имя?#.zip ")]
        [TestCase(false, "%2e%2e%2fexport.zip")]
        [TestCase(true, "fixture_export.zip")]
        [TestCase(true, " spaced + имя?#.zip ")]
        [TestCase(true, "%2e%2e%2fexport.zip")]
        public async Task FileOperations_PreserveExactFilenameAndBinaryData(bool delete, string name)
        {
            byte[] bytes = { 0x50, 0x4b, 0x03, 0x04, 0, 255, 128, 10 };
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(delete ? HttpMethod.Delete : HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/exports/" + Uri.EscapeDataString(name)));
                Assert.That(request.Content, Is.Null);
                if (delete) { return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}")); }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (delete) { await client.DeleteExportAsync(name); }
            else { Assert.That(await client.DownloadExportAsync(name), Is.EqualTo(bytes)); }
        }

        [Test]
        public async Task FileOperations_RejectPathsAndControlCharacters(
            [Values(false, true)] bool delete,
            [Values(null, "", " \t", ".", "..", "../file.zip", "..\\file.zip", "/file.zip", "a/file.zip", "a\\file.zip", "bad\0file.zip", "bad\nfile.zip")] string? name)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (delete) { await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteExportAsync(name!)); }
            else { await Assert.ThrowsAsync<ArgumentException>(() => client.DownloadExportAsync(name!)); }
        }

        [TestCase("null")]
        [TestCase("{}")]
        [TestCase("invalid JSON")]
        [TestCase("[1]")]
        public async Task List_RejectsMalformedResponses(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetExportsAsync());
        }

        [Test]
        public async Task NativeErrors_PreserveStatusAndResponse(
            [Values("list", "download", "delete")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.export.error\",\"message\":\"export unavailable\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
            Assert.That(error.Message, Is.EqualTo("export unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("list", "download", "delete")] string operation)
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
        public async Task Cancellation_ReachesAuthenticationOrRequest([Values("list", "download", "delete")] string operation,
            [Values(false, true)] bool authentication)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, operation, source.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetExportsAsync(token);
                case "download": return client.DownloadExportAsync("fixture.zip", token);
                case "delete": return client.DeleteExportAsync("fixture.zip", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
