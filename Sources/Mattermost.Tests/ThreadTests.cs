using Mattermost.Exceptions;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ThreadTests
    {
        private const string ThreadJson = "{\"id\":\"root\",\"reply_count\":4200000000,\"last_reply_at\":4200000000123,\"last_viewed_at\":4200000000111,\"participants\":[{\"id\":\"user\",\"username\":\"participant\"}],\"post\":{\"id\":\"root\",\"message\":\"Root text\"},\"unread_replies\":3100000000,\"unread_mentions\":3000000000,\"is_urgent\":true,\"delete_at\":4200000000456}";

        [TestCase("list", "GET", "?per_page=60&since=0&unread=false&extended=false&deleted=false&totalsOnly=false&threadsOnly=false&excludeDirect=false")]
        [TestCase("get", "GET", "/root%2F1%3F%23?extended=false")]
        [TestCase("follow", "PUT", "/root%2F1%3F%23/following")]
        [TestCase("unfollow", "DELETE", "/root%2F1%3F%23/following")]
        [TestCase("read", "PUT", "/root%2F1%3F%23/read/4200000000123")]
        [TestCase("unread", "POST", "/root%2F1%3F%23/set_unread/post%2F1%3F%23")]
        [TestCase("read-all", "PUT", "/read")]
        public async Task Requests_UseEscapedRoutesWithoutBodies(string operation, string method, string suffix)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23/teams/team%2F1%3F%23/threads" + suffix));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response(operation)));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task List_UsesNativeCursorFilters(bool after)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?per_page=2&since=4200000000123&unread=true&extended=true&deleted=true&totalsOnly=false&threadsOnly=true&excludeDirect=true&"
                    + (after ? "after" : "before") + "=cursor%2F1%3F%23"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response("list")));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetUserThreadsAsync("me", "team", perPage: 2,
                beforeThreadId: after ? null : " cursor/1?# ", afterThreadId: after ? " cursor/1?# " : null,
                since: 4200000000123, unread: true, extended: true, deleted: true, threadsOnly: true, excludeDirect: true);
        }

        [Test]
        public async Task ExtendedDetails_AndLargeTimestampsAreDeserialized()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, ThreadJson)));
            using MattermostClient client = ApiTestHttp.Client(http);
            UserThread thread = await client.GetUserThreadAsync("me", "team", "root", extended: true);
            Assert.That(thread.ReplyCount, Is.EqualTo(4200000000));
            Assert.That(thread.LastReplyAt, Is.EqualTo(4200000000123));
            Assert.That(thread.LastViewedAt, Is.EqualTo(4200000000111));
            Assert.That(thread.UnreadReplies, Is.EqualTo(3100000000));
            Assert.That(thread.UnreadMentions, Is.EqualTo(3000000000));
            Assert.That(thread.DeletedAt, Is.EqualTo(4200000000456));
            Assert.That(thread.IsUrgent, Is.True);
            Assert.That(thread.Participants![0].Username, Is.EqualTo("participant"));
            Assert.That(thread.Post!.Text, Is.EqualTo("Root text"));
        }

        [Test]
        public async Task Totals_AndEmptyCollectionsPreserveNativeShape()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Does.Contain("totalsOnly=true"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                    "{\"total\":4200000000,\"total_unread_threads\":3100000000,\"total_unread_mentions\":3000000000,\"total_unread_urgent_mentions\":2900000000,\"threads\":null}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserThreadsResponse response = await client.GetUserThreadsAsync("me", "team", totalsOnly: true);
            Assert.That(response.Total, Is.EqualTo(4200000000));
            Assert.That(response.TotalUnreadThreads, Is.EqualTo(3100000000));
            Assert.That(response.TotalUnreadMentions, Is.EqualTo(3000000000));
            Assert.That(response.TotalUnreadUrgentMentions, Is.EqualTo(2900000000));
            Assert.That(response.Threads, Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidIdentifiers_AreRejectedBeforeHttp(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations)
            {
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, userId: id!));
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, teamId: id!));
                if (operation != "list" && operation != "read-all")
                {
                    await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, threadId: id!));
                }
            }
            await Assert.ThrowsAsync<ArgumentException>(() => client.MarkThreadUnreadAsync("me", "team", "root", id!));
        }

        [Test]
        public async Task InvalidFilters_AreRejectedBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserThreadsAsync("me", "team", perPage: 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetUserThreadsAsync("me", "team", since: -1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUserThreadsAsync("me", "team", beforeThreadId: "a", afterThreadId: "b"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetUserThreadsAsync("me", "team", totalsOnly: true, threadsOnly: true));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateThreadReadAsync("me", "team", "root", -1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.UpdateThreadReadAsync("me", "team", "root", 0));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [Values("list", "get", "follow", "unfollow", "read", "unread", "read-all")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected(
            [Values("list", "get", "follow", "unfollow", "read", "unread", "read-all")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("list", "get", "follow", "unfollow", "read", "unread", "read-all")] string operation)
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
            [Values("list", "get", "follow", "unfollow", "read", "unread", "read-all")] string operation)
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
            Task pending = InvokeAsync(client, operation, token: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static readonly string[] Operations = { "list", "get", "follow", "unfollow", "read", "unread", "read-all" };

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "list": return "{\"total\":1,\"threads\":[" + ThreadJson + "]}";
                case "get": case "read": case "unread": return ThreadJson;
                case "follow": case "unfollow": case "read-all": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, string userId = " user/1?# ",
            string teamId = " team/1?# ", string threadId = " root/1?# ", CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetUserThreadsAsync(userId, teamId, cancellationToken: token);
                case "get": return client.GetUserThreadAsync(userId, teamId, threadId, cancellationToken: token);
                case "follow": return client.FollowThreadAsync(userId, teamId, threadId, token);
                case "unfollow": return client.UnfollowThreadAsync(userId, teamId, threadId, token);
                case "read": return client.UpdateThreadReadAsync(userId, teamId, threadId, 4200000000123, token);
                case "unread": return client.MarkThreadUnreadAsync(userId, teamId, threadId, " post/1?# ", token);
                case "read-all": return client.MarkAllThreadsReadAsync(userId, teamId, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
