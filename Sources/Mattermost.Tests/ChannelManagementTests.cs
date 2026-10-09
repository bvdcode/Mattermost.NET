using Mattermost.Enums;
using Mattermost.Models.Channels;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class ChannelManagementTests
    {
        private const string ChannelJson = "{\"id\":\"channel-1\",\"name\":\"test\",\"display_name\":\"Test\",\"type\":\"O\",\"header\":\"header\",\"purpose\":\"purpose\"}";

        [Test]
        public async Task UpdateChannel_SendsOnlyEditableMetadataWithMatchingId()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1%3F%23"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(item => item.Name), Is.EquivalentTo(new[] { "id", "name", "display_name", "header", "purpose" }));
                Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("channel/1?#"));
                Assert.That(body.RootElement.GetProperty("name").GetString(), Is.EqualTo("test"));
                Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("Test"));
                Assert.That(body.RootElement.GetProperty("header").GetString(), Is.EqualTo("  header  "));
                Assert.That(body.RootElement.GetProperty("purpose").GetString(), Is.Empty);
                return ApiTestHttp.Json(HttpStatusCode.OK, ChannelJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Channel channel = new Channel { Id = " channel/1?# ", Name = "test", DisplayName = "Test", Header = "  header  ", Purpose = "", Type = "P" };
            Assert.That((await client.UpdateChannelAsync(channel)).Id, Is.EqualTo("channel-1"));
            Assert.That(channel.Id, Is.EqualTo(" channel/1?# "));
        }

        [Test]
        public async Task PatchChannel_OmitsNullAndSendsEmptyValues()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1%3F%23/patch"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(item => item.Name), Is.EquivalentTo(new[] { "header", "purpose" }));
                Assert.That(body.RootElement.GetProperty("header").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty("purpose").GetString(), Is.EqualTo("  purpose  "));
                return ApiTestHttp.Json(HttpStatusCode.OK, ChannelJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That((await client.PatchChannelAsync(" channel/1?# ", header: "", purpose: "  purpose  ")).Name, Is.EqualTo("test"));
        }

        [TestCase(ChannelType.Public, "O")]
        [TestCase(ChannelType.Private, "P")]
        public async Task Privacy_SendsExplicitEnumMapping(ChannelType privacy, string type)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1%3F%23/privacy"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("privacy").GetString(), Is.EqualTo(type));
                return ApiTestHttp.Json(HttpStatusCode.OK, ChannelJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateChannelPrivacyAsync(" channel/1?# ", privacy);
        }

        [Test]
        public async Task Restore_SendsPostWithoutBody()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1%3F%23/restore"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, ChannelJson));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.RestoreChannelAsync(" channel/1?# ");
        }

        [Test]
        [SetCulture("ar-SA")]
        public async Task PagedReads_SendPagingAndReturnChannels([Values(false, true)] bool deleted,
            [Values(0, 1)] int page, [Values(1, 60, 200)] int perPage)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                string kind = "private";
                if (deleted) { kind = "deleted"; }
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery,
                    Is.EqualTo(FormattableString.Invariant($"/chat/api/v4/teams/team%2F1%3F%23/channels/{kind}?page={page}&per_page={perPage}")));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[" + ChannelJson + "]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<Channel> channels;
            if (deleted) { channels = await client.GetDeletedChannelsAsync(" team/1?# ", page, perPage); }
            else { channels = await client.GetPrivateChannelsAsync(" team/1?# ", page, perPage); }
            Assert.That(channels.Single().ChannelType, Is.EqualTo(ChannelType.Public));
        }

        [Test]
        public async Task BulkPublicLookup_TrimsArrayAndEnumeratesOnce()
        {
            int enumerations = 0;
            IEnumerable<string> Ids()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " channel-1 ";
                yield return "channel-2";
            }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/team%2F1%3F%23/channels/ids"));
                string[]? ids = JsonSerializer.Deserialize<string[]>(await request.Content!.ReadAsStringAsync(token));
                Assert.That(ids, Is.EqualTo(new[] { "channel-1", "channel-2" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[" + ChannelJson + "]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That((await client.GetPublicChannelsByIdsAsync(" team/1?# ", Ids())).Single().Id, Is.EqualTo("channel-1"));
        }

        [TestCase(" term ", "term")]
        [TestCase("", "")]
        public async Task TeamSearch_SendsOnlySearchTerm(string input, string expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/team%2F1%3F%23/channels/search"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("term").GetString(), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.SearchTeamChannelsAsync(" team/1?# ", input), Is.Empty);
        }
    }
}
