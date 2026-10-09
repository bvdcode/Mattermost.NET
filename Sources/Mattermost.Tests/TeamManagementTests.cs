using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Teams;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TeamManagementTests
    {
        private const string TeamJson = "{\"id\":\"team-1\",\"name\":\"test\",\"display_name\":\"Test\",\"description\":\"info\",\"company_name\":\"Company\",\"allow_open_invite\":false}";

        [TestCase(TeamType.Open, "O")]
        [TestCase(TeamType.InviteOnly, "I")]
        public async Task Create_SendsOnlyRequiredFields(TeamType type, string wireType)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams"));
                using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                Assert.That(json.RootElement.GetProperty("name").GetString(), Is.EqualTo("test"));
                Assert.That(json.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("Test"));
                Assert.That(json.RootElement.GetProperty("type").GetString(), Is.EqualTo(wireType));
                return ApiTestHttp.Json(HttpStatusCode.Created, TeamJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Team team = await client.CreateTeamAsync(" test ", " Test ", type);
            Assert.That(team.Id, Is.EqualTo("team-1"));
            Assert.That(team.CompanyName, Is.EqualTo("Company"));
        }

        [Test]
        public async Task Patch_OmitsNullAndKeepsEmptyStringAndFalse()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/team%2F1%3F%23/patch"));
                using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(4));
                Assert.That(json.RootElement.TryGetProperty("display_name", out _), Is.False);
                Assert.That(json.RootElement.GetProperty("description").GetString(), Is.Empty);
                Assert.That(json.RootElement.GetProperty("company_name").GetString(), Is.EqualTo("  Company  "));
                Assert.That(json.RootElement.GetProperty("allowed_domains").GetString(), Is.Empty);
                Assert.That(json.RootElement.GetProperty("allow_open_invite").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, TeamJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Team team = await client.PatchTeamAsync(" team/1?# ", description: "", companyName: "  Company  ",
                allowedDomains: "", allowOpenInvite: false);
            Assert.That(team.Description, Is.EqualTo("info"));
        }

        [TestCase(" \t ", "")]
        [TestCase(" support ", "support")]
        [TestCase("support", "support")]
        public async Task Search_SendsOnlyTrimmedTerm(string term, string expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/search"));
                using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(json.RootElement.GetProperty("term").GetString(), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[" + TeamJson + "]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<Team> teams = await client.SearchTeamsAsync(term);
            Assert.That(teams.Single().Id, Is.EqualTo("team-1"));
        }

        [Test]
        public async Task AddMember_EscapesRouteButNotBodyIdentifiers()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/team%2F1%3F%23/members"));
                using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(json.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
                Assert.That(json.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/1?#"));
                Assert.That(json.RootElement.GetProperty("user_id").GetString(), Is.EqualTo("user/1?#"));
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"team_id\":\"team-1\",\"user_id\":\"user-1\",\"roles\":\"team_user\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            TeamMember member = await client.AddTeamMemberAsync(" team/1?# ", " user/1?# ");
            Assert.That(member.UserId, Is.EqualTo("user-1"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankArguments_AreRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.CreateTeamAsync(value!, "Test", TeamType.Open));
            Assert.Throws<ArgumentException>(() => client.CreateTeamAsync("test", value!, TeamType.Open));
            Assert.Throws<ArgumentException>(() => client.PatchTeamAsync(value!));
            Assert.Throws<ArgumentException>(() => client.AddTeamMemberAsync(value!, "user-1"));
            Assert.Throws<ArgumentException>(() => client.AddTeamMemberAsync("team-1", value!));
        }

        [Test]
        public void InvalidTypeAndNullTerm_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentOutOfRangeException>(() => client.CreateTeamAsync("test", "Test", (TeamType)42));
            Assert.Throws<ArgumentNullException>(() => client.SearchTeamsAsync(null!));
        }

        [Test]
        public async Task EmptyPatch_SendsEmptyObject()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{}"));
                return ApiTestHttp.Json(HttpStatusCode.OK, TeamJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.PatchTeamAsync("team-1");
        }

        [Test]
        public async Task ApiErrors_PreserveDetails([Values("create", "patch", "search", "member")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(exception!.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("rejected"));
            Assert.That(exception.RequestUri, Does.StartWith("https://mattermost.example/chat/api/v4/teams"));
        }

        [Test]
        public async Task MalformedResponses_Throw([Values("create", "patch", "search", "member")] string operation,
            [Values("null", "not json")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("create", "patch", "search", "member")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => InvokeAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        private static Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "create": return client.CreateTeamAsync("test", "Test", TeamType.Open, token);
                case "patch": return client.PatchTeamAsync("team-1", cancellationToken: token);
                case "search": return client.SearchTeamsAsync("test", cancellationToken: token);
                case "member": return client.AddTeamMemberAsync("team-1", "user-1", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
