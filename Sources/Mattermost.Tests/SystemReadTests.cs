using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class SystemReadTests
    {
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public async Task Health_DoesNotAuthenticateAndDecodesExactNames(bool backends, bool rest)
        {
            int requests = 0;
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Headers.Authorization, Is.Null);
                Assert.That(request.Content, Is.Null);
                string query = "get_server_status=" + backends.ToString().ToLowerInvariant() + "&use_rest_semantics=" + rest.ToString().ToLowerInvariant();
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/system/ping?" + query));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                    "{\"status\":\"OK\",\"database_status\":\"OK\",\"filestore_status\":\"OK\",\"AndroidLatestVersion\":\"2.1\",\"AndroidMinVersion\":\"2.0\",\"IosLatestVersion\":\"2.3\",\"IosMinVersion\":\"2.2\",\"ActiveSearchBackend\":\"database\"}"));
            }));
            using MattermostClient client = new MattermostClient("https://mattermost.example/chat", http);
            SystemStatusResponse status = await client.GetSystemStatusAsync(backends, rest);
            Assert.That(requests, Is.EqualTo(1));
            Assert.That(status.Status, Is.EqualTo("OK"));
            Assert.That(status.DatabaseStatus, Is.EqualTo("OK"));
            Assert.That(status.FileStoreStatus, Is.EqualTo("OK"));
            Assert.That(status.AndroidLatestVersion, Is.EqualTo("2.1"));
            Assert.That(status.AndroidMinVersion, Is.EqualTo("2.0"));
            Assert.That(status.IosLatestVersion, Is.EqualTo("2.3"));
            Assert.That(status.IosMinVersion, Is.EqualTo("2.2"));
            Assert.That(status.ActiveSearchBackend, Is.EqualTo("database"));
        }

        [Test]
        public async Task Health_DoesNotValidateAnApiToken()
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                Assert.That(request.Headers.Authorization, Is.Null);
                Assert.That(request.RequestUri!.AbsolutePath, Does.EndWith("/system/ping"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            SystemStatusResponse status = await client.GetSystemStatusAsync();
            Assert.That(status.Status, Is.EqualTo("OK"));
            Assert.That(status.DatabaseStatus, Is.Null);
            Assert.That(status.FileStoreStatus, Is.Null);
            Assert.That(status.ActiveSearchBackend, Is.Null);
        }

        [Test]
        public async Task Health_UnhealthyStatusIsReturnedWithRestSemantics()
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"UNHEALTHY\",\"database_status\":\"UNHEALTHY\"}"))));
            using MattermostClient client = new MattermostClient("https://mattermost.example", http);
            SystemStatusResponse status = await client.GetSystemStatusAsync(checkBackends: true, useRestSemantics: true);
            Assert.That(status.Status, Is.EqualTo("UNHEALTHY"));
            Assert.That(status.DatabaseStatus, Is.EqualTo("UNHEALTHY"));
        }

        [TestCase("[\"Europe/Berlin\",\"America/Los_Angeles\"]", 2)]
        [TestCase("[]", 0)]
        public async Task Timezones_AuthenticateAndReadStringArray(string body, int count)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/system/timezones"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<string> zones = await client.GetSupportedTimezonesAsync();
            Assert.That(zones, Has.Count.EqualTo(count));
        }

        [Test]
        public async Task Errors_PreserveDetails([Values(false, true)] bool health,
            [Values(HttpStatusCode.Unauthorized, HttpStatusCode.BadGateway, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = Respond(health, status, "{\"message\":\"unavailable\"}");
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, health));
            Assert.That(exception!.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("unavailable"));
            Assert.That(exception.RequestMethod, Is.EqualTo("GET"));
        }

        [Test]
        public async Task Health_DefaultHttp500PreservesUnhealthyBody()
        {
            const string body = "{\"status\":\"UNHEALTHY\",\"database_status\":\"UNHEALTHY\"}";
            using HttpClient http = Respond(true, HttpStatusCode.InternalServerError, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => client.GetSystemStatusAsync());
            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(exception.ResponseJson, Is.EqualTo(body));
        }

        [Test]
        public async Task MalformedResponses_Throw([Values(false, true)] bool health, [Values("null", "not json")] string body)
        {
            using HttpClient http = Respond(health, HttpStatusCode.OK, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, health));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool health)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => InvokeAsync(client, health, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, health));
        }

        private static HttpClient Respond(bool health, HttpStatusCode status, string body)
        {
            if (health)
            {
                return new HttpClient(new DelegateHttpMessageHandler((request, _) =>
                {
                    Assert.That(request.RequestUri!.AbsolutePath, Does.EndWith("/system/ping"));
                    Assert.That(request.Headers.Authorization, Is.Null);
                    return Task.FromResult(ApiTestHttp.Json(status, body));
                }));
            }
            return ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, body)));
        }

        private static Task InvokeAsync(MattermostClient client, bool health, CancellationToken token = default)
        {
            if (health) { return client.GetSystemStatusAsync(cancellationToken: token); }
            return client.GetSupportedTimezonesAsync(token);
        }
    }
}
