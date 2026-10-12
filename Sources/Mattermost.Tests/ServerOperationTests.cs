using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ServerOperationTests
    {
        [Test]
        public async Task SiteUrlTest_UsesTrimmedUrlAndNativeRoute()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/site_url/test"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EqualTo(new[] { "site_url" }));
                Assert.That(body.RootElement.GetProperty("site_url").GetString(), Is.EqualTo("https://mattermost.example/base"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.TestSiteUrlAsync(" https://mattermost.example/base ");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("/relative")]
        [TestCase("ftp://example.com")]
        public async Task SiteUrlTest_RejectsInvalidUrl(string? url)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.TestSiteUrlAsync(url!));
        }

        [Test]
        public async Task FileTest_UsesCurrentConfigurationWithoutBody()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/file/test"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.TestFileStoreConnectionAsync();
        }

        [Test]
        public async Task ConfigPatch_CopiesOnlySelectedNativeSectionsBeforeAuthentication()
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri.PathAndQuery, Is.EqualTo("/chat/api/v4/config/patch"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EqualTo(new[] { "TeamSettings" }));
                Assert.That(body.RootElement.GetProperty("TeamSettings").GetProperty("SiteName").GetString(), Is.EqualTo(""));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"TeamSettings\":{\"SiteName\":\"\",\"MaxUsersPerTeam\":1000},\"ServiceSettings\":{\"EnableDeveloper\":false}}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            using JsonDocument input = JsonDocument.Parse("{\"SiteName\":\"\"}");
            Dictionary<string, JsonElement> sections = new Dictionary<string, JsonElement> { ["TeamSettings"] = input.RootElement };
            Task<IDictionary<string, JsonElement>> pending = client.PatchServerConfigAsync(sections);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            input.Dispose();
            sections.Clear();
            release.SetResult();
            IDictionary<string, JsonElement> result = await pending.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.That(result["TeamSettings"].GetProperty("SiteName").GetString(), Is.Empty);
            Assert.That(result["ServiceSettings"].GetProperty("EnableDeveloper").GetBoolean(), Is.False);
        }

        [Test]
        public async Task ConfigPatch_RejectsNullAndEmptyInput()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PatchServerConfigAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.PatchServerConfigAsync(new Dictionary<string, JsonElement>()));
        }

        [TestCase("", "{}")]
        [TestCase(" ", "{}")]
        [TestCase("TeamSettings", "null")]
        [TestCase("TeamSettings", "[]")]
        [TestCase("TeamSettings", "1")]
        public async Task ConfigPatch_RejectsInvalidSections(string name, string json)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using JsonDocument value = JsonDocument.Parse(json);
            await Assert.ThrowsAsync<ArgumentException>(() => client.PatchServerConfigAsync(new Dictionary<string, JsonElement> { [name] = value.RootElement }));
        }

        [Test]
        public async Task Errors_ArePreserved([Values("config", "site", "file", "addLog", "queryLogs")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"operation failed\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("operation failed"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("config", "site", "file", "addLog", "queryLogs")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values("config", "site", "file", "addLog", "queryLogs")] string operation)
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
            Task pending = InvokeAsync(client, operation, cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "config": return client.PatchServerConfigAsync(new Dictionary<string, JsonElement> { ["TeamSettings"] = JsonSerializer.SerializeToElement(new { SiteName = "Test" }) }, token);
                case "site": return client.TestSiteUrlAsync("https://mattermost.example", token);
                case "file": return client.TestFileStoreConnectionAsync(token);
                case "addLog": return client.AddClientLogAsync("message", cancellationToken: token);
                case "queryLogs": return client.QueryServerLogsAsync(cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
