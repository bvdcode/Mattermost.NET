using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class TeamReadTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_DoNotSendRequests(string? identifier)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            await AssertInvalidIdentifierAsync(() => client.GetTeamMembersAsync(identifier!), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetTeamMemberAsync(identifier!, "me"), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetTeamMemberAsync("team-1", identifier!), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetTeamMembersByIdsAsync(identifier!, new[] { "user-1" }), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamMembersAsync(identifier!), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetTeamStatsAsync(identifier!), "teamId");
            await AssertInvalidIdentifierAsync(() => client.TeamExistsAsync(identifier!), "teamName");
            await AssertInvalidIdentifierAsync(() => client.GetTeamUnreadAsync(identifier!, "me"), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetTeamUnreadAsync("team-1", identifier!), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamsUnreadAsync(identifier!), "userId");
        }

        [TestCase(-1, 60, "page")]
        [TestCase(0, 0, "perPage")]
        [TestCase(0, -1, "perPage")]
        [TestCase(0, 201, "perPage")]
        public void InvalidPaging_DoesNotSendRequests(int page, int perPage, string parameter)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetTeamMembersAsync("team-1", page, perPage))!.ParamName,
                Is.EqualTo(parameter));
        }

        [TestCaseSource(nameof(InvalidUserIds))]
        public void InvalidBatch_DoesNotSendRequests(string[]? ids)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Catch<ArgumentException>(() => client.GetTeamMembersByIdsAsync("team-1", ids!))!.ParamName,
                Is.EqualTo("userIds"));
        }

        private static IEnumerable<TestCaseData> InvalidUserIds()
        {
            yield return new TestCaseData(new object?[] { null });
            yield return new TestCaseData(new object[] { Array.Empty<string>() });
            yield return new TestCaseData(new object[] { new string[] { null! } });
            yield return new TestCaseData(new object[] { new[] { "first", "" } });
            yield return new TestCaseData(new object[] { new[] { " \t" } });
        }

        [Test]
        public async Task HttpFailures_ArePropagated(
            [Values("members", "member", "batch", "user-members", "stats", "exists", "unread", "user-unread")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Access denied\"}")));
            using MattermostClient client = CreateClient(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;
            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Access denied"));
        }

        [Test]
        public async Task InvalidResponses_AreRejected(
            [Values("members", "member", "batch", "user-members", "stats", "exists", "unread", "user-unread")] string operation,
            [Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task TeamExists_MissingFlag_IsNotTreatedAsFalse()
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "{}")));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => client.TeamExistsAsync("team-name"));
        }

        [TestCase("members")]
        [TestCase("member")]
        [TestCase("batch")]
        [TestCase("user-members")]
        [TestCase("stats")]
        [TestCase("exists")]
        [TestCase("unread")]
        [TestCase("user-unread")]
        public async Task CancelledAndDisposedCalls_DoNotSendRequests(string operation)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_ReachesAuthenticationAndRequest(
            [Values("members", "member", "batch", "user-members", "stats", "exists", "unread", "user-unread")] string operation,
            [Values(false, true)] bool duringAuthentication)
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("The request must be cancelled.");
            }));
            using MattermostClient client = CreateClient(http);
            Task pending = InvokeAsync(client, operation, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "members": return client.GetTeamMembersAsync("team-1", cancellationToken: token);
                case "member": return client.GetTeamMemberAsync("team-1", "me", token);
                case "batch": return client.GetTeamMembersByIdsAsync("team-1", new[] { "user-1" }, token);
                case "user-members": return client.GetUserTeamMembersAsync("me", token);
                case "stats": return client.GetTeamStatsAsync("team-1", token);
                case "exists": return client.TeamExistsAsync("team-name", token);
                case "unread": return client.GetTeamUnreadAsync("team-1", "me", token);
                case "user-unread": return client.GetUserTeamsUnreadAsync("me", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static async Task AssertInvalidIdentifierAsync(Func<Task> invoke, string parameterName)
        {
            ArgumentException exception = (await Assert.CatchAsync<ArgumentException>(async () => await invoke()))!;
            Assert.That(exception.ParamName, Is.EqualTo(parameterName));
        }
    }
}
