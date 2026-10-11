using Mattermost.Exceptions;
using Mattermost.Models.Webhooks;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class WebhookTests
    {
        private const string IncomingJson = "{\"id\":\"hook-1\",\"user_id\":\"user-1\",\"team_id\":\"team-1\",\"channel_id\":\"channel-1\",\"display_name\":\"Incoming\",\"create_at\":4200000000123,\"update_at\":4200000000456,\"last_used\":4200000000789,\"channel_locked\":true}";
        private const string OutgoingJson = "{\"id\":\"hook-1\",\"token\":\"test-webhook-token\",\"creator_id\":\"user-1\",\"team_id\":\"team-1\",\"channel_id\":\"channel-1\",\"trigger_words\":[\"hello\"],\"trigger_when\":1,\"callback_urls\":[\"https://receiver.example/webhook\"],\"create_at\":4200000000123,\"update_at\":4200000000456,\"delete_at\":4200000000789}";

        [TestCase("incoming-list", "GET", "/incoming?page=0&per_page=60&team_id=team%2F1%3F%23")]
        [TestCase("incoming-get", "GET", "/incoming/hook%2F1%3F%23")]
        [TestCase("incoming-create", "POST", "/incoming")]
        [TestCase("incoming-update", "PUT", "/incoming/hook%2F1%3F%23")]
        [TestCase("incoming-delete", "DELETE", "/incoming/hook%2F1%3F%23")]
        [TestCase("outgoing-list", "GET", "/outgoing?page=0&per_page=60&team_id=team%2F1%3F%23&channel_id=channel%2F1%3F%23")]
        [TestCase("outgoing-get", "GET", "/outgoing/hook%2F1%3F%23")]
        [TestCase("outgoing-create", "POST", "/outgoing")]
        [TestCase("outgoing-update", "PUT", "/outgoing/hook%2F1%3F%23")]
        [TestCase("outgoing-regenerate", "POST", "/outgoing/hook%2F1%3F%23/regen_token")]
        [TestCase("outgoing-delete", "DELETE", "/outgoing/hook%2F1%3F%23")]
        public async Task Requests_UseExactRoutesAndWritableFields(string operation, string method, string path)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/hooks" + path));
                if (operation.EndsWith("-create", StringComparison.Ordinal) || operation.EndsWith("-update", StringComparison.Ordinal))
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1?#"));
                    Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("  Hook ✨  "));
                    Assert.That(body.RootElement.TryGetProperty("create_at", out _), Is.False);
                    Assert.That(body.RootElement.TryGetProperty("update_at", out _), Is.False);
                    if (operation.StartsWith("incoming", StringComparison.Ordinal))
                    {
                        Assert.That(body.RootElement.GetProperty("channel_locked").GetBoolean(), Is.False);
                        Assert.That(body.RootElement.TryGetProperty("team_id", out _), Is.False);
                        Assert.That(body.RootElement.TryGetProperty("last_used", out _), Is.False);
                        Assert.That(body.RootElement.TryGetProperty("user_id", out _), Is.EqualTo(operation == "incoming-create"));
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(7));
                    }
                    else
                    {
                        Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/1?#"));
                        Assert.That(body.RootElement.GetProperty("trigger_when").GetInt32(), Is.EqualTo(1));
                        Assert.That(body.RootElement.GetProperty("callback_urls")[0].GetString(), Is.EqualTo("https://receiver.example/hook?a=1&b=2"));
                        Assert.That(body.RootElement.GetProperty("trigger_words")[0].GetString(), Is.EqualTo("hello"));
                        Assert.That(body.RootElement.TryGetProperty("creator_id", out _), Is.EqualTo(operation == "outgoing-create"));
                        if (operation == "outgoing-update")
                        {
                            Assert.That(body.RootElement.GetProperty("token").GetString(), Is.EqualTo("test-webhook-token"));
                            Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(12));
                        }
                        else
                        {
                            Assert.That(body.RootElement.TryGetProperty("token", out _), Is.False);
                            Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(11));
                        }
                    }
                    if (operation.EndsWith("-update", StringComparison.Ordinal)) { Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("hook/1?#")); }
                    else { Assert.That(body.RootElement.TryGetProperty("id", out _), Is.False); }
                }
                else { Assert.That(request.Content, Is.Null); }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [TestCase(OutgoingWebhookTriggerWhen.FirstWordExactMatch, 0)]
        [TestCase(OutgoingWebhookTriggerWhen.FirstWordStartsWith, 1)]
        public async Task TriggerMode_UsesNativeNumericValues(OutgoingWebhookTriggerWhen mode, int expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("trigger_when").GetInt32(), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.Created, OutgoingJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            OutgoingWebhook hook = OutgoingFixture();
            hook.TriggerWhen = mode;
            await client.CreateOutgoingWebhookAsync(hook);
        }

        [Test]
        public async Task Models_DeserializeFieldsAndLargeTimestamps()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                request.RequestUri!.AbsolutePath.Contains("/incoming/", StringComparison.Ordinal) ? IncomingJson : OutgoingJson)));
            using MattermostClient client = ApiTestHttp.Client(http);
            IncomingWebhook incoming = await client.GetIncomingWebhookAsync("hook");
            Assert.That(incoming.CreatedAt, Is.EqualTo(4200000000123));
            Assert.That(incoming.UpdatedAt, Is.EqualTo(4200000000456));
            Assert.That(incoming.LastUsedAt, Is.EqualTo(4200000000789));
            Assert.That(incoming.ChannelLocked, Is.True);
            OutgoingWebhook outgoing = await client.RegenerateOutgoingWebhookTokenAsync("hook");
            Assert.That(outgoing.Token, Is.EqualTo("test-webhook-token"));
            Assert.That(outgoing.CreatorId, Is.EqualTo("user-1"));
            Assert.That(outgoing.DeletedAt, Is.EqualTo(4200000000789));
            Assert.That(outgoing.TriggerWhen, Is.EqualTo(OutgoingWebhookTriggerWhen.FirstWordStartsWith));
            Assert.That(outgoing.TriggerWords, Is.EqualTo(new[] { "hello" }));
        }

        [TestCase("null")]
        [TestCase("[]")]
        public async Task EmptyLists_ReturnEmptyCollections(string json)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetIncomingWebhooksAsync(), Is.Empty);
            Assert.That(await client.GetOutgoingWebhooksAsync(), Is.Empty);
        }

        [Test]
        public async Task OutgoingWrite_CapturesCollectionsBeforeAuthentication()
        {
            OutgoingWebhook input = OutgoingFixture();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    input.CallbackUrls.Clear();
                    input.TriggerWords!.Clear();
                    input.Token = "changed";
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("callback_urls").GetArrayLength(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("trigger_words").GetArrayLength(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("token").GetString(), Is.EqualTo("test-webhook-token"));
                return ApiTestHttp.Json(HttpStatusCode.OK, OutgoingJson);
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateOutgoingWebhookAsync(input);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations.Where(value => value.EndsWith("-get", StringComparison.Ordinal) || value.EndsWith("-delete", StringComparison.Ordinal) || value == "outgoing-regenerate"))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id: id!));
            }
            IncomingWebhook incoming = IncomingFixture();
            incoming.Id = id!;
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateIncomingWebhookAsync(incoming));
            OutgoingWebhook outgoing = OutgoingFixture();
            outgoing.Id = id!;
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateOutgoingWebhookAsync(outgoing));
        }

        [Test]
        public async Task InvalidWritesAndFilters_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateIncomingWebhookAsync(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateOutgoingWebhookAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateIncomingWebhookAsync(new IncomingWebhook()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateOutgoingWebhookAsync(new OutgoingWebhook()));
            OutgoingWebhook outgoing = OutgoingFixture();
            outgoing.Token = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateOutgoingWebhookAsync(outgoing));
            outgoing.CallbackUrls = new[] { "file:///data" };
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateOutgoingWebhookAsync(outgoing));
            outgoing.CallbackUrls = new[] { "https://receiver.example" };
            outgoing.TriggerWhen = (OutgoingWebhookTriggerWhen)42;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.CreateOutgoingWebhookAsync(outgoing));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetIncomingWebhooksAsync(page: -1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetOutgoingWebhooksAsync(perPage: 0));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetOutgoingWebhooksAsync(channelId: " "));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task UnexpectedNullResponses_AreRejected([ValueSource(nameof(NonListOperations))] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
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
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, token: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([ValueSource(nameof(Operations))] string operation)
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
            Task pending = InvokeAsync(client, operation, token: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static readonly string[] Operations = { "incoming-list", "incoming-get", "incoming-create", "incoming-update", "incoming-delete", "outgoing-list", "outgoing-get", "outgoing-create", "outgoing-update", "outgoing-regenerate", "outgoing-delete" };

        private static IEnumerable<string> NonListOperations => Operations.Where(value => !value.EndsWith("-list", StringComparison.Ordinal));

        private static IncomingWebhook IncomingFixture() => new IncomingWebhook
        {
            Id = " hook/1?# ", ChannelId = " channel/1?# ", DisplayName = "  Hook ✨  ", UserId = "owner-1", TeamId = "not-sent",
            CreatedAt = 4200000000123, LastUsedAt = 4200000000456
        };

        private static OutgoingWebhook OutgoingFixture() => new OutgoingWebhook
        {
            Id = " hook/1?# ", ChannelId = " channel/1?# ", TeamId = " team/1?# ", DisplayName = "  Hook ✨  ",
            Token = "test-webhook-token", CreatorId = "owner-1", CreatedAt = 4200000000123,
            TriggerWords = new List<string> { "hello" }, TriggerWhen = OutgoingWebhookTriggerWhen.FirstWordStartsWith,
            CallbackUrls = new List<string> { " https://receiver.example/hook?a=1&b=2 " }
        };

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "incoming-list": return "[" + IncomingJson + "]";
                case "outgoing-list": return "[" + OutgoingJson + "]";
                case "incoming-get": case "incoming-create": case "incoming-update": return IncomingJson;
                case "outgoing-get": case "outgoing-create": case "outgoing-update": case "outgoing-regenerate": return OutgoingJson;
                case "incoming-delete": case "outgoing-delete": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, string id = " hook/1?# ", CancellationToken token = default)
        {
            switch (operation)
            {
                case "incoming-list": return client.GetIncomingWebhooksAsync(" team/1?# ", cancellationToken: token);
                case "incoming-get": return client.GetIncomingWebhookAsync(id, token);
                case "incoming-create": return client.CreateIncomingWebhookAsync(IncomingFixture(), token);
                case "incoming-update": return client.UpdateIncomingWebhookAsync(IncomingFixture(), token);
                case "incoming-delete": return client.DeleteIncomingWebhookAsync(id, token);
                case "outgoing-list": return client.GetOutgoingWebhooksAsync(" team/1?# ", " channel/1?# ", cancellationToken: token);
                case "outgoing-get": return client.GetOutgoingWebhookAsync(id, token);
                case "outgoing-create": return client.CreateOutgoingWebhookAsync(OutgoingFixture(), token);
                case "outgoing-update": return client.UpdateOutgoingWebhookAsync(OutgoingFixture(), token);
                case "outgoing-regenerate": return client.RegenerateOutgoingWebhookTokenAsync(id, token);
                case "outgoing-delete": return client.DeleteOutgoingWebhookAsync(id, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
