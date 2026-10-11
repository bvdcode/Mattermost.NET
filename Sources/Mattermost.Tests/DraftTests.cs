using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class DraftTests
    {
        private const string DraftJson = "{\"user_id\":\"user-1\",\"channel_id\":\"channel-1\",\"root_id\":\"root-1\",\"message\":\"Draft text\",\"create_at\":4200000000123,\"update_at\":4200000000456,\"delete_at\":0,\"type\":\"\",\"props\":{\"custom\":false},\"file_ids\":[\"file-1\"],\"metadata\":{\"files\":[{\"id\":\"file-1\"}]},\"priority\":{\"priority\":\"urgent\",\"requested_ack\":true,\"persistent_notifications\":false}}";

        [TestCase("list", "GET", "/users/me/teams/team%2F1%3F%23/drafts")]
        [TestCase("upsert", "POST", "/drafts")]
        [TestCase("delete-channel", "DELETE", "/users/me/channels/channel%2F1%3F%23/drafts")]
        [TestCase("delete-thread", "DELETE", "/users/me/channels/channel%2F1%3F%23/drafts/root%2F1%3F%23")]
        public async Task Requests_UseCurrentUserAndExactRoutes(string operation, string method, string path)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                if (operation == "upsert")
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1?#"));
                    Assert.That(body.RootElement.GetProperty("root_id").GetString(), Is.EqualTo("root/1?#"));
                    Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo("  Draft ✨  "));
                    Assert.That(body.RootElement.GetProperty("props").GetProperty("custom").GetBoolean(), Is.False);
                    Assert.That(body.RootElement.GetProperty("file_ids")[0].GetString(), Is.EqualTo("file-1"));
                    Assert.That(body.RootElement.GetProperty("priority").GetProperty("persistent_notifications").GetBoolean(), Is.False);
                    Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(7));
                    Assert.That(body.RootElement.TryGetProperty("user_id", out _), Is.False);
                    Assert.That(body.RootElement.TryGetProperty("update_at", out _), Is.False);
                    Assert.That(body.RootElement.TryGetProperty("metadata", out _), Is.False);
                }
                else { Assert.That(request.Content, Is.Null); }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Drafts_DeserializeNativeFieldsAndLargeTimestamps()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[" + DraftJson + "]")));
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<Draft> drafts = await client.GetDraftsAsync("team");
            Draft draft = drafts[0];
            Assert.That(draft.UserId, Is.EqualTo("user-1"));
            Assert.That(draft.RootId, Is.EqualTo("root-1"));
            Assert.That(draft.Text, Is.EqualTo("Draft text"));
            Assert.That(draft.CreatedAt, Is.EqualTo(4200000000123));
            Assert.That(draft.UpdatedAt, Is.EqualTo(4200000000456));
            Assert.That(draft.FileIdentifiers, Is.EqualTo(new[] { "file-1" }));
            Assert.That(((JsonElement)draft.Props!["custom"]).GetBoolean(), Is.False);
            Assert.That(((JsonElement)draft.Priority!["priority"]).GetString(), Is.EqualTo("urgent"));
            Assert.That(((JsonElement)draft.Metadata!["files"])[0].GetProperty("id").GetString(), Is.EqualTo("file-1"));
        }

        [Test]
        public async Task EmptyMessage_DeletesAndReturnsNull()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty("root_id").GetString(), Is.Empty);
                return ApiTestHttp.Json(HttpStatusCode.Created, "null");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Draft? draft = await client.UpsertDraftAsync(new Draft { ChannelId = "channel", Text = string.Empty });
            Assert.That(draft, Is.Null);
        }

        [Test]
        public async Task Upsert_CapturesModelAndCollectionsBeforeAuthentication()
        {
            Draft input = Fixture();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    input.Text = "changed";
                    input.FileIdentifiers!.Clear();
                    input.Props!.Clear();
                    input.Priority!.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo("  Draft ✨  "));
                Assert.That(body.RootElement.GetProperty("file_ids").GetArrayLength(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("props").GetProperty("custom").GetBoolean(), Is.False);
                Assert.That(body.RootElement.GetProperty("priority").GetProperty("priority").GetString(), Is.EqualTo("urgent"));
                return ApiTestHttp.Json(HttpStatusCode.Created, DraftJson);
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpsertDraftAsync(input);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetDraftsAsync(id!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteDraftAsync(id!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpsertDraftAsync(new Draft { ChannelId = id! }));
            if (id is not null) { await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteDraftAsync("channel", id)); }
        }

        [Test]
        public async Task InvalidDrafts_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpsertDraftAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpsertDraftAsync(new Draft { ChannelId = "channel", Text = null! }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpsertDraftAsync(new Draft { ChannelId = "channel", FileIdentifiers = new[] { " " } }));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [Values("list", "upsert", "delete-channel", "delete-thread")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [TestCase("delete-channel")]
        [TestCase("delete-thread")]
        public async Task UnexpectedNullResponses_AreRejected(string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("null")]
        [TestCase("[]")]
        public async Task EmptyDraftList_ReturnsEmptyCollection(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetDraftsAsync("team"), Is.Empty);
        }

        [Test]
        public async Task MalformedResponses_AreRejected([Values("list", "upsert", "delete-channel", "delete-thread")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("list", "upsert", "delete-channel", "delete-thread")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp([Values("list", "upsert", "delete-channel", "delete-thread")] string operation)
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

        private static Draft Fixture() => new Draft
        {
            ChannelId = " channel/1?# ", RootId = " root/1?# ", Text = "  Draft ✨  ", UserId = "not-sent",
            UpdatedAt = 4200000000123, FileIdentifiers = new List<string> { " file-1 " },
            Props = new Dictionary<string, object> { ["custom"] = false },
            Metadata = new Dictionary<string, object> { ["readonly"] = "not-sent" },
            Priority = new Dictionary<string, object> { ["priority"] = "urgent", ["persistent_notifications"] = false }
        };

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "list": return "[" + DraftJson + "]";
                case "upsert": return DraftJson;
                case "delete-channel": case "delete-thread": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetDraftsAsync(" team/1?# ", token);
                case "upsert": return client.UpsertDraftAsync(Fixture(), token);
                case "delete-channel": return client.DeleteDraftAsync(" channel/1?# ", cancellationToken: token);
                case "delete-thread": return client.DeleteDraftAsync(" channel/1?# ", " root/1?# ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
