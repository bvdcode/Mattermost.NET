using Mattermost.Enums;
using Mattermost.Models.Jobs;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class JobManagementTests
    {
        [Test]
        public async Task Create_SelectsOnlyNativeWritableFields([Values(false, true)] bool hasData)
        {
            Dictionary<string, string>? data = hasData ? new Dictionary<string, string> { ["source_file"] = " archive.zip ", ["empty"] = "" } : null;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/jobs"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "type", "data" }));
                Assert.That(body.RootElement.GetProperty("type").GetString(), Is.EqualTo("import_process"));
                JsonElement values = body.RootElement.GetProperty("data");
                if (hasData)
                {
                    Assert.That(values.GetProperty("source_file").GetString(), Is.EqualTo(" archive.zip "));
                    Assert.That(values.GetProperty("empty").GetString(), Is.Empty);
                }
                else { Assert.That(values.ValueKind, Is.EqualTo(JsonValueKind.Null)); }
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"id\":\"job1\",\"type\":\"import_process\",\"status\":\"pending\",\"create_at\":4294967297,\"data\":{\"native\":\"value\"}}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Job result = await client.CreateJobAsync(" import_process ", data);
            Assert.That(result.Id, Is.EqualTo("job1"));
            Assert.That(result.Type, Is.EqualTo("import_process"));
            Assert.That(result.Status, Is.EqualTo("pending"));
            Assert.That(result.CreatedAt, Is.EqualTo(4294967297L));
            Assert.That(result.Data!["native"], Is.EqualTo("value"));
        }

        [Test]
        public async Task Create_CopiesDataBeforeAuthentication()
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("data").GetProperty(" untouched key ").GetString(), Is.EqualTo(" original value "));
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"id\":\"job1\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Dictionary<string, string> data = new Dictionary<string, string> { [" untouched key "] = " original value " };
            Task<Job> pending = client.CreateJobAsync("active_users", data);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            data.Clear();
            release.SetResult();
            Assert.That((await pending.WaitAsync(TimeSpan.FromSeconds(5))).Id, Is.EqualTo("job1"));
        }

        [Test]
        public async Task Cancel_UsesBodylessNativePost()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/jobs/job%2F1/cancel"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CancelJobAsync(" job/1 ");
        }

        [TestCase(JobStatusUpdate.Pending, "pending", false)]
        [TestCase(JobStatusUpdate.Pending, "pending", true)]
        [TestCase(JobStatusUpdate.CancelRequested, "cancel_requested", false)]
        [TestCase(JobStatusUpdate.CancelRequested, "cancel_requested", true)]
        [TestCase(JobStatusUpdate.Canceled, "canceled", false)]
        [TestCase(JobStatusUpdate.Canceled, "canceled", true)]
        public async Task Status_UsesNativePatchAndExplicitForce(JobStatusUpdate status, string expected, bool force)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Patch));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/jobs/job%2F1/status"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "status", "force" }));
                Assert.That(body.RootElement.GetProperty("status").GetString(), Is.EqualTo(expected));
                Assert.That(body.RootElement.GetProperty("force").GetBoolean(), Is.EqualTo(force));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateJobStatusAsync(" job/1 ", status, force);
        }
    }
}
