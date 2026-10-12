using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class AdministrativeReadTests
    {
        [Test]
        public async Task InvalidEmails_UsesNativePagingAndUserModel()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/invalid_emails?page=0&per_page=60"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[{\"id\":\"user1\",\"username\":\"report-user\",\"email\":\"valid@example.test\"}]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<User> result = await client.GetUsersWithInvalidEmailsAsync();
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("user1"));
            Assert.That(result[0].Username, Is.EqualTo("report-user"));
            Assert.That(result[0].Email, Is.EqualTo("valid@example.test"));
        }

        [Test]
        public async Task InvalidEmails_UsesExplicitPagingAndPreservesEmptyPage()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=3&per_page=200"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetUsersWithInvalidEmailsAsync(3, 200), Is.Empty);
        }

        [TestCase(-1, 60)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 201)]
        public async Task InvalidEmails_RejectsInvalidPagingBeforeHttp(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUsersWithInvalidEmailsAsync(page, perPage));
        }

        [Test]
        public async Task Allowed_ReadsEligibilityWithoutStartingAnUpgrade()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/upgrade_to_enterprise/allowed"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CheckEnterpriseUpgradeAllowedAsync();
        }

        [Test]
        public async Task UpgradeStatus_PreservesProgressAndInBodyErrors([Values(0L, 100L, 4294967297L)] long percentage,
            [Values(null, "signature check failed")] string? error)
        {
            string json = "{\"percentage\":" + percentage.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ",\"error\":" + JsonSerializer.Serialize(error) + "}";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/upgrade_to_enterprise/status"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            EnterpriseUpgradeStatus result = await client.GetEnterpriseUpgradeStatusAsync();
            Assert.That(result.Percentage, Is.EqualTo(percentage));
            Assert.That(result.Error, Is.EqualTo(error));
            using JsonDocument expected = JsonDocument.Parse(json);
            Assert.That(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(result), expected.RootElement), Is.True);
        }

        [Test]
        public async Task UpgradeStatus_RejectsMalformedResponse([Values("null", "[]", "invalid JSON", "{\"percentage\":1.5}", "{\"error\":false}")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetEnterpriseUpgradeStatusAsync());
        }

        [Test]
        public async Task InvalidEmails_RejectsMalformedResponse([Values("null", "{}", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetUsersWithInvalidEmailsAsync());
        }

        [Test]
        public async Task NativeErrors_ArePreserved([Values("allowed", "upgrade", "emails")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"operation unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("operation unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("allowed", "upgrade", "emails")] string operation)
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
        public async Task Cancellation_ReachesHttp([Values("allowed", "upgrade", "emails")] string operation)
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
                case "allowed": return client.CheckEnterpriseUpgradeAllowedAsync(token);
                case "upgrade": return client.GetEnterpriseUpgradeStatusAsync(token);
                case "emails": return client.GetUsersWithInvalidEmailsAsync(cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
