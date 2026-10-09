using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal partial class ChannelReadExpansionTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_DoNotSendRequests(string? identifier)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            await AssertInvalidIdentifierAsync(() => client.GetChannelMembersByIdsAsync(identifier!, new[] { "user-1" }), "channelId");
            await AssertInvalidIdentifierAsync(() => client.GetChannelStatsAsync(identifier!), "channelId");
            await AssertInvalidIdentifierAsync(() => client.GetChannelUnreadAsync(identifier!, "me"), "channelId");
            await AssertInvalidIdentifierAsync(() => client.GetChannelUnreadAsync("channel-1", identifier!), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamChannelsAsync(identifier!, "team-1"), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamChannelsAsync("me", identifier!), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamChannelMembersAsync(identifier!, "team-1"), "userId");
            await AssertInvalidIdentifierAsync(() => client.GetUserTeamChannelMembersAsync("me", identifier!), "teamId");
            await AssertInvalidIdentifierAsync(() => client.GetChannelTimezonesAsync(identifier!), "channelId");
        }

        [Test]
        public void NegativeDeletionTimestamp_DoesNotSendRequests()
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetUserTeamChannelsAsync("me", "team-1", lastDeleteAt: -1))!.ParamName,
                Is.EqualTo("lastDeleteAt"));
        }

        [TestCaseSource(nameof(InvalidUserIds))]
        public void InvalidBatch_DoesNotSendRequests(string[]? ids)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            Assert.That(Assert.Catch<ArgumentException>(() => client.GetChannelMembersByIdsAsync("channel-1", ids!))!.ParamName,
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
            [Values("batch", "stats", "unread", "channels", "memberships", "timezones")] string operation,
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
            [Values("batch", "stats", "unread", "channels", "memberships")] string operation,
            [Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task GetChannelTimezones_MalformedJson_IsRejected()
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetChannelTimezonesAsync("channel-1"));
        }

        [TestCase("batch", "{}")]
        [TestCase("stats", "[]")]
        [TestCase("unread", "[]")]
        [TestCase("channels", "{}")]
        [TestCase("memberships", "{}")]
        [TestCase("timezones", "[{}]")]
        public async Task WrongResponseShapes_AreRejected(string operation, string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("batch")]
        [TestCase("stats")]
        [TestCase("unread")]
        [TestCase("channels")]
        [TestCase("memberships")]
        [TestCase("timezones")]
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
            [Values("batch", "stats", "unread", "channels", "memberships", "timezones")] string operation,
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
                case "batch": return client.GetChannelMembersByIdsAsync("channel-1", new[] { "user-1" }, token);
                case "stats": return client.GetChannelStatsAsync("channel-1", cancellationToken: token);
                case "unread": return client.GetChannelUnreadAsync("channel-1", " me ", token);
                case "channels": return client.GetUserTeamChannelsAsync(" me ", "team-1", cancellationToken: token);
                case "memberships": return client.GetUserTeamChannelMembersAsync(" me ", "team-1", token);
                case "timezones": return client.GetChannelTimezonesAsync("channel-1", token);
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
