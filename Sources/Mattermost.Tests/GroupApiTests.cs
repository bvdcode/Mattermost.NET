using Mattermost.Exceptions;
using Mattermost.Models.Groups;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class GroupApiTests
    {
        private static readonly string[] Operations = { "list", "get", "names", "create", "patch", "delete", "restore", "members", "add", "remove", "stats", "user", "channel-groups", "team-groups", "team-channels", "link-team", "link-channel", "unlink-team", "unlink-channel", "get-team", "get-channel", "teams", "channels", "patch-team", "patch-channel" };

        [TestCase("list", "GET", "/groups?page=2&per_page=17", null)]
        [TestCase("get", "GET", "/groups/group%2F1%3F%23?include_member_count=true&include_member_ids=true", null)]
        [TestCase("names", "POST", "/groups/names", "[\"group\",\"other\"]")]
        [TestCase("create", "POST", "/groups", "{\"name\":\"group\",\"display_name\":\"Display\",\"description\":\"Description\",\"source\":\"custom\",\"allow_reference\":true,\"user_ids\":[\"user\"]}")]
        [TestCase("patch", "PUT", "/groups/group%2F1%3F%23/patch", "{\"description\":\"\",\"allow_reference\":false}")]
        [TestCase("delete", "DELETE", "/groups/group%2F1%3F%23", null)]
        [TestCase("restore", "POST", "/groups/group%2F1%3F%23/restore", null)]
        [TestCase("members", "GET", "/groups/group%2F1%3F%23/members?page=2&per_page=17", null)]
        [TestCase("add", "POST", "/groups/group%2F1%3F%23/members", "{\"user_ids\":[\"user\"]}")]
        [TestCase("remove", "DELETE", "/groups/group%2F1%3F%23/members", "{\"user_ids\":[\"user\"]}")]
        [TestCase("stats", "GET", "/groups/group%2F1%3F%23/stats", null)]
        [TestCase("user", "GET", "/users/user%2F1%3F%23/groups", null)]
        [TestCase("channel-groups", "GET", "/channels/channel%2F1%3F%23/groups?page=2&per_page=17&include_member_count=true&filter_allow_reference=false&paginate=false&q=term", null)]
        [TestCase("team-groups", "GET", "/teams/team%2F1%3F%23/groups?page=2&per_page=17&include_member_count=true&filter_allow_reference=false&paginate=false&q=term", null)]
        [TestCase("team-channels", "GET", "/teams/team%2F1%3F%23/groups_by_channels?page=2&per_page=17&include_member_count=true&filter_allow_reference=false&paginate=false&q=term", null)]
        [TestCase("link-team", "POST", "/groups/group%2F1%3F%23/teams/team%2F1%3F%23/link", "{\"auto_add\":false,\"scheme_admin\":true}")]
        [TestCase("link-channel", "POST", "/groups/group%2F1%3F%23/channels/channel%2F1%3F%23/link", "{}")]
        [TestCase("unlink-team", "DELETE", "/groups/group%2F1%3F%23/teams/team%2F1%3F%23/link", null)]
        [TestCase("unlink-channel", "DELETE", "/groups/group%2F1%3F%23/channels/channel%2F1%3F%23/link", null)]
        [TestCase("get-team", "GET", "/groups/group%2F1%3F%23/teams/team%2F1%3F%23", null)]
        [TestCase("get-channel", "GET", "/groups/group%2F1%3F%23/channels/channel%2F1%3F%23", null)]
        [TestCase("teams", "GET", "/groups/group%2F1%3F%23/teams", null)]
        [TestCase("channels", "GET", "/groups/group%2F1%3F%23/channels", null)]
        [TestCase("patch-team", "PUT", "/groups/group%2F1%3F%23/teams/team%2F1%3F%23/patch", "{\"auto_add\":true}")]
        [TestCase("patch-channel", "PUT", "/groups/group%2F1%3F%23/channels/channel%2F1%3F%23/patch", "{\"scheme_admin\":false}")]
        public async Task Requests_UseNativeRoutesAndPayloads(string operation, string method, string route, string? json)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                if (json is null) { Assert.That(request.Content, Is.Null); }
                else { Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(json)); }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IMattermostGroupsClient api = client;
            await InvokeAsync(api, operation);
        }

        [Test]
        public async Task Filters_AreEscapedAndCapturedBeforeAuthentication()
        {
            GroupSearchOptions filters = new GroupSearchOptions
            {
                Query = " + имя?&# ", IncludeMemberCount = true, FilterAllowReference = true, IncludeArchived = true,
                Since = 9007199254740993L, NotAssociatedToTeam = " team/1 ", NotAssociatedToChannel = " channel/1 "
            };
            string route = "/chat/api/v4/groups?page=0&per_page=60&include_member_count=true&filter_allow_reference=true"
                + "&include_archived=true&since=9007199254740993&q=" + Uri.EscapeDataString("+ имя?&#")
                + "&not_associated_to_team=team%2F1&not_associated_to_channel=channel%2F1";
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    filters.Query = "changed"; filters.Since = 0; filters.NotAssociatedToTeam = "changed";
                    return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}"));
                }
                Assert.That(request.RequestUri.PathAndQuery, Is.EqualTo(route));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetGroupsAsync(options: filters), Is.Empty);
        }

        [TestCase("names")]
        [TestCase("create")]
        [TestCase("add")]
        [TestCase("remove")]
        public async Task MutableBatches_AreCopiedBeforeAuthentication(string operation)
        {
            List<string> values = new List<string> { " user " };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    values.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement array = operation == "names" ? body.RootElement : body.RootElement.GetProperty("user_ids");
                Assert.That(array.GetArrayLength(), Is.EqualTo(1));
                Assert.That(array[0].GetString(), Is.EqualTo("user"));
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "names": await client.GetGroupsByNamesAsync(values); break;
                case "create": await client.CreateGroupAsync("group", "Display", values); break;
                case "add": await client.AddGroupMembersAsync("group", values); break;
                case "remove": await client.RemoveGroupMembersAsync("group", values); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [TestCase("names")]
        [TestCase("add")]
        [TestCase("remove")]
        public async Task EmptyBatches_AreSentToNativeEndpoint(string operation)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement array = operation == "names" ? body.RootElement : body.RootElement.GetProperty("user_ids");
                Assert.That(array.GetArrayLength(), Is.EqualTo(0));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "names": Assert.That(await client.GetGroupsByNamesAsync(Array.Empty<string>()), Is.Empty); break;
                case "add": Assert.That(await client.AddGroupMembersAsync("group", Array.Empty<string>()), Is.Empty); break;
                case "remove": Assert.That(await client.RemoveGroupMembersAsync("group", Array.Empty<string>()), Is.Empty); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public void InvalidInputs_AreRejectedBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.GetGroupAsync(" "));
            Assert.Throws<ArgumentException>(() => client.CreateGroupAsync(" ", "Display"));
            Assert.Throws<ArgumentException>(() => client.CreateGroupAsync("name", " "));
            Assert.Throws<ArgumentNullException>(() => client.CreateGroupAsync("name", "Display", description: null!));
            Assert.Throws<ArgumentNullException>(() => client.GetGroupsByNamesAsync(null!));
            Assert.Throws<ArgumentException>(() => client.AddGroupMembersAsync("group", new[] { " " }));
            Assert.Throws<ArgumentNullException>(() => client.RemoveGroupMembersAsync("group", null!));
            Assert.Throws<ArgumentException>(() => client.PatchGroupAsync("group"));
            Assert.Throws<ArgumentException>(() => client.PatchGroupTeamAsync("group", "team"));
            Assert.Throws<ArgumentException>(() => client.PatchGroupChannelAsync("group", "channel"));
            Assert.Throws<ArgumentException>(() => client.GetGroupChannelAsync("group", " "));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetGroupsAsync(page: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetGroupsAsync(options: new GroupSearchOptions { Since = -1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetGroupMembersAsync("group", perPage: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetTeamGroupsAsync("team", page: -1));
        }

        [Test]
        public async Task NativeErrors_ArePreserved([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.group.error\",\"message\":\"group rejected\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
        }

        [Test]
        public async Task MalformedResponses_AreRejected([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
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
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([ValueSource(nameof(Operations))] string operation)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, operation, source.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "list": case "names": case "user": return "[{\"id\":\"group\",\"source\":\"custom\"}]";
                case "get": case "create": case "patch": case "delete": case "restore": return "{\"id\":\"group\",\"source\":\"custom\"}";
                case "members": return "{\"members\":[{\"id\":\"user\"}],\"total_member_count\":1}";
                case "add": case "remove": return "[{\"group_id\":\"group\",\"user_id\":\"user\",\"create_at\":9007199254740993,\"delete_at\":0}]";
                case "stats": return "{\"group_id\":\"group\",\"total_member_count\":10}";
                case "channel-groups": case "team-groups": return "{\"groups\":[{\"id\":\"group\",\"scheme_admin\":false}],\"total_group_count\":1}";
                case "team-channels": return "{\"groups\":{\"channel\":[{\"id\":\"group\",\"scheme_admin\":true}]}}";
                case "unlink-team": case "unlink-channel": return "{\"status\":\"OK\"}";
                case "teams": case "channels": return "[{\"group_id\":\"group\",\"team_id\":\"team\",\"auto_add\":true,\"scheme_admin\":false}]";
                case "link-team": case "get-team": case "patch-team": return "{\"group_id\":\"group\",\"team_id\":\"team\",\"auto_add\":true,\"scheme_admin\":false}";
                case "link-channel": case "get-channel": case "patch-channel": return "{\"group_id\":\"group\",\"channel_id\":\"channel\",\"auto_add\":false,\"scheme_admin\":true}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostGroupsClient client, string operation, CancellationToken token = default)
        {
            const string group = " group/1?# ";
            const string team = " team/1?# ";
            const string channel = " channel/1?# ";
            switch (operation)
            {
                case "list": return client.GetGroupsAsync(2, 17, cancellationToken: token);
                case "get": return client.GetGroupAsync(group, true, true, token);
                case "names": return client.GetGroupsByNamesAsync(new[] { " group ", "other" }, token);
                case "create": return client.CreateGroupAsync(" group ", " Display ", new[] { " user " }, " Description ", token);
                case "patch": return client.PatchGroupAsync(group, description: " ", allowReference: false, cancellationToken: token);
                case "delete": return client.DeleteGroupAsync(group, token);
                case "restore": return client.RestoreGroupAsync(group, token);
                case "members": return client.GetGroupMembersAsync(group, 2, 17, token);
                case "add": return client.AddGroupMembersAsync(group, new[] { " user " }, token);
                case "remove": return client.RemoveGroupMembersAsync(group, new[] { " user " }, token);
                case "stats": return client.GetGroupStatsAsync(group, token);
                case "user": return client.GetUserGroupsAsync(" user/1?# ", token);
                case "channel-groups": return client.GetChannelGroupsAsync(channel, 2, 17, " term ", true, false, false, token);
                case "team-groups": return client.GetTeamGroupsAsync(team, 2, 17, " term ", true, false, false, token);
                case "team-channels": return client.GetTeamChannelGroupsAsync(team, 2, 17, " term ", true, false, false, token);
                case "link-team": return client.LinkGroupToTeamAsync(group, team, false, true, token);
                case "link-channel": return client.LinkGroupToChannelAsync(group, channel, cancellationToken: token);
                case "unlink-team": return client.UnlinkGroupFromTeamAsync(group, team, token);
                case "unlink-channel": return client.UnlinkGroupFromChannelAsync(group, channel, token);
                case "get-team": return client.GetGroupTeamAsync(group, team, token);
                case "get-channel": return client.GetGroupChannelAsync(group, channel, token);
                case "teams": return client.GetGroupTeamsAsync(group, token);
                case "channels": return client.GetGroupChannelsAsync(group, token);
                case "patch-team": return client.PatchGroupTeamAsync(group, team, autoAdd: true, cancellationToken: token);
                case "patch-channel": return client.PatchGroupChannelAsync(group, channel, schemeAdmin: false, cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
