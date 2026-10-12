using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserCountTests
    {
        [Test]
        public async Task Count_UsesNativeQueryAndLongResult([Values(false, true)] bool deleted,
            [Values(false, true)] bool bots, [Values(false, true)] bool remote)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/stats/filtered?include_deleted="
                    + deleted.ToString().ToLowerInvariant() + "&include_bots=" + bots.ToString().ToLowerInvariant()
                    + "&include_remote_users=" + remote.ToString().ToLowerInvariant()));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"total_users_count\":4294967297}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetFilteredUserCountAsync(includeDeleted: deleted, includeBots: bots, includeRemoteUsers: remote), Is.EqualTo(4294967297L));
        }

        [Test]
        public async Task Count_TrimsEscapesAndDeduplicatesRoleFilters()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/stats/filtered?include_deleted=false&include_bots=false&include_remote_users=false"
                    + "&in_team=team%2F1&in_channel=channel%2F1&roles=system_user%2Csystem_admin&team_roles=team_user&channel_roles=channel_user"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"total_users_count\":0}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            long count = await client.GetFilteredUserCountAsync(" team/1 ", " channel/1 ", roles: new[] { " system_user ", "system_admin", "system_user" },
                teamRoles: new[] { " team_user " }, channelRoles: new[] { " channel_user " });
            Assert.That(count, Is.Zero);
        }

        [Test]
        public async Task Count_EscapesRoleQueryValues()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Does.EndWith("&roles=role%26in_team%3Devil"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"total_users_count\":0}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetFilteredUserCountAsync(roles: new[] { "role&in_team=evil" });
        }

        [Test]
        public async Task Count_RejectsBlankIdentifiers([Values(false, true)] bool team, [Values("", " ")] string value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (team) { await Assert.ThrowsAsync<ArgumentException>(() => client.GetFilteredUserCountAsync(teamId: value)); }
            else { await Assert.ThrowsAsync<ArgumentException>(() => client.GetFilteredUserCountAsync(channelId: value)); }
        }

        [Test]
        public async Task Count_ScopedRolesRequireMatchingIdentifier([Values(false, true)] bool team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (team) { await Assert.ThrowsAsync<ArgumentException>(() => client.GetFilteredUserCountAsync(teamRoles: new[] { "team_user" })); }
            else { await Assert.ThrowsAsync<ArgumentException>(() => client.GetFilteredUserCountAsync(channelRoles: new[] { "channel_user" })); }
        }

        [Test]
        public async Task Count_RoleCollectionsCannotBeEmpty([Values("roles", "teamRoles", "channelRoles")] string filter)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => WithRolesAsync(client, filter, Array.Empty<string>()));
        }

        [Test]
        public async Task Count_RoleNamesMustBeNonblankAndSeparate([Values("roles", "teamRoles", "channelRoles")] string filter,
            [Values(null, "", " ", "system_user,system_admin")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => WithRolesAsync(client, filter, new[] { value! }));
        }

        [TestCase("null")]
        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("{\"total_users_count\":null}")]
        [TestCase("{\"total_users_count\":\"1\"}")]
        [TestCase("{\"total_users_count\":1.5}")]
        [TestCase("{\"total_users_count\":9223372036854775808}")]
        [TestCase("invalid JSON")]
        public async Task Count_RejectsInvalidNativeCount(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetFilteredUserCountAsync());
        }

        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Count_PreservesServerErrors(HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"count unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.GetFilteredUserCountAsync());
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("count unavailable"));
        }

        [Test]
        public async Task Count_CancellationAndDisposalRejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => client.GetFilteredUserCountAsync(cancellationToken: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetFilteredUserCountAsync());
        }

        [Test]
        public async Task Count_CancellationReachesHttp()
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
            Task pending = client.GetFilteredUserCountAsync(cancellationToken: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task<long> WithRolesAsync(MattermostClient client, string filter, IEnumerable<string> roles)
        {
            switch (filter)
            {
                case "roles": return client.GetFilteredUserCountAsync(roles: roles);
                case "teamRoles": return client.GetFilteredUserCountAsync(teamId: "team-1", teamRoles: roles);
                case "channelRoles": return client.GetFilteredUserCountAsync(channelId: "channel-1", channelRoles: roles);
                default: throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }
    }
}
