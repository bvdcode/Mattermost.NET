using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelReadAllTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task MarkAllRead_UsesNativeRouteAndMillisecondResponse(bool team)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                string path = "/chat/api/v4/channels/members/user%2F1%3Fx%23y/direct/read";
                if (team) { path = "/chat/api/v4/users/user%2F1%3Fx%23y/teams/team%2F1%3Fx%23y/read"; }
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo(path));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri.Query, Is.Empty);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\",\"last_viewed_at_times\":{\"channel-1\":1791800000123,\"channel-2\":1791800000456}}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelViewResponse result = await InvokeAsync(client, team, " user/1?x#y ", " team/1?x#y ");
            Assert.That(result.Status, Is.EqualTo("OK"));
            Assert.That(result.LastViewedAtTimes, Has.Count.EqualTo(2));
            Assert.That(result.LastViewedAtTimes!["channel-1"], Is.EqualTo(1791800000123L));
            Assert.That(result.LastViewedAtTimes["channel-2"], Is.EqualTo(1791800000456L));
        }

        [TestCase(false, "null")]
        [TestCase(false, "{}")]
        [TestCase(true, "null")]
        [TestCase(true, "{}")]
        public async Task MarkAllRead_EmptyNativeTimeMapIsAccepted(bool team, string times)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsolutePath, Does.Contain("/me/"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\",\"last_viewed_at_times\":" + times + "}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelViewResponse result = await InvokeAsync(client, team, "me");
            Assert.That(result.Status, Is.EqualTo("OK"));
            if (times == "null") { Assert.That(result.LastViewedAtTimes, Is.Null); }
            else { Assert.That(result.LastViewedAtTimes, Is.Empty); }
        }

        [TestCase(false, null)]
        [TestCase(false, "")]
        [TestCase(false, " \t")]
        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(true, " \t")]
        public void MarkAllRead_BlankUserIsRejected(bool team, string? userId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => InvokeAsync(client, team, userId!))!;
            Assert.That(error.ParamName, Is.EqualTo("userId"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void MarkAllRead_BlankTeamIsRejected(string? teamId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => client.MarkAllTeamChannelsReadAsync("me", teamId!))!;
            Assert.That(error.ParamName, Is.EqualTo("teamId"));
        }

        [TestCase(false, HttpStatusCode.BadRequest)]
        [TestCase(false, HttpStatusCode.Unauthorized)]
        [TestCase(false, HttpStatusCode.Forbidden)]
        [TestCase(false, HttpStatusCode.NotFound)]
        [TestCase(false, HttpStatusCode.InternalServerError)]
        [TestCase(false, HttpStatusCode.NotImplemented)]
        [TestCase(true, HttpStatusCode.BadRequest)]
        [TestCase(true, HttpStatusCode.Unauthorized)]
        [TestCase(true, HttpStatusCode.Forbidden)]
        [TestCase(true, HttpStatusCode.NotFound)]
        [TestCase(true, HttpStatusCode.InternalServerError)]
        [TestCase(true, HttpStatusCode.NotImplemented)]
        public async Task MarkAllRead_NativeErrorIsPropagatedWithoutRetry(bool team, HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot mark read\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, team)))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot mark read"));
            Assert.That(error.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase(false, "null")]
        [TestCase(false, "[]")]
        [TestCase(false, "{broken")]
        [TestCase(true, "null")]
        [TestCase(true, "[]")]
        [TestCase(true, "{broken")]
        public async Task MarkAllRead_InvalidResponseIsRejected(bool team, string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => InvokeAsync(client, team));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MarkAllRead_CancelledAndDisposedCallsSendNothing(bool team)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => InvokeAsync(client, team, token: cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => InvokeAsync(client, team));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public async Task MarkAllRead_CancellationStopsAuthenticationOrRequest(bool team, bool authentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("Expected cancellation.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task<ChannelViewResponse> pending = InvokeAsync(client, team, token: cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task<ChannelViewResponse> InvokeAsync(IMattermostChannelsClient client, bool team,
            string userId = "user-1", string teamId = "team-1", CancellationToken token = default)
        {
            if (team) { return client.MarkAllTeamChannelsReadAsync(userId, teamId, token); }
            return client.MarkAllDirectMessagesReadAsync(userId, token);
        }
    }
}
