using Mattermost.Exceptions;
using Mattermost.Models.Jobs;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class JobReadTests
    {
        private const string JobJson = "{\"id\":\"job\",\"type\":\"future_type\",\"priority\":5000000001,\"create_at\":1800000000001,\"start_at\":1800000000002,\"last_activity_at\":1800000000003,\"status\":\"future_status\",\"progress\":5000000002,\"data\":{\"team_id\":\"team\",\"message\":\"text ✨\"}}";

        [TestCase("all", "/chat/api/v4/jobs?page=2&per_page=3&job_type=type%2F1&status=status%26value")]
        [TestCase("one", "/chat/api/v4/jobs/job%2F1")]
        [TestCase("type", "/chat/api/v4/jobs/type/type%2F1?page=2&per_page=3&team_id=team%2F1")]
        public async Task Reads_UseNativeRoutesAndCompleteModel(string operation, string route)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                Assert.That(request.Content, Is.Null);
                string json = operation == "one" ? JobJson : "[" + JobJson + "]";
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object result = await ReadAsync(client, operation);
            Job job = operation == "one" ? (Job)result : ((IList<Job>)result).Single();
            Assert.Multiple(() =>
            {
                Assert.That(job.Id, Is.EqualTo("job"));
                Assert.That(job.Type, Is.EqualTo("future_type"));
                Assert.That(job.Priority, Is.EqualTo(5000000001L));
                Assert.That(job.CreatedAt, Is.EqualTo(1800000000001L));
                Assert.That(job.StartedAt, Is.EqualTo(1800000000002L));
                Assert.That(job.LastActivityAt, Is.EqualTo(1800000000003L));
                Assert.That(job.Status, Is.EqualTo("future_status"));
                Assert.That(job.Progress, Is.EqualTo(5000000002L));
                Assert.That(job.Data!["team_id"], Is.EqualTo("team"));
                Assert.That(job.Data["message"], Is.EqualTo("text ✨"));
            });
        }

        [Test]
        public async Task Defaults_OmitFilters()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetJobsAsync();
            await client.GetJobsByTypeAsync("migrations");
        }

        [TestCase(null, "success", "?page=0&per_page=60&status=success")]
        [TestCase("migrations", null, "?page=0&per_page=60&job_type=migrations")]
        public async Task OptionalFilters_AreIndependent(string? type, string? status, string query)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo(query));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetJobsAsync(jobType: type, status: status);
        }

        [Test]
        public async Task Lists_NormalizeNativeNull([Values("all", "type")] string operation, [Values("[]", "null")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await ReadAsync(client, operation), Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task RequiredValues_RejectEmptyBeforeHttp(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetJobAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetJobsByTypeAsync(value!));
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task SuppliedFilters_RejectBlankBeforeHttp(string value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetJobsAsync(jobType: value));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetJobsAsync(status: value));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetJobsByTypeAsync("migrations", teamId: value));
        }

        [TestCase(-1, 1)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        public async Task Lists_RejectInvalidPaging(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetJobsAsync(page, perPage));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetJobsByTypeAsync("migrations", page, perPage));
        }

        [Test]
        public async Task Errors_PreserveNativeDetails([Values("all", "one", "type")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.job.error\",\"message\":\"job unavailable\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
            Assert.That(error.Message, Is.EqualTo("job unavailable"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("all", "one", "type")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "bad json")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task Single_NullObjectIsRejected()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetJobAsync("job"));
        }

        [Test]
        public async Task JobData_PreservesNullAndEmpty([Values("null", "{}")] string data)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "{\"id\":\"job\",\"data\":" + data + "}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Job job = await client.GetJobAsync("job");
            if (data == "null") { Assert.That(job.Data, Is.Null); }
            else { Assert.That(job.Data, Is.Empty); }
            Assert.That(job.StartedAt, Is.Zero);
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("all", "one", "type")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp([Values("all", "one", "type")] string operation)
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
                case "all": return await client.GetJobsAsync(2, 3, " type/1 ", " status&value ", token);
                case "one": return await client.GetJobAsync(" job/1 ", token);
                case "type": return await client.GetJobsByTypeAsync(" type/1 ", 2, 3, " team/1 ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
