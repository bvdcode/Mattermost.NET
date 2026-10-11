using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ClientSettingsTests
    {
        [TestCase("config", false, "/config/client")]
        [TestCase("config", true, "/config/client")]
        [TestCase("license", false, "/license/client")]
        [TestCase("license", true, "/license/client")]
        [TestCase("load", true, "/license/load_metric")]
        public async Task Requests_UseNativeRoutesAndExplicitAuthorization(string operation, bool authenticated, string path)
        {
            int authRequests = 0;
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    authRequests++;
                    return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                Assert.That(request.Content, Is.Null);
                if (authenticated) { Assert.That(request.Headers.Authorization!.Parameter, Is.EqualTo("test-token")); }
                else { Assert.That(request.Headers.Authorization, Is.Null); }
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response(operation)));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation, authenticated);
            Assert.That(authRequests, Is.EqualTo(authenticated ? 1 : 0));
        }

        [TestCase("config")]
        [TestCase("license")]
        public async Task PublicReads_DoNotLeakAnInitializedToken(string operation)
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                Assert.That(request.Headers.Authorization, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response(operation)));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetMeAsync();
            await InvokeAsync(client, operation, false);
        }

        [Test]
        public async Task PublicReads_WorkWithoutCredentialsAndKeepNativeStrings()
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                Assert.That(request.Headers.Authorization, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"EnableCustomEmoji\":\"true\",\"Version\":\"11.3\",\"SiteName\":\"Team ✨\"}"));
            }));
            using MattermostClient client = new MattermostClient("https://mattermost.example", http);
            IDictionary<string, string> config = await client.GetClientConfigAsync();
            Assert.That(config["EnableCustomEmoji"], Is.EqualTo("true"));
            Assert.That(config["Version"], Is.EqualTo("11.3"));
            Assert.That(config["SiteName"], Is.EqualTo("Team ✨"));
            await client.GetClientLicenseAsync();
        }

        [TestCase(0)]
        [TestCase(1000)]
        [TestCase(4200000000123)]
        public async Task LoadMetric_ReturnsNativeInteger(long expected)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"load\":" + expected + "}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetLicenseLoadMetricAsync(), Is.EqualTo(expected));
        }

        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("{\"load\":null}")]
        [TestCase("{\"load\":\"1000\"}")]
        [TestCase("{\"load\":0.5}")]
        [TestCase("{\"load\":9223372036854775808}")]
        public async Task InvalidMetricShapes_AreRejected(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetLicenseLoadMetricAsync());
        }

        [Test]
        public async Task Errors_PreserveServerDetails([Values("config", "license", "load")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, true));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected([Values("config", "license", "load")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation, true));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("config", "license", "load")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, true, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, true));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("config", "license", "load")] string operation)
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
            Task pending = InvokeAsync(client, operation, true, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "config": return "{\"Version\":\"11.3\"}";
                case "license": return "{\"IsLicensed\":\"false\"}";
                case "load": return "{\"load\":0}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, bool authenticated, CancellationToken token = default)
        {
            switch (operation)
            {
                case "config": return client.GetClientConfigAsync(authenticated, token);
                case "license": return client.GetClientLicenseAsync(authenticated, token);
                case "load": return client.GetLicenseLoadMetricAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
