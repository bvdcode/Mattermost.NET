using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Schemes;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class SchemeApiTests
    {
        private const string SchemeJson = "{\"id\":\"scheme\",\"name\":\"name\",\"display_name\":\"Display\",\"description\":\"Description\",\"scope\":\"team\",\"create_at\":9007199254740993,\"update_at\":9007199254740994,\"delete_at\":0,\"default_team_admin_role\":\"ta\",\"default_team_user_role\":\"tu\",\"default_team_guest_role\":\"tg\",\"default_channel_admin_role\":\"ca\",\"default_channel_user_role\":\"cu\",\"default_channel_guest_role\":\"cg\",\"default_playbook_admin_role\":\"pa\",\"default_playbook_member_role\":\"pm\",\"default_run_admin_role\":\"ra\",\"default_run_member_role\":\"rm\"}";
        private static readonly string[] Operations = { "list", "get", "create", "patch", "delete", "teams", "channels", "team-set", "channel-set" };

        [TestCase("list", "GET", "/schemes?page=2&per_page=17&scope=team", null)]
        [TestCase("get", "GET", "/schemes/scheme%2F1%3F%23", null)]
        [TestCase("create", "POST", "/schemes", "{\"display_name\":\"Display\",\"scope\":\"channel\",\"description\":\"Description\",\"name\":\"name\"}")]
        [TestCase("patch", "PUT", "/schemes/scheme%2F1%3F%23/patch", "{\"description\":\"\"}")]
        [TestCase("delete", "DELETE", "/schemes/scheme%2F1%3F%23", null)]
        [TestCase("teams", "GET", "/schemes/scheme%2F1%3F%23/teams?page=2&per_page=17", null)]
        [TestCase("channels", "GET", "/schemes/scheme%2F1%3F%23/channels?page=2&per_page=17", null)]
        [TestCase("team-set", "PUT", "/teams/team%2F1%3F%23/scheme", "{\"scheme_id\":\"scheme/1?#\"}")]
        [TestCase("channel-set", "PUT", "/channels/channel%2F1%3F%23/scheme", "{\"scheme_id\":\"scheme/1?#\"}")]
        public async Task Requests_UseExactRoutesAndSelectedPayloads(string operation, string method, string route, string? json)
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
            await InvokeAsync(client, operation);
        }

        [TestCase(null, "")]
        [TestCase(SchemeScope.Team, "&scope=team")]
        [TestCase(SchemeScope.Channel, "&scope=channel")]
        public async Task ScopeFilters_AreExplicit(SchemeScope? scope, string suffix)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60" + suffix));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetSchemesAsync(scope: scope), Is.Empty);
        }

        [Test]
        public async Task Create_OmitsServerControlledFieldsAndGeneratedName()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(item => item.Name), Is.EquivalentTo(new[] { "display_name", "scope", "description" }));
                return ApiTestHttp.Json(HttpStatusCode.Created, SchemeJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Scheme result = await client.CreateSchemeAsync("Display", SchemeScope.Team);
            Assert.That(result.Id, Is.EqualTo("scheme"));
        }

        [Test]
        public async Task TeamAssignment_EmptyRestoresDefault()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{\"scheme_id\":\"\"}"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SetTeamSchemeAsync("team", " ");
        }

        [Test]
        public void SchemeModel_PreservesAllNativeRoleNamesAndIntegerTimes()
        {
            Scheme scheme = JsonSerializer.Deserialize<Scheme>(SchemeJson)!;
            Assert.That(scheme.CreatedAt, Is.EqualTo(9007199254740993L));
            Assert.That(scheme.UpdatedAt, Is.EqualTo(9007199254740994L));
            Assert.That(scheme.DefaultTeamAdminRole, Is.EqualTo("ta"));
            Assert.That(scheme.DefaultTeamUserRole, Is.EqualTo("tu"));
            Assert.That(scheme.DefaultTeamGuestRole, Is.EqualTo("tg"));
            Assert.That(scheme.DefaultChannelAdminRole, Is.EqualTo("ca"));
            Assert.That(scheme.DefaultChannelUserRole, Is.EqualTo("cu"));
            Assert.That(scheme.DefaultChannelGuestRole, Is.EqualTo("cg"));
            Assert.That(scheme.DefaultPlaybookAdminRole, Is.EqualTo("pa"));
            Assert.That(scheme.DefaultPlaybookMemberRole, Is.EqualTo("pm"));
            Assert.That(scheme.DefaultRunAdminRole, Is.EqualTo("ra"));
            Assert.That(scheme.DefaultRunMemberRole, Is.EqualTo("rm"));
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(scheme));
            Assert.That(serialized.RootElement.EnumerateObject().Count(), Is.EqualTo(18));
            Assert.That(serialized.RootElement.GetProperty("create_at").GetInt64(), Is.EqualTo(scheme.CreatedAt));
        }

        [Test]
        public void InvalidInputs_RejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetSchemesAsync(scope: (SchemeScope)123));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.CreateSchemeAsync("name", (SchemeScope)123));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetSchemesAsync(page: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetSchemeTeamsAsync("id", perPage: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetSchemeChannelsAsync("id", page: -1));
            Assert.Throws<ArgumentException>(() => client.GetSchemeAsync(" "));
            Assert.Throws<ArgumentException>(() => client.CreateSchemeAsync(" ", SchemeScope.Team));
            Assert.Throws<ArgumentNullException>(() => client.CreateSchemeAsync("name", SchemeScope.Team, description: null!));
            Assert.Throws<ArgumentException>(() => client.PatchSchemeAsync("id"));
            Assert.Throws<ArgumentException>(() => client.DeleteSchemeAsync(" "));
            Assert.Throws<ArgumentNullException>(() => client.SetTeamSchemeAsync("team", null!));
            Assert.Throws<ArgumentException>(() => client.SetChannelSchemeAsync("channel", " "));
        }

        [Test]
        public async Task NativeErrors_ArePreserved([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.scheme.error\",\"message\":\"scheme rejected\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
        }

        [Test]
        public async Task NullAndMalformedResponses_AreRejected([ValueSource(nameof(Operations))] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
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
        public async Task InFlightCancellation_ReachesAuthenticationAndRequest([ValueSource(nameof(Operations))] string operation,
            [Values(false, true)] bool authentication)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            }));
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
                case "list": return "[" + SchemeJson + "]";
                case "get": case "create": case "patch": return SchemeJson;
                case "teams": return "[{\"id\":\"team\"}]";
                case "channels": return "[{\"id\":\"channel\"}]";
                case "delete": case "team-set": case "channel-set": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetSchemesAsync(2, 17, SchemeScope.Team, token);
                case "get": return client.GetSchemeAsync(" scheme/1?# ", token);
                case "create": return client.CreateSchemeAsync(" Display ", SchemeScope.Channel, " name ", " Description ", token);
                case "patch": return client.PatchSchemeAsync(" scheme/1?# ", description: " ", cancellationToken: token);
                case "delete": return client.DeleteSchemeAsync(" scheme/1?# ", token);
                case "teams": return client.GetSchemeTeamsAsync(" scheme/1?# ", 2, 17, token);
                case "channels": return client.GetSchemeChannelsAsync(" scheme/1?# ", 2, 17, token);
                case "team-set": return client.SetTeamSchemeAsync(" team/1?# ", " scheme/1?# ", token);
                case "channel-set": return client.SetChannelSchemeAsync(" channel/1?# ", " scheme/1?# ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
