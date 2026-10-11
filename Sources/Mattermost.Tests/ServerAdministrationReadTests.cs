using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ServerAdministrationReadTests
    {
        [TestCase("audits", "/chat/api/v4/audits?page=2&per_page=3", "[{\"id\":\"audit\",\"create_at\":1800000000000,\"user_id\":\"user\",\"action\":\"action\",\"extra_info\":\"details\",\"ip_address\":\"192.0.2.1\",\"session_id\":\"session\"}]")]
        [TestCase("logs", "/chat/api/v4/logs?page=2&logs_per_page=3", "[\"line ✨\"]")]
        [TestCase("analytics", "/chat/api/v4/analytics/old?name=standard&team_id=team%2F1", "[{\"name\":\"metric\",\"value\":1.25}]")]
        [TestCase("busy", "/chat/api/v4/server_busy", "{\"busy\":true,\"expires\":5000000001,\"expires_ts\":\"2128-06-11T08:53:21Z\"}")]
        public async Task Reads_UseNativeRoutesAndCompleteModels(string operation, string route, string json)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object result = await ReadAsync(client, operation);
            switch (operation)
            {
                case "audits":
                    UserAudit audit = ((IList<UserAudit>)result).Single();
                    Assert.Multiple(() =>
                    {
                        Assert.That(audit.Id, Is.EqualTo("audit"));
                        Assert.That(audit.CreatedAt, Is.EqualTo(1800000000000L));
                        Assert.That(audit.UserId, Is.EqualTo("user"));
                        Assert.That(audit.Action, Is.EqualTo("action"));
                        Assert.That(audit.ExtraInfo, Is.EqualTo("details"));
                        Assert.That(audit.IpAddress, Is.EqualTo("192.0.2.1"));
                        Assert.That(audit.SessionId, Is.EqualTo("session"));
                    });
                    break;
                case "logs": Assert.That(((IList<string>)result).Single(), Is.EqualTo("line ✨")); break;
                case "analytics":
                    AnalyticsRow row = ((IList<AnalyticsRow>)result).Single();
                    Assert.That(row.Name, Is.EqualTo("metric"));
                    Assert.That(row.Value, Is.EqualTo(1.25));
                    break;
                case "busy":
                    ServerBusyState state = (ServerBusyState)result;
                    Assert.That(state.IsBusy, Is.True);
                    Assert.That(state.ExpiresAt, Is.EqualTo(5000000001L));
                    Assert.That(state.ExpiresTimestamp, Is.EqualTo("2128-06-11T08:53:21Z"));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [TestCase(AnalyticsReport.Standard, "standard")]
        [TestCase(AnalyticsReport.BotPostCountsByDay, "bot_post_counts_day")]
        [TestCase(AnalyticsReport.PostCountsByDay, "post_counts_day")]
        [TestCase(AnalyticsReport.UserCountsWithPostsByDay, "user_counts_with_posts_day")]
        [TestCase(AnalyticsReport.ExtraCounts, "extra_counts")]
        public async Task Analytics_MapsEveryReportAndOmitsUnspecifiedTeam(AnalyticsReport report, string name)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?name=" + name));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetServerAnalyticsAsync(report);
        }

        [Test]
        public async Task Defaults_UseNativePagingAndIdleState()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                switch (request.RequestUri!.AbsolutePath)
                {
                    case "/chat/api/v4/logs":
                        Assert.That(request.RequestUri.Query, Is.EqualTo("?page=0&logs_per_page=10000"));
                        return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
                    case "/chat/api/v4/audits":
                        Assert.That(request.RequestUri.Query, Is.EqualTo("?page=0&per_page=60"));
                        return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
                    case "/chat/api/v4/server_busy":
                        return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"busy\":false,\"expires\":-62135596800}"));
                    default: throw new AssertionException("Unexpected route.");
                }
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetServerLogsAsync();
            await client.GetServerAuditsAsync();
            ServerBusyState state = await client.GetServerBusyAsync();
            Assert.That(state.IsBusy, Is.False);
            Assert.That(state.ExpiresAt, Is.EqualTo(-62135596800L));
            Assert.That(state.ExpiresTimestamp, Is.Null);
        }

        [Test]
        public async Task EmptyCollections_NormalizeNativeNull([Values("audits", "logs", "analytics")] string operation,
            [Values("null", "[]")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await ReadAsync(client, operation), Is.Empty);
        }

        [TestCase(-1, 3)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 10001)]
        public async Task Logs_RejectInvalidPaging(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetServerLogsAsync(page, perPage));
        }

        [TestCase(-1, 3)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        public async Task Audits_RejectInvalidPaging(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetServerAuditsAsync(page, perPage));
        }

        [TestCase(-1)]
        [TestCase(999)]
        public async Task Analytics_RejectsInvalidReports(int report)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetServerAnalyticsAsync((AnalyticsReport)report));
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task Analytics_RejectsBlankSuppliedTeam(string teamId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetServerAnalyticsAsync(teamId: teamId));
        }

        [Test]
        public async Task NativeErrors_PropagateDetails([Values("audits", "logs", "analytics", "busy")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"not allowed\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("not allowed"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("audits", "logs", "analytics", "busy")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task Analytics_PreservesNativeServerFailures()
        {
            const string errorJson = "{\"id\":\"app.webhooks.analytics_outgoing_count.app_error\",\"message\":\"Unable to count the outgoing webhooks.\"}";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?name=extra_counts&team_id=team"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.InternalServerError, errorJson));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.GetServerAnalyticsAsync(AnalyticsReport.ExtraCounts, "team"));
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(error.ResponseJson, Is.EqualTo(errorJson));
            Assert.That(error.Message, Is.EqualTo("Unable to count the outgoing webhooks."));
        }

        [Test]
        public async Task Busy_NullObjectIsRejected()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetServerBusyAsync());
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("audits", "logs", "analytics", "busy")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => ReadAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("audits", "logs", "analytics", "busy")] string operation)
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
            Task pending = ReadAsync(client, operation, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static async Task<object> ReadAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "audits": return await client.GetServerAuditsAsync(2, 3, token);
                case "logs": return await client.GetServerLogsAsync(2, 3, token);
                case "analytics": return await client.GetServerAnalyticsAsync(teamId: " team/1 ", cancellationToken: token);
                case "busy": return await client.GetServerBusyAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
