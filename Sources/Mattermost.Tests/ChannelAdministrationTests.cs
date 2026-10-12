using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelAdministrationTests
    {
        [Test]
        public async Task List_RequestsNativeCountedPage([Values(false, true)] bool deleted, [Values(false, true)] bool excludeDefault)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels?page=2&per_page=200&include_total_count=true&include_deleted="
                    + deleted.ToString().ToLowerInvariant() + "&exclude_default_channels=" + excludeDefault.ToString().ToLowerInvariant()));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, NativePage));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            AssertPage(await client.GetAllChannelsAsync(2, 200, deleted, excludeDefault));
        }

        [Test]
        public async Task Search_UsesAdministrativeJsonPaging([Values(null, ChannelType.Public, ChannelType.Private)] ChannelType? type,
            [Values(false, true)] bool deleted, [Values(false, true)] bool excludeDefault)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/search?system_console=true"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement root = body.RootElement;
                Assert.That(root.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[]
                {
                    "term", "page", "per_page", "team_ids", "public", "private", "include_deleted", "exclude_default_channels"
                }));
                Assert.That(root.GetProperty("term").GetString(), Is.EqualTo("Project & test"));
                Assert.That(root.GetProperty("page").GetInt32(), Is.EqualTo(2));
                Assert.That(root.GetProperty("per_page").GetInt32(), Is.EqualTo(200));
                Assert.That(root.GetProperty("team_ids").EnumerateArray().Select(id => id.GetString()), Is.EqualTo(new[] { "team/1", "team-2" }));
                Assert.That(root.GetProperty("public").GetBoolean(), Is.EqualTo(type == ChannelType.Public));
                Assert.That(root.GetProperty("private").GetBoolean(), Is.EqualTo(type == ChannelType.Private));
                Assert.That(root.GetProperty("include_deleted").GetBoolean(), Is.EqualTo(deleted));
                Assert.That(root.GetProperty("exclude_default_channels").GetBoolean(), Is.EqualTo(excludeDefault));
                return ApiTestHttp.Json(HttpStatusCode.OK, NativePage);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            AssertPage(await client.SearchAllChannelsAsync(" Project & test ", 2, 200, new[] { " team/1 ", "team-2", "team/1" }, type, deleted, excludeDefault));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Reads_UseDefaultPageAndAcceptEmptyResults(bool search)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                if (search)
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    Assert.That(body.RootElement.GetProperty("term").GetString(), Is.Empty);
                    Assert.That(body.RootElement.GetProperty("page").GetInt32(), Is.Zero);
                    Assert.That(body.RootElement.GetProperty("per_page").GetInt32(), Is.EqualTo(60));
                    Assert.That(body.RootElement.GetProperty("team_ids").ValueKind, Is.EqualTo(JsonValueKind.Null));
                }
                else { Assert.That(request.RequestUri!.Query, Does.StartWith("?page=0&per_page=60&include_total_count=true")); }
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"channels\":[],\"total_count\":0}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelListResponse result = await ReadAsync(client, search);
            Assert.That(result.Channels, Is.Empty);
            Assert.That(result.TotalCount, Is.Zero);
        }

        [Test]
        public async Task Paging_IsValidated([Values(false, true)] bool search, [Values(-1, 0, 1)] int page,
            [Values(-1, 0, 201)] int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (search) { await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchAllChannelsAsync("", page, perPage)); }
            else { await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetAllChannelsAsync(page, perPage)); }
        }

        [TestCase(ChannelType.Direct)]
        [TestCase(ChannelType.Group)]
        [TestCase((ChannelType)999)]
        public async Task Search_RejectsUnsupportedChannelType(ChannelType type)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SearchAllChannelsAsync("", channelType: type));
        }

        [Test]
        public async Task Search_RejectsNullTerm()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SearchAllChannelsAsync(null!));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Search_RejectsBlankTeamIdentifier(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SearchAllChannelsAsync("", teamIds: new[] { id! }));
        }

        [Test]
        public async Task Search_EmptyTeamListIsNotAFilter()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("team_ids").GetArrayLength(), Is.Zero);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"channels\":[],\"total_count\":0}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SearchAllChannelsAsync("", teamIds: Array.Empty<string>());
        }

        [Test]
        public void Model_SerializesTeamMetadataWithoutChangingBaseChannel()
        {
            ChannelWithTeamData channel = new ChannelWithTeamData
            {
                Id = "channel-1", TeamId = "team-1", TeamName = "team-url", TeamDisplayName = "Team display", TeamUpdatedAt = 4294967297L
            };
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(channel));
            Assert.That(encoded.RootElement.GetProperty("team_name").GetString(), Is.EqualTo("team-url"));
            Assert.That(encoded.RootElement.GetProperty("team_display_name").GetString(), Is.EqualTo("Team display"));
            Assert.That(encoded.RootElement.GetProperty("team_update_at").GetInt64(), Is.EqualTo(4294967297L));
            using JsonDocument original = JsonDocument.Parse(JsonSerializer.Serialize(new Channel { Id = "channel-1" }));
            Assert.That(original.RootElement.TryGetProperty("team_name", out _), Is.False);
        }

        [Test]
        public async Task ServerErrors_ArePreserved([Values(false, true)] bool search,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"discovery unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, search));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("discovery unavailable"));
        }

        [Test]
        public async Task InvalidResponses_Throw([Values(false, true)] bool search,
            [Values("null", "[]", "invalid JSON", "{\"channels\":{},\"total_count\":0}")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, search));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool search)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => ReadAsync(client, search, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ReadAsync(client, search));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool search)
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
            Task pending = ReadAsync(client, search, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private const string NativePage = "{\"channels\":[{\"id\":\"channel-1\",\"team_id\":\"team-1\",\"type\":\"P\",\"name\":\"project\","
            + "\"team_name\":\"team-url\",\"team_display_name\":\"Team display\",\"team_update_at\":4294967297}],\"total_count\":4294967298}";

        private static void AssertPage(ChannelListResponse result)
        {
            Assert.That(result.TotalCount, Is.EqualTo(4294967298L));
            Assert.That(result.Channels, Has.Count.EqualTo(1));
            Assert.That(result.Channels[0].Id, Is.EqualTo("channel-1"));
            Assert.That(result.Channels[0].Type, Is.EqualTo("P"));
            Assert.That(result.Channels[0].Name, Is.EqualTo("project"));
            Assert.That(result.Channels[0].TeamId, Is.EqualTo("team-1"));
            Assert.That(result.Channels[0].TeamName, Is.EqualTo("team-url"));
            Assert.That(result.Channels[0].TeamDisplayName, Is.EqualTo("Team display"));
            Assert.That(result.Channels[0].TeamUpdatedAt, Is.EqualTo(4294967297L));
        }

        private static Task<ChannelListResponse> ReadAsync(MattermostClient client, bool search, CancellationToken token = default)
        {
            if (search) { return client.SearchAllChannelsAsync("", cancellationToken: token); }
            return client.GetAllChannelsAsync(cancellationToken: token);
        }
    }
}
