using Mattermost.Enums;
using Mattermost.Models.Channels;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class ChannelReadExpansionTests
    {
        [Test]
        public async Task GetChannelMembersByIds_SendsArrayAndEnumeratesOnce()
        {
            int enumerations = 0;
            int requests = 0;
            IEnumerable<string> Ids()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " user-1 ";
                yield return "user-2";
            }
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/members/ids"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                string[]? ids = JsonSerializer.Deserialize<string[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(ids, Is.EqualTo(new[] { "user-1", "user-2" }));
                return JsonResponse(HttpStatusCode.OK, "[" + MemberJson() + "]");
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;
            IList<ChannelUserInfo> members = await api.GetChannelMembersByIdsAsync(" channel/1?# ", Ids());
            Assert.That(requests, Is.EqualTo(1));
            AssertMember(members.Single());
        }

        [Test]
        public async Task GetChannelStats_EscapesChannelAndPreservesInt64Counts(
            [Values(false, true)] bool excludeFilesCount, [Values(0L, 2147483648L, long.MaxValue)] long count)
        {
            long guests = Math.Max(0, count - 1);
            long pins = Math.Max(0, count - 2);
            long files = Math.Max(0, count - 3);
            if (excludeFilesCount)
            {
                files = -1;
            }
            string body = JsonSerializer.Serialize(new
            {
                channel_id = "channel-1", member_count = count, guest_count = guests,
                pinnedpost_count = pins, files_count = files
            });
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                string expected = "https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/stats";
                if (excludeFilesCount)
                {
                    expected += "?exclude_files_count=true";
                }
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo(expected));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, body));
            });
            using MattermostClient client = CreateClient(http);
            ChannelStats stats = await client.GetChannelStatsAsync(" channel/1?# ", excludeFilesCount);
            Assert.That(stats.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(stats.MemberCount, Is.EqualTo(count));
            Assert.That(stats.GuestCount, Is.EqualTo(guests));
            Assert.That(stats.PinnedPostCount, Is.EqualTo(pins));
            Assert.That(stats.FilesCount, Is.EqualTo(files));
        }

        [TestCase("team-1")]
        [TestCase("")]
        public async Task GetChannelUnread_EscapesIdentifiersAndPreservesAllCounts(string teamId)
        {
            string body = JsonSerializer.Serialize(new
            {
                team_id = teamId, channel_id = "channel-1", msg_count = long.MaxValue,
                mention_count = 2147483648L, mention_count_root = 2L, urgent_mention_count = 3L, msg_count_root = 4L
            });
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/channels/channel%2F1%3F%23/unread"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, body));
            });
            using MattermostClient client = CreateClient(http);
            ChannelUnread unread = await client.GetChannelUnreadAsync(" channel/1?# ", " user/1?# ");
            Assert.That(unread.TeamId, Is.EqualTo(teamId));
            Assert.That(unread.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(unread.MessageCount, Is.EqualTo(long.MaxValue));
            Assert.That(unread.MentionCount, Is.EqualTo(2147483648L));
            Assert.That(unread.RootMentionCount, Is.EqualTo(2));
            Assert.That(unread.UrgentMentionCount, Is.EqualTo(3));
            Assert.That(unread.RootMessageCount, Is.EqualTo(4));
        }

        [TestCase(false, 0L)]
        [TestCase(true, 0L)]
        [TestCase(true, 1790000000000L)]
        [TestCase(false, long.MaxValue)]
        [SetCulture("ar-SA")]
        public async Task GetUserTeamChannels_EscapesIdentifiersAndSendsFilters(bool includeDeleted, long lastDeleteAt)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                string expected = FormattableString.Invariant($"https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/teams/team%2F1%3F%23/channels?include_deleted={includeDeleted.ToString().ToLowerInvariant()}&last_delete_at={lastDeleteAt}");
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo(expected));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK,
                    "[{\"id\":\"channel-1\",\"team_id\":\"team-1\",\"type\":\"P\",\"display_name\":\"Private\",\"delete_at\":1790000000000}]"));
            });
            using MattermostClient client = CreateClient(http);
            IList<Channel> channels = await client.GetUserTeamChannelsAsync(" user/1?# ", " team/1?# ", includeDeleted, lastDeleteAt);
            Channel channel = channels.Single();
            Assert.That(channel.Id, Is.EqualTo("channel-1"));
            Assert.That(channel.TeamId, Is.EqualTo("team-1"));
            Assert.That(channel.ChannelType, Is.EqualTo(ChannelType.Private));
            Assert.That(channel.DisplayName, Is.EqualTo("Private"));
            Assert.That(channel.DeletedAt, Is.EqualTo(1790000000000L));
        }

        [Test]
        public async Task GetUserTeamChannelMembers_EscapesIdentifiersAndReadsMemberships()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/teams/team%2F1%3F%23/channels/members"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + MemberJson() + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IList<ChannelUserInfo> memberships = await client.GetUserTeamChannelMembersAsync(" user/1?# ", " team/1?# ");
            AssertMember(memberships.Single());
        }

        [Test]
        public async Task GetChannelTimezones_ReturnsNamesWithoutConvertingThem()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/timezones"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[\"Pacific/Honolulu\",\"Europe/Berlin\"]"));
            });
            using MattermostClient client = CreateClient(http);
            Assert.That(await client.GetChannelTimezonesAsync(" channel/1?# "), Is.EqualTo(new[] { "Pacific/Honolulu", "Europe/Berlin" }));
        }

        [TestCase("unread", "/users/me/channels/channel-1/unread", "{\"channel_id\":\"channel-1\"}")]
        [TestCase("channels", "/users/me/teams/team-1/channels?include_deleted=false&last_delete_at=0", "[]")]
        [TestCase("memberships", "/users/me/teams/team-1/channels/members", "[]")]
        public async Task UserReads_SupportMeAndDefaults(string operation, string path, string body)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                AssertReadRequest(request);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, body));
            });
            using MattermostClient client = CreateClient(http);
            await InvokeAsync(client, operation);
        }

        [TestCase("batch", "[]")]
        [TestCase("channels", "[]")]
        [TestCase("memberships", "[]")]
        [TestCase("timezones", "[]")]
        [TestCase("timezones", "null")]
        public async Task EmptyCollections_RemainEmpty(string operation, string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            switch (operation)
            {
                case "batch": Assert.That(await client.GetChannelMembersByIdsAsync("channel-1", new[] { "user-1" }), Is.Empty); break;
                case "channels": Assert.That(await client.GetUserTeamChannelsAsync("me", "team-1"), Is.Empty); break;
                case "memberships": Assert.That(await client.GetUserTeamChannelMembersAsync("me", "team-1"), Is.Empty); break;
                case "timezones": Assert.That(await client.GetChannelTimezonesAsync("channel-1"), Is.Empty); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static string MemberJson() => "{\"channel_id\":\"channel-1\",\"user_id\":\"user-1\",\"roles\":\"channel_user channel_admin\",\"last_viewed_at\":1790000000000,\"msg_count\":2147483648,\"mention_count\":5,\"last_update_at\":-1}";

        private static void AssertMember(ChannelUserInfo member)
        {
            Assert.That(member.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(member.UserId, Is.EqualTo("user-1"));
            Assert.That(member.Roles, Is.EqualTo("channel_user channel_admin"));
            Assert.That(member.LastViewedAt, Is.EqualTo(1790000000000L));
            Assert.That(member.MessageCount, Is.EqualTo(2147483648L));
            Assert.That(member.MentionCount, Is.EqualTo(5));
            Assert.That(member.UpdatedAt, Is.EqualTo(-1));
        }

        private static MattermostClient CreateClient(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);
        private static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        private static void AssertReadRequest(HttpRequestMessage request)
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Content, Is.Null);
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
