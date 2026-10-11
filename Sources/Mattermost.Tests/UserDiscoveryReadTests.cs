using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using Mattermost.Models.Users;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserDiscoveryReadTests
    {
        [TestCase("known", "/users/known")]
        [TestCase("autocomplete", "/users/autocomplete?name=alice&limit=100")]
        [TestCase("count", "/users/stats")]
        [TestCase("groups", "/users/group_channels")]
        [TestCase("members", "/users/user%2F1%3F%23/channel_members?page=0&per_page=60")]
        [TestCase("sessions", "/users/user%2F1%3F%23/sessions")]
        [TestCase("audits", "/users/user%2F1%3F%23/audits?page=0&per_page=60")]
        public async Task Requests_UseNativeRoutesAndAuthorization(string operation, string route)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                switch (operation)
                {
                    case "groups":
                        Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                        Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                        Assert.That(await request.Content.ReadAsStringAsync(token), Is.EqualTo("[\"group-1\",\"group-2\"]"));
                        break;
                    default:
                        Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                        Assert.That(request.Content, Is.Null);
                        break;
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Autocomplete_EncodesNativeFiltersAndPreservesResultGroups()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?name=a%26b%2B%E2%9C%A8&limit=17&in_team=team%2F1&in_channel=channel%3F1"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                    "{\"users\":[{\"id\":\"in\"}],\"out_of_channel\":[{\"id\":\"out\"}],\"agents\":[{\"id\":\"agent\"}]}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserAutocomplete result = await client.AutocompleteUsersAsync(" a&b+✨ ", " team/1 ", " channel?1 ", 17);
            Assert.That(result.Users!.Single().Id, Is.EqualTo("in"));
            Assert.That(result.OutOfChannel!.Single().Id, Is.EqualTo("out"));
            Assert.That(result.Agents!.Single().Id, Is.EqualTo("agent"));
        }

        [Test]
        public async Task Autocomplete_EmptyNameAndOptionalListsAreSupported()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?name=&limit=100"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"users\":[]}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserAutocomplete result = await client.AutocompleteUsersAsync("");
            Assert.That(result.Users, Is.Empty);
            Assert.That(result.OutOfChannel, Is.Null);
            Assert.That(result.Agents, Is.Null);
        }

        [Test]
        public async Task GroupLookup_EnumeratesOnceAndKeepsDictionaryKeys()
        {
            int enumerations = 0;
            IEnumerable<string> Ids()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " group-1 ";
            }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("[\"group-1\"]"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"group-1\":[{\"id\":\"member-1\"}]}" );
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IDictionary<string, IList<User>> result = await client.GetUsersByGroupChannelIdsAsync(Ids());
            Assert.That(result.Keys, Is.EqualTo(new[] { "group-1" }));
            Assert.That(result["group-1"].Single().Id, Is.EqualTo("member-1"));
        }

        [Test]
        public async Task Models_ReadNativeTimestampsAndInheritedMembership()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                string json;
                switch (request.RequestUri!.AbsolutePath.Split('/').Last())
                {
                    case "sessions":
                        json = "[{\"id\":\"session\",\"create_at\":1790000000123,\"expires_at\":1790009999123,\"last_activity_at\":1790001000123,\"user_id\":\"user\",\"device_id\":\"device\",\"voip_device_id\":\"voice\",\"roles\":\"system_user\",\"is_oauth\":true,\"expired_notify\":true,\"local\":true,\"props\":{\"platform\":\"web\"},\"team_members\":[{\"team_id\":\"team\",\"user_id\":\"user\"}]}]";
                        break;
                    case "audits":
                        json = "[{\"id\":\"audit\",\"create_at\":1790000000123,\"user_id\":\"user\",\"action\":\"login\",\"extra_info\":\"web\",\"ip_address\":\"192.0.2.1\",\"session_id\":\"session\"}]";
                        break;
                    case "channel_members":
                        json = "[{\"channel_id\":\"channel\",\"user_id\":\"user\",\"roles\":\"channel_user\",\"msg_count\":4200000000,\"mention_count\":10,\"last_viewed_at\":1790000000123,\"last_update_at\":1790000000456,\"notify_props\":{\"desktop\":\"mention\"},\"team_display_name\":\"Team ✨\",\"team_name\":\"team\",\"team_update_at\":1790000000789,\"mention_count_root\":3,\"msg_count_root\":8,\"urgent_mention_count\":2,\"scheme_user\":true,\"scheme_admin\":true,\"scheme_guest\":false,\"explicit_roles\":\"custom_role\",\"autotranslation_disabled\":true}]";
                        break;
                    default: throw new AssertionException("Unexpected route.");
                }
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserSession session = (await client.GetUserSessionsAsync("me")).Single();
            Assert.That(session.CreatedAt, Is.EqualTo(1790000000123L));
            Assert.That(session.ExpiresAt, Is.EqualTo(1790009999123L));
            Assert.That(session.LastActivityAt, Is.EqualTo(1790001000123L));
            Assert.That(session.VoipDeviceId, Is.EqualTo("voice"));
            Assert.That(session.IsOAuth && session.ExpiredNotify && session.Local, Is.True);
            Assert.That(session.Props!["platform"], Is.EqualTo("web"));
            Assert.That(session.TeamMembers!.Single().TeamId, Is.EqualTo("team"));
            UserAudit audit = (await client.GetUserAuditsAsync("me")).Single();
            Assert.That(audit.CreatedAt, Is.EqualTo(1790000000123L));
            Assert.That(audit.Action, Is.EqualTo("login"));
            Assert.That(audit.ExtraInfo, Is.EqualTo("web"));
            Assert.That(audit.IpAddress, Is.EqualTo("192.0.2.1"));
            Assert.That(audit.SessionId, Is.EqualTo(session.Id));
            ChannelMemberWithTeamData member = (await client.GetUserChannelMembersAsync("me")).Single();
            Assert.That(member.ChannelId, Is.EqualTo("channel"));
            Assert.That(member.MessageCount, Is.EqualTo(4200000000L));
            Assert.That(member.LastViewedAt, Is.EqualTo(1790000000123L));
            Assert.That(member.UpdatedAt, Is.EqualTo(1790000000456L));
            Assert.That(member.NotifyProps.Desktop, Is.EqualTo("mention"));
            Assert.That(member.TeamDisplayName, Is.EqualTo("Team ✨"));
            Assert.That(member.TeamUpdatedAt, Is.EqualTo(1790000000789L));
            Assert.That(member.RootMessageCount, Is.EqualTo(8));
            Assert.That(member.RootMentionCount, Is.EqualTo(3));
            Assert.That(member.UrgentMentionCount, Is.EqualTo(2));
            Assert.That(member.SchemeUser && member.SchemeAdmin && member.AutoTranslationDisabled, Is.True);
            Assert.That(member.ExplicitRoles, Is.EqualTo("custom_role"));
        }

        [TestCase(0L)]
        [TestCase(4200000000123L)]
        public async Task Count_PreservesNativeInteger(long count)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "{\"total_users_count\":" + count.ToString(CultureInfo.InvariantCulture) + "}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetTotalUserCountAsync(), Is.EqualTo(count));
        }

        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("{\"total_users_count\":null}")]
        [TestCase("{\"total_users_count\":\"1\"}")]
        [TestCase("{\"total_users_count\":0.5}")]
        [TestCase("{\"total_users_count\":9223372036854775808}")]
        public async Task Count_RejectsInvalidShapes(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetTotalUserCountAsync());
        }

        [Test]
        public async Task ListReads_NormalizeNativeNull([Values("known", "members", "sessions", "audits")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "known": Assert.That(await client.GetKnownUserIdsAsync(), Is.Empty); break;
                case "members": Assert.That(await client.GetUserChannelMembersAsync("me"), Is.Empty); break;
                case "sessions": Assert.That(await client.GetUserSessionsAsync("me"), Is.Empty); break;
                case "audits": Assert.That(await client.GetUserAuditsAsync("me"), Is.Empty); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task Errors_PreserveDetails([Values("known", "autocomplete", "count", "groups", "members", "sessions", "audits")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedJson_IsRejected([Values("known", "autocomplete", "count", "groups", "members", "sessions", "audits")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("known", "autocomplete", "count", "groups", "members", "sessions", "audits")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp([Values("known", "autocomplete", "count", "groups", "members", "sessions", "audits")] string operation)
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

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "known": return "[]";
                case "autocomplete": return "{\"users\":[]}";
                case "count": return "{\"total_users_count\":1}";
                case "groups": return "{}";
                case "members": case "sessions": case "audits": return "[]";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "known": return client.GetKnownUserIdsAsync(token);
                case "autocomplete": return client.AutocompleteUsersAsync("alice", cancellationToken: token);
                case "count": return client.GetTotalUserCountAsync(token);
                case "groups": return client.GetUsersByGroupChannelIdsAsync(new[] { " group-1 ", "group-2" }, token);
                case "members": return client.GetUserChannelMembersAsync(" user/1?# ", cancellationToken: token);
                case "sessions": return client.GetUserSessionsAsync(" user/1?# ", token);
                case "audits": return client.GetUserAuditsAsync(" user/1?# ", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
