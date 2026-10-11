using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Teams;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TeamLifecycleTests
    {
        private const string TeamJson = "{\"id\":\"team-1\",\"name\":\"team-name\",\"display_name\":\"Team name\",\"description\":\"Description\",\"type\":\"O\",\"invite_id\":\"invite-2\",\"allow_open_invite\":true,\"update_at\":4200000000123}";

        [TestCase("update", "PUT", "/teams/team%2F1%3F%23", true)]
        [TestCase("delete", "DELETE", "/teams/team%2F1%3F%23?permanent=false", false)]
        [TestCase("restore", "POST", "/teams/team%2F1%3F%23/restore", false)]
        [TestCase("privacy", "PUT", "/teams/team%2F1%3F%23/privacy", true)]
        [TestCase("regenerate", "POST", "/teams/team%2F1%3F%23/regenerate_invite_id", false)]
        [TestCase("invite", "GET", "/teams/invite/team%2F1%3F%23", false)]
        [TestCase("add", "POST", "/teams/team%2F1%3F%23/members/batch", true)]
        [TestCase("remove", "DELETE", "/teams/team%2F1%3F%23/members/user%2F1%3F%23", false)]
        [TestCase("roles", "PUT", "/teams/team%2F1%3F%23/members/user%2F1%3F%23/roles", true)]
        [TestCase("scheme", "PUT", "/teams/team%2F1%3F%23/members/user%2F1%3F%23/schemeRoles", true)]
        public async Task Requests_UseExactRoutesAndPayloads(string operation, string method, string path, bool hasBody)
        {
            using HttpClient http = CreateHttp(operation, async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                Assert.That(request.Content is not null, Is.EqualTo(hasBody));
                if (hasBody)
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    switch (operation)
                    {
                        case "update":
                            Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("team/1?#"));
                            Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("  Team ✨  "));
                            Assert.That(body.RootElement.GetProperty("description").GetString(), Is.EqualTo("Description"));
                            Assert.That(body.RootElement.GetProperty("email").GetString(), Is.EqualTo("team@example.org"));
                            Assert.That(body.RootElement.GetProperty("allow_open_invite").GetBoolean(), Is.False);
                            Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(11));
                            Assert.That(body.RootElement.TryGetProperty("create_at", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("invite_id", out _), Is.False);
                            break;
                        case "privacy":
                            Assert.That(body.RootElement.GetProperty("privacy").GetString(), Is.EqualTo("I"));
                            break;
                        case "add":
                            Assert.That(body.RootElement.GetArrayLength(), Is.EqualTo(2));
                            Assert.That(body.RootElement[0].GetProperty("team_id").GetString(), Is.EqualTo("team/1?#"));
                            Assert.That(body.RootElement[0].GetProperty("user_id").GetString(), Is.EqualTo("user/1?#"));
                            Assert.That(body.RootElement[1].GetProperty("user_id").GetString(), Is.EqualTo("user-2"));
                            Assert.That(body.RootElement[0].EnumerateObject().Count(), Is.EqualTo(2));
                            break;
                        case "roles":
                            Assert.That(body.RootElement.GetProperty("roles").GetString(), Is.EqualTo("team_user team_admin"));
                            break;
                        case "scheme":
                            Assert.That(body.RootElement.GetProperty("scheme_user").GetBoolean(), Is.True);
                            Assert.That(body.RootElement.GetProperty("scheme_admin").GetBoolean(), Is.True);
                            Assert.That(body.RootElement.GetProperty("scheme_guest").GetBoolean(), Is.False);
                            Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                            break;
                        default: throw new ArgumentOutOfRangeException(nameof(operation));
                    }
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [TestCase("true", 4200000000123L)]
        [TestCase("false", 0L)]
        [TestCase("null", 4200000000123L)]
        public async Task Update_PreservesIconAndGroupConstraintFromReadResponse(string groupConstrained, long iconUpdate)
        {
            string json = "{\"id\":\"team\",\"name\":\"team\",\"display_name\":\"Team\",\"type\":\"I\","
                + "\"last_team_icon_update\":" + iconUpdate.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ",\"group_constrained\":" + groupConstrained + "}";
            bool updated = false;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                if (request.Method == HttpMethod.Put)
                {
                    using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                    Assert.That(body.RootElement.GetProperty("description").GetString(), Is.EqualTo("New description"));
                    Assert.That(body.RootElement.GetProperty("last_team_icon_update").GetInt64(), Is.EqualTo(iconUpdate));
                    Assert.That(body.RootElement.GetProperty("group_constrained").GetRawText(), Is.EqualTo(groupConstrained));
                    updated = true;
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, json);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Team team = await client.GetTeamAsync("team");
            team.Description = "New description";
            await client.UpdateTeamAsync(team);
            Assert.That(updated, Is.True);
        }

        [Test]
        public void TeamSerialization_OmitsDefaultIconAndGroupConstraint()
        {
            using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(new Team()));
            Assert.That(json.RootElement.TryGetProperty("last_team_icon_update", out _), Is.False);
            Assert.That(json.RootElement.TryGetProperty("group_constrained", out _), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Deletion_RequiresExplicitPermanentFlag(bool permanent)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?permanent=" + permanent.ToString().ToLowerInvariant()));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response("delete")));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.DeleteTeamAsync("team", permanent);
        }

        [TestCase(TeamType.Open, "O")]
        [TestCase(TeamType.InviteOnly, "I")]
        public async Task Privacy_UsesNativeValues(TeamType type, string expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("privacy").GetString(), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, TeamJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateTeamPrivacyAsync("team", type);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task InviteInfo_DoesNotAuthenticateOrAttachStoredToken(bool hasToken)
        {
            using HttpClient http = CreateHttp("invite", (_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "{\"id\":\"team\",\"name\":\"name\",\"display_name\":\"Display\",\"description\":\"Description\"}")));
            MattermostClient selectedClient;
            if (hasToken) { selectedClient = ApiTestHttp.Client(http); }
            else { selectedClient = new MattermostClient("https://mattermost.example/chat", http); }
            using MattermostClient client = selectedClient;
            Team team = await client.GetTeamInviteInfoAsync("invite");
            Assert.That(team.Id, Is.EqualTo("team"));
            Assert.That(team.Name, Is.EqualTo("name"));
            Assert.That(team.DisplayName, Is.EqualTo("Display"));
            Assert.That(team.Description, Is.EqualTo("Description"));
        }

        [Test]
        public async Task TeamAndMemberResponses_AreTyped()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                Response(request.RequestUri!.AbsolutePath.EndsWith("batch", StringComparison.Ordinal) ? "add" : "restore"))));
            using MattermostClient client = ApiTestHttp.Client(http);
            Team team = await client.RestoreTeamAsync("team");
            Assert.That(team.UpdatedAt, Is.EqualTo(4200000000123));
            Assert.That(team.InviteId, Is.EqualTo("invite-2"));
            IList<TeamMember> members = await client.AddTeamMembersAsync("team", new[] { "user-1" });
            Assert.That(members[0].TeamId, Is.EqualTo("team-1"));
            Assert.That(members[0].UserId, Is.EqualTo("user-1"));
            Assert.That(members[0].SchemeAdmin, Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations)
            {
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
            }
            await Assert.ThrowsAsync<ArgumentException>(() => client.RemoveTeamMemberAsync("team", id!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateTeamMemberRolesAsync("team", id!, "team_user"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AddTeamMembersAsync("team", new[] { id! }));
        }

        [Test]
        public async Task InvalidModelsCollectionsAndEnums_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateTeamAsync(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.AddTeamMembersAsync("team", null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AddTeamMembersAsync("team", Array.Empty<string>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.AddTeamMembersAsync("team", Enumerable.Range(0, 257).Select(index => "user-" + index)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateTeamMemberRolesAsync("team", "user", null!));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateTeamPrivacyAsync("team", (TeamType)100));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [Values("update", "delete", "restore", "privacy", "regenerate", "invite", "add", "remove", "roles", "scheme")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttp(operation, (_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected(
            [Values("update", "delete", "restore", "privacy", "regenerate", "invite", "add", "remove", "roles", "scheme")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = CreateHttp(operation, (_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("update", "delete", "restore", "privacy", "regenerate", "invite", "add", "remove", "roles", "scheme")] string operation)
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
            [Values("update", "delete", "restore", "privacy", "regenerate", "invite", "add", "remove", "roles", "scheme")] string operation)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = CreateHttp(operation, async (_, token) =>
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

        private static readonly string[] Operations = { "update", "delete", "restore", "privacy", "regenerate", "invite", "add", "remove", "roles", "scheme" };

        private static HttpClient CreateHttp(string operation, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            if (operation != "invite") { return ApiTestHttp.Create(respond); }
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization, Is.Null);
                Assert.That(request.RequestUri!.AbsolutePath, Does.Contain("/teams/invite/"));
                return respond(request, token);
            }));
        }

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "update": case "restore": case "privacy": case "regenerate": case "invite": return TeamJson;
                case "add": return "[{\"team_id\":\"team-1\",\"user_id\":\"user-1\",\"scheme_admin\":true}]";
                case "delete": case "remove": case "roles": case "scheme": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, string teamId = " team/1?# ", CancellationToken token = default)
        {
            switch (operation)
            {
                case "update": return client.UpdateTeamAsync(new Team
                {
                    Id = teamId, Name = "team-name", DisplayName = "  Team ✨  ", Description = "Description", Type = "O",
                    Email = "team@example.org", AllowOpenInvite = false, InviteId = "readonly", CreatedAt = 4200000000123
                }, token);
                case "delete": return client.DeleteTeamAsync(teamId, cancellationToken: token);
                case "restore": return client.RestoreTeamAsync(teamId, token);
                case "privacy": return client.UpdateTeamPrivacyAsync(teamId, TeamType.InviteOnly, token);
                case "regenerate": return client.RegenerateTeamInviteIdAsync(teamId, token);
                case "invite": return client.GetTeamInviteInfoAsync(teamId, token);
                case "add": return client.AddTeamMembersAsync(teamId, new[] { " user/1?# ", "user-2" }, token);
                case "remove": return client.RemoveTeamMemberAsync(teamId, " user/1?# ", token);
                case "roles": return client.UpdateTeamMemberRolesAsync(teamId, " user/1?# ", "team_user team_admin", token);
                case "scheme": return client.UpdateTeamMemberSchemeRolesAsync(teamId, " user/1?# ", true, true, cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
