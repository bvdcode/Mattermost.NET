using Mattermost.Exceptions;
using Mattermost.Models.Teams;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TeamInvitationTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task Join_UsesOneEscapedQueryAndNoBody(bool emailToken)
        {
            const string invitation = "a/b &?+#%ю";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.Content, Is.Null);
                string parameter = emailToken ? "token" : "invite_id";
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/teams/members/invite?" + parameter + "=" + Uri.EscapeDataString(invitation)));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Created,
                    "{\"team_id\":\"team-1\",\"user_id\":\"user-1\",\"roles\":\"team_user\",\"delete_at\":0,\"scheme_user\":true,\"scheme_admin\":false}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            TeamMember membership = await JoinAsync(client, emailToken, " " + invitation + " ");
            Assert.That(membership.TeamId, Is.EqualTo("team-1"));
            Assert.That(membership.UserId, Is.EqualTo("user-1"));
            Assert.That(membership.Roles, Is.EqualTo("team_user"));
            Assert.That(membership.DeletedAt, Is.Zero);
            Assert.That(membership.SchemeUser, Is.True);
            Assert.That(membership.SchemeAdmin, Is.False);
        }

        [TestCase(null, null)]
        [TestCase("invite", "token")]
        [TestCase("", null)]
        [TestCase(" ", null)]
        [TestCase(null, "")]
        [TestCase(null, " ")]
        public async Task Join_RequiresExactlyOneNonemptyInvitation(string? inviteId, string? token)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<ArgumentException>(() => client.JoinTeamFromInviteAsync(inviteId, token));
        }

        [Test]
        public async Task Join_PreservesServerError([Values(false, true)] bool emailToken,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"invalid invitation\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => JoinAsync(client, emailToken));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("invalid invitation"));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
        }

        [Test]
        public async Task Join_RejectsMissingOrMalformedMembership([Values(false, true)] bool emailToken,
            [Values("null", "[]", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Created, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => JoinAsync(client, emailToken));
        }

        [Test]
        public async Task Join_CancellationAndDisposalRejectBeforeHttp([Values(false, true)] bool emailToken)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => JoinAsync(client, emailToken, cancellationToken: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => JoinAsync(client, emailToken));
        }

        [Test]
        public async Task Join_CancellationReachesHttp([Values(false, true)] bool emailToken)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, cancellationToken) =>
            {
                entered.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = JoinAsync(client, emailToken, cancellationToken: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task<TeamMember> JoinAsync(MattermostClient client, bool emailToken, string value = "invite-1",
            CancellationToken cancellationToken = default)
        {
            if (emailToken) { return client.JoinTeamFromInviteAsync(token: value, cancellationToken: cancellationToken); }
            return client.JoinTeamFromInviteAsync(value, cancellationToken: cancellationToken);
        }
    }
}
