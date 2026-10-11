using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class SidebarCategoryTests
    {
        private const string CategoryJson = "{\"id\":\"cat/1?#\",\"user_id\":\"user/1?#\",\"team_id\":\"team/1?#\",\"display_name\":\"  Tests ✨  \",\"type\":\"custom\",\"sorting\":\"alpha\",\"sort_order\":4200000000,\"muted\":true,\"collapsed\":true,\"channel_ids\":[\"channel-2\",\"channel-1\"]}";

        [TestCase("list", "GET", "", false)]
        [TestCase("get", "GET", "/cat%2F1%3F%23", false)]
        [TestCase("order", "GET", "/order", false)]
        [TestCase("create", "POST", "", true)]
        [TestCase("update", "PUT", "/cat%2F1%3F%23", true)]
        [TestCase("batch", "PUT", "", true)]
        [TestCase("reorder", "PUT", "/order", true)]
        [TestCase("delete", "DELETE", "/cat%2F1%3F%23", false)]
        public async Task Requests_UseExpectedRoutesAndPayloads(string operation, string method, string suffix, bool hasBody)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23/teams/team%2F1%3F%23/channels/categories" + suffix));
                Assert.That(request.Content is not null, Is.EqualTo(hasBody));
                if (hasBody)
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    JsonElement payload = body.RootElement;
                    if (operation == "reorder")
                    {
                        Assert.That(payload.EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "cat-2", "cat-1" }));
                    }
                    else
                    {
                        if (operation == "batch") { payload = payload[0]; }
                        Assert.That(payload.GetProperty("user_id").GetString(), Is.EqualTo("user/1?#"));
                        Assert.That(payload.GetProperty("team_id").GetString(), Is.EqualTo("team/1?#"));
                        Assert.That(payload.GetProperty("display_name").GetString(), Is.EqualTo("  Tests ✨  "));
                        Assert.That(payload.GetProperty("sort_order").GetInt64(), Is.EqualTo(4200000000));
                        Assert.That(payload.GetProperty("muted").GetBoolean(), Is.True);
                        Assert.That(payload.GetProperty("collapsed").GetBoolean(), Is.True);
                        Assert.That(payload.GetProperty("sorting").GetString(), Is.EqualTo("alpha"));
                        Assert.That(payload.GetProperty("type").GetString(), Is.EqualTo("custom"));
                        Assert.That(payload.GetProperty("channel_ids").EnumerateArray().Select(item => item.GetString()),
                            Is.EqualTo(new[] { "channel-2", "channel-1" }));
                        Assert.That(payload.EnumerateObject().Count(), Is.EqualTo(10));
                    }
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task ListAndSingleReads_DeserializeFullCategoryAndOrder()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                Response(request.RequestUri!.AbsolutePath.EndsWith("categories", StringComparison.Ordinal) ? "list" : "get"))));
            using MattermostClient client = ApiTestHttp.Client(http);
            OrderedSidebarCategories result = await client.GetSidebarCategoriesAsync("user", "team");
            Assert.That(result.Order, Is.EqualTo(new[] { "cat/1?#" }));
            Assert.That(result.Categories.Count, Is.EqualTo(1));
            SidebarCategory category = await client.GetSidebarCategoryAsync("user", "team", "cat");
            Assert.That(category.Id, Is.EqualTo(result.Categories[0].Id));
            Assert.That(category.UserId, Is.EqualTo("user/1?#"));
            Assert.That(category.TeamId, Is.EqualTo("team/1?#"));
            Assert.That(category.DisplayName, Is.EqualTo("  Tests ✨  "));
            Assert.That(category.SortOrder, Is.EqualTo(4200000000));
            Assert.That(category.Muted, Is.True);
            Assert.That(category.Collapsed, Is.True);
            Assert.That(category.ChannelIds, Is.EqualTo(new[] { "channel-2", "channel-1" }));
        }

        [Test]
        public async Task EmptyCategory_PreservesNullChannelList()
        {
            SidebarCategory category = JsonSerializer.Deserialize<SidebarCategory>("{\"channel_ids\":null,\"sorting\":\"\"}")!;
            Assert.That(category.ChannelIds, Is.Null);
            Assert.That(category.Sorting, Is.Empty);
        }

        [Test]
        public async Task Writes_SnapshotInputWithoutMutatingIt()
        {
            SidebarCategory category = Fixture();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("users/me", StringComparison.Ordinal))
                {
                    category.DisplayName = "Changed later";
                    category.ChannelIds!.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("  Tests ✨  "));
                Assert.That(body.RootElement.GetProperty("channel_ids").GetArrayLength(), Is.EqualTo(2));
                return ApiTestHttp.Json(HttpStatusCode.OK, CategoryJson);
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CreateSidebarCategoryAsync(category);
            Assert.That(category.UserId, Is.EqualTo(" user/1?# "));
            Assert.That(category.TeamId, Is.EqualTo(" team/1?# "));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations)
            {
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, userId: id!));
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, teamId: id!));
            }
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetSidebarCategoryAsync("user", "team", id!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteSidebarCategoryAsync("user", "team", id!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoryOrderAsync("user", "team", new[] { id! }));
        }

        [Test]
        public async Task InvalidWrites_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateSidebarCategoryAsync(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateSidebarCategoryAsync(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateSidebarCategoriesAsync("user", "team", null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateSidebarCategoryOrderAsync("user", "team", null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoriesAsync("user", "team", Array.Empty<SidebarCategory>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoriesAsync("user", "team", new SidebarCategory[] { null! }));
            SidebarCategory category = Fixture();
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoriesAsync(category.UserId, category.TeamId, new[] { category, category }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoriesAsync("other", category.TeamId, new[] { category }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoryOrderAsync("user", "team", Array.Empty<string>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoryOrderAsync("user", "team", new[] { "cat", " cat " }));
            category.Id = " ";
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSidebarCategoryAsync(category));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [Values("list", "get", "order", "create", "update", "batch", "reorder", "delete")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected(
            [Values("list", "get", "order", "create", "update", "batch", "reorder", "delete")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("list", "get", "order", "create", "update", "batch", "reorder", "delete")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp(
            [Values("list", "get", "order", "create", "update", "batch", "reorder", "delete")] string operation)
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

        private static readonly string[] Operations = { "list", "get", "order", "create", "update", "batch", "reorder", "delete" };

        private static SidebarCategory Fixture() => new SidebarCategory
        {
            Id = "cat/1?#", UserId = " user/1?# ", TeamId = " team/1?# ", DisplayName = "  Tests ✨  ",
            Type = "custom", Sorting = "alpha", SortOrder = 4200000000, Muted = true, Collapsed = true,
            ChannelIds = new List<string> { "channel-2", "channel-1" }
        };

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "list": return "{\"categories\":[" + CategoryJson + "],\"order\":[\"cat/1?#\"]}";
                case "get": case "create": case "update": return CategoryJson;
                case "batch": return "[" + CategoryJson + "]";
                case "order": case "reorder": return "[\"cat-2\",\"cat-1\"]";
                case "delete": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, string userId = " user/1?# ",
            string teamId = " team/1?# ", CancellationToken token = default)
        {
            SidebarCategory category = Fixture();
            category.UserId = userId;
            category.TeamId = teamId;
            switch (operation)
            {
                case "list": return client.GetSidebarCategoriesAsync(userId, teamId, token);
                case "get": return client.GetSidebarCategoryAsync(userId, teamId, " cat/1?# ", token);
                case "order": return client.GetSidebarCategoryOrderAsync(userId, teamId, token);
                case "create": return client.CreateSidebarCategoryAsync(category, token);
                case "update": return client.UpdateSidebarCategoryAsync(category, token);
                case "batch": return client.UpdateSidebarCategoriesAsync(userId, teamId, new[] { category }, token);
                case "reorder": return client.UpdateSidebarCategoryOrderAsync(userId, teamId, new[] { " cat-2 ", "cat-1" }, token);
                case "delete": return client.DeleteSidebarCategoryAsync(userId, teamId, " cat/1?# ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
