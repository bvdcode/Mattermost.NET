using Mattermost.Enums;
using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class UserReportTests
    {
        [Test]
        public async Task InvalidPageSize_RejectsBeforeHttp([Values(-1, 0, 101, int.MaxValue)] int size)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserReportsAsync(pageSize: size));
        }

        [TestCase("user1", null)]
        [TestCase(null, "value")]
        [TestCase("user1", "")]
        [TestCase(" ", "value")]
        [TestCase("", "value")]
        public async Task IncompleteCursor_RejectsBeforeHttp(string? id, string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<ArgumentException>(() => client.GetUserReportsAsync(fromId: id, fromColumnValue: value));
        }

        [Test]
        public async Task ContradictoryActivityFilters_RejectBeforeHttp([Values(false, true)] bool count)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => count
                ? client.GetUserReportCountAsync(hideActive: true, hideInactive: true)
                : client.GetUserReportsAsync(hideActive: true, hideInactive: true));
        }

        [Test]
        public async Task BlankTeam_RejectsBeforeHttp([Values(false, true)] bool count, [Values("", " ")] string team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<ArgumentException>(() => count
                ? client.GetUserReportCountAsync(teamId: team)
                : client.GetUserReportsAsync(teamId: team));
        }

        [Test]
        public async Task InvalidEnums_RejectBeforeHttp([Values("sort", "date", "guest", "count-guest")] string field,
            [Values(-1, int.MaxValue)] int value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Task InvokeAsync()
            {
                switch (field)
                {
                    case "sort": return client.GetUserReportsAsync(sortColumn: (UserReportSortColumn)value);
                    case "date": return client.GetUserReportsAsync(dateRange: (UserReportDateRange)value);
                    case "guest": return client.GetUserReportsAsync(guestFilter: (UserReportGuestFilter)value);
                    case "count-guest": return client.GetUserReportCountAsync(guestFilter: (UserReportGuestFilter)value);
                    default: throw new ArgumentOutOfRangeException(nameof(field));
                }
            }
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(InvokeAsync);
        }

        [Test]
        public async Task Count_RejectsMalformedValues([Values("null", "{}", "[]", "\"1\"", "1.5", "9223372036854775808", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetUserReportCountAsync());
        }

        [Test]
        public async Task List_RejectsMalformedValues([Values("null", "{}", "1", "invalid JSON", "[{\"days_active\":1.5}]")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetUserReportsAsync());
        }

        [Test]
        public async Task NativeErrors_ArePreserved([Values(false, true)] bool count,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"report unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeReportAsync(client, count));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("report unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool count)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeReportAsync(client, count, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeReportAsync(client, count));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool count)
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
            Task pending = InvokeReportAsync(client, count, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeReportAsync(MattermostClient client, bool count, CancellationToken token = default)
        {
            if (count) { return client.GetUserReportCountAsync(cancellationToken: token); }
            return client.GetUserReportsAsync(cancellationToken: token);
        }
    }
}
