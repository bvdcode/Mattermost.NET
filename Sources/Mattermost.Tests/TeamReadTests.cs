using Mattermost.Models.Teams;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class TeamReadTests
    {
        [TestCase(0, 60, false, false, "")]
        [TestCase(2, 15, true, false, "&sort=Username")]
        [TestCase(0, 200, false, true, "&exclude_deleted_users=true")]
        [TestCase(1, 100, true, true, "&sort=Username&exclude_deleted_users=true")]
        [SetCulture("ar-SA")]
        public async Task GetTeamMembers_EscapesTeamAndSendsPagingAndFilters(int page, int perPage,
            bool sortByUsername, bool excludeDeletedUsers, string filters)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                string expected = FormattableString.Invariant($"https://mattermost.example/chat/api/v4/teams/team%2F1%3F%23/members?page={page}&per_page={perPage}") + filters;
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo(expected));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + MemberJson(0) + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;
            IList<TeamMember> members = await api.GetTeamMembersAsync(" team/1?# ", page, perPage, sortByUsername, excludeDeletedUsers);
            AssertMember(members.Single(), 0);
        }

        [TestCase(-1L)]
        [TestCase(0L)]
        [TestCase(1790000000123L)]
        [TestCase(long.MaxValue)]
        public async Task GetTeamMember_EscapesIdentifiersAndPreservesMembership(long deletedAt)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/teams/team%2F1%3F%23/members/user%2F1%3F%23"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, MemberJson(deletedAt)));
            });
            using MattermostClient client = CreateClient(http);
            TeamMember member = await client.GetTeamMemberAsync(" team/1?# ", " user/1?# ");
            AssertMember(member, deletedAt);
        }

        [Test]
        public async Task GetTeamMembersByIds_SendsOneArrayRequest_EnumeratesOnce()
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
                    Is.EqualTo("https://mattermost.example/chat/api/v4/teams/team%2F1%3F%23/members/ids"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                string[]? ids = JsonSerializer.Deserialize<string[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(ids, Is.EqualTo(new[] { "user-1", "user-2" }));
                return JsonResponse(HttpStatusCode.OK, "[" + MemberJson(0) + "]");
            });
            using MattermostClient client = CreateClient(http);
            IList<TeamMember> members = await client.GetTeamMembersByIdsAsync(" team/1?# ", Ids());
            Assert.That(requests, Is.EqualTo(1));
            AssertMember(members.Single(), 0);
        }

        [TestCase("user/1?#", "user%2F1%3F%23")]
        [TestCase("me", "me")]
        public async Task GetUserTeamMembers_EscapesUserAndSupportsMe(string userId, string escaped)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/" + escaped + "/teams/members"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + MemberJson(-1) + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IList<TeamMember> members = await client.GetUserTeamMembersAsync(" " + userId + " ");
            AssertMember(members.Single(), -1);
        }

        [TestCase(0L)]
        [TestCase(2147483648L)]
        [TestCase(long.MaxValue)]
        public async Task GetTeamStats_PreservesInt64Counts(long count)
        {
            long activeCount = Math.Max(0, count - 1);
            string body = JsonSerializer.Serialize(new { team_id = "team-1", total_member_count = count, active_member_count = activeCount });
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/teams/team%2F1%3F%23/stats"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, body));
            });
            using MattermostClient client = CreateClient(http);
            TeamStats stats = await client.GetTeamStatsAsync(" team/1?# ");
            Assert.That(stats.TeamId, Is.EqualTo("team-1"));
            Assert.That(stats.TotalMemberCount, Is.EqualTo(count));
            Assert.That(stats.ActiveMemberCount, Is.EqualTo(activeCount));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TeamExists_UsesNameAndReturnsServerFlag(bool exists)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/teams/name/team%2Fname%3F%23/exists"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new { exists })));
            });
            using MattermostClient client = CreateClient(http);
            Assert.That(await client.TeamExistsAsync(" team/name?# "), Is.EqualTo(exists));
        }

        [Test]
        public async Task GetTeamUnread_EscapesIdentifiersAndReadsAllCounts()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/teams/team%2F1%3F%23/unread"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, UnreadJson()));
            });
            using MattermostClient client = CreateClient(http);
            TeamUnread unread = await client.GetTeamUnreadAsync(" team/1?# ", " user/1?# ");
            AssertUnread(unread);
        }

        [TestCase(null, false, "?include_collapsed_threads=false")]
        [TestCase("", true, "?include_collapsed_threads=true")]
        [TestCase(" \t", false, "?include_collapsed_threads=false")]
        [TestCase(" team/1?# ", false, "?include_collapsed_threads=false&exclude_team=team%2F1%3F%23")]
        [TestCase(" team/1?# ", true, "?include_collapsed_threads=true&exclude_team=team%2F1%3F%23")]
        public async Task GetUserTeamsUnread_SendsOptionalFilters(string? excludeTeamId, bool includeThreads, string query)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/me/teams/unread" + query));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + UnreadJson() + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IList<TeamUnread> unread = await client.GetUserTeamsUnreadAsync(" me ", excludeTeamId, includeThreads);
            AssertUnread(unread.Single());
        }

        [TestCase("members")]
        [TestCase("batch")]
        [TestCase("user-members")]
        [TestCase("user-unread")]
        public async Task EmptyCollections_RemainEmpty(string operation)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]")));
            using MattermostClient client = CreateClient(http);
            switch (operation)
            {
                case "members": Assert.That(await client.GetTeamMembersAsync("team-1"), Is.Empty); break;
                case "batch": Assert.That(await client.GetTeamMembersByIdsAsync("team-1", new[] { "user-1" }), Is.Empty); break;
                case "user-members": Assert.That(await client.GetUserTeamMembersAsync("me"), Is.Empty); break;
                case "user-unread": Assert.That(await client.GetUserTeamsUnreadAsync("me"), Is.Empty); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static string MemberJson(long deletedAt) => JsonSerializer.Serialize(new
        {
            team_id = "team-1", user_id = "user-2", roles = "team_user team_admin", delete_at = deletedAt,
            scheme_guest = false, scheme_user = true, scheme_admin = true, explicit_roles = "custom_role"
        });

        private static void AssertMember(TeamMember member, long deletedAt)
        {
            Assert.That(member.TeamId, Is.EqualTo("team-1"));
            Assert.That(member.UserId, Is.EqualTo("user-2"));
            Assert.That(member.Roles, Is.EqualTo("team_user team_admin"));
            Assert.That(member.DeletedAt, Is.EqualTo(deletedAt));
            Assert.That(member.SchemeGuest, Is.False);
            Assert.That(member.SchemeUser, Is.True);
            Assert.That(member.SchemeAdmin, Is.True);
            Assert.That(member.ExplicitRoles, Is.EqualTo("custom_role"));
        }

        private static string UnreadJson() => "{\"team_id\":\"team-1\",\"msg_count\":2147483648,\"mention_count\":2,\"mention_count_root\":3,\"msg_count_root\":4,\"thread_count\":5,\"thread_mention_count\":6,\"thread_urgent_mention_count\":7}";

        private static void AssertUnread(TeamUnread unread)
        {
            Assert.That(unread.TeamId, Is.EqualTo("team-1"));
            Assert.That(unread.MessageCount, Is.EqualTo(2147483648L));
            Assert.That(unread.MentionCount, Is.EqualTo(2));
            Assert.That(unread.RootMentionCount, Is.EqualTo(3));
            Assert.That(unread.RootMessageCount, Is.EqualTo(4));
            Assert.That(unread.ThreadCount, Is.EqualTo(5));
            Assert.That(unread.ThreadMentionCount, Is.EqualTo(6));
            Assert.That(unread.ThreadUrgentMentionCount, Is.EqualTo(7));
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

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
