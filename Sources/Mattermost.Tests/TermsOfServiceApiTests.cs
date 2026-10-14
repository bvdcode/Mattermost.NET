using Mattermost.Exceptions;
using Mattermost.Models;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TermsOfServiceApiTests
    {
        private const string TermsJson = "{\"id\":\"terms\",\"create_at\":9007199254740993,\"user_id\":\"author\",\"text\":\"Terms\\nDetails\"}";
        private const string AcceptanceJson = "{\"user_id\":\"user\",\"terms_of_service_id\":\"terms\",\"create_at\":9007199254740993}";
        private static readonly string[] Operations = { "latest", "create", "user", "accept", "reject" };

        [TestCase("latest", "GET", "/terms_of_service", null)]
        [TestCase("create", "POST", "/terms_of_service", "{\"text\":\"Terms\\nDetails\"}")]
        [TestCase("user", "GET", "/users/me/terms_of_service", null)]
        [TestCase("accept", "POST", "/users/me/terms_of_service", "{\"termsOfServiceId\":\"terms/1?#\",\"accepted\":true}")]
        [TestCase("reject", "POST", "/users/me/terms_of_service", "{\"termsOfServiceId\":\"terms/1?#\",\"accepted\":false}")]
        public async Task Requests_UseNativeFieldsAndCurrentAccount(string operation, string method, string route, string? json)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                if (json is null) { Assert.That(request.Content, Is.Null); }
                else { Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(json)); }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Models_PreserveNativeFieldsAndExactTimestamps()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                request.RequestUri!.AbsolutePath.EndsWith("/users/me/terms_of_service", StringComparison.Ordinal) ? AcceptanceJson : TermsJson)));
            using MattermostClient client = ApiTestHttp.Client(http);
            TermsOfService terms = await client.GetTermsOfServiceAsync();
            Assert.That(terms.Id, Is.EqualTo("terms"));
            Assert.That(terms.UserId, Is.EqualTo("author"));
            Assert.That(terms.Text, Is.EqualTo("Terms\nDetails"));
            Assert.That(terms.CreatedAt, Is.EqualTo(9007199254740993L));
            UserTermsOfService acceptance = await client.GetUserTermsOfServiceAsync();
            Assert.That(acceptance.UserId, Is.EqualTo("user"));
            Assert.That(acceptance.TermsOfServiceId, Is.EqualTo("terms"));
            Assert.That(acceptance.CreatedAt, Is.EqualTo(9007199254740993L));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(terms));
            Assert.That(encoded.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "id", "create_at", "user_id", "text" }));
            using JsonDocument encodedAcceptance = JsonDocument.Parse(JsonSerializer.Serialize(acceptance));
            Assert.That(encodedAcceptance.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "user_id", "terms_of_service_id", "create_at" }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \r\n ")]
        public void BlankInputs_RejectBeforeHttp(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.CreateTermsOfServiceAsync(value!));
            Assert.Throws<ArgumentException>(() => client.RegisterTermsOfServiceActionAsync(value!, true));
        }

        [Test]
        public async Task NativeErrors_ArePreserved([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.terms.error\",\"message\":\"terms rejected\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
        }

        [Test]
        public async Task InvalidResponses_AreRejected([ValueSource(nameof(Operations))] string operation, [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([ValueSource(nameof(Operations))] string operation)
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
        public async Task InFlightCancellation_ReachesAuthenticationAndRequest([ValueSource(nameof(Operations))] string operation, [Values(false, true)] bool authentication)
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

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "latest": case "create": return TermsJson;
                case "user": return AcceptanceJson;
                case "accept": case "reject": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static async Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "latest": await client.GetTermsOfServiceAsync(token); break;
                case "create": await client.CreateTermsOfServiceAsync("  Terms\nDetails  ", token); break;
                case "user": await client.GetUserTermsOfServiceAsync(token); break;
                case "accept": await client.RegisterTermsOfServiceActionAsync(" terms/1?# ", true, token); break;
                case "reject": await client.RegisterTermsOfServiceActionAsync(" terms/1?# ", false, token); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
