using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelMemberSettingsTests
    {
        [TestCase("roles", "PUT", "/channels/channel%2F1%3F%23/members/user%2F1%3F%23/roles")]
        [TestCase("scheme", "PUT", "/channels/channel%2F1%3F%23/members/user%2F1%3F%23/schemeRoles")]
        [TestCase("notify", "PUT", "/channels/channel%2F1%3F%23/members/user%2F1%3F%23/notify_props")]
        [TestCase("view", "POST", "/channels/members/user%2F1%3F%23/view")]
        [TestCase("read", "POST", "/channels/members/user%2F1%3F%23/mark_read")]
        [TestCase("counts", "POST", "/channels/stats/member_count")]
        [TestCase("search", "POST", "/channels/group/search")]
        public async Task Requests_UseExactRoutesAndPayloads(string operation, string method, string path)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                switch (operation)
                {
                    case "roles":
                        Assert.That(body.RootElement.GetProperty("roles").GetString(), Is.EqualTo("channel_user channel_admin"));
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                        break;
                    case "scheme":
                        Assert.That(body.RootElement.GetProperty("scheme_user").GetBoolean(), Is.True);
                        Assert.That(body.RootElement.GetProperty("scheme_admin").GetBoolean(), Is.True);
                        Assert.That(body.RootElement.GetProperty("scheme_guest").GetBoolean(), Is.False);
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                        break;
                    case "notify":
                        Assert.That(body.RootElement.GetProperty("desktop").GetString(), Is.EqualTo("mention"));
                        Assert.That(body.RootElement.GetProperty("mark_unread").GetString(), Is.EqualTo("mention"));
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
                        break;
                    case "view":
                        Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1?#"));
                        Assert.That(body.RootElement.GetProperty("prev_channel_id").GetString(), Is.EqualTo("previous/1"));
                        Assert.That(body.RootElement.GetProperty("collapsed_threads_supported").GetBoolean(), Is.True);
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                        break;
                    case "read": case "counts":
                        Assert.That(body.RootElement.EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "channel/1?#", "channel-2" }));
                        break;
                    case "search":
                        Assert.That(body.RootElement.GetProperty("term").GetString(), Is.EqualTo("member name ✨"));
                        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(operation));
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [TestCase("")]
        [TestCase(" \t")]
        public async Task ViewChannel_AllowsFocusLossAndOmitsPreviousChannel(string channelId)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty("collapsed_threads_supported").GetBoolean(), Is.False);
                Assert.That(body.RootElement.TryGetProperty("prev_channel_id", out _), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\",\"last_viewed_at_times\":null}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelViewResponse result = await client.ViewChannelAsync("me", channelId);
            Assert.That(result.Status, Is.EqualTo("OK"));
            Assert.That(result.LastViewedAtTimes, Is.Null);
        }

        [Test]
        public async Task Responses_PreserveCountersAndChannelModels()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                string operation = request.RequestUri!.AbsolutePath.Split('/').Last();
                switch (operation)
                {
                    case "view": case "mark_read": return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response("view")));
                    case "member_count": return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response("counts")));
                    case "search": return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Response("search")));
                    default: throw new AssertionException("Unexpected route.");
                }
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelViewResponse viewed = await client.ViewChannelAsync("me", "channel");
            Assert.That(viewed.Status, Is.EqualTo("OK"));
            Assert.That(viewed.LastViewedAtTimes!["channel-1"], Is.EqualTo(4200000000123));
            ChannelViewResponse read = await client.MarkChannelsReadAsync("me", new[] { "channel" });
            Assert.That(read.LastViewedAtTimes!["channel-1"], Is.EqualTo(4200000000123));
            IDictionary<string, long> counts = await client.GetChannelsMemberCountsAsync(new[] { "channel" });
            Assert.That(counts["channel-1"], Is.EqualTo(4200000000));
            IList<Channel> channels = await client.SearchGroupChannelsAsync("member");
            Assert.That(channels.Count, Is.EqualTo(1));
            Assert.That(channels[0].Type, Is.EqualTo("G"));
            Assert.That(channels[0].DisplayName, Is.EqualTo("Group chat"));
        }

        [TestCase(false, false, true)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        public async Task SchemeRoles_IncludeAllBooleanValues(bool user, bool admin, bool guest)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("scheme_user").GetBoolean(), Is.EqualTo(user));
                Assert.That(body.RootElement.GetProperty("scheme_admin").GetBoolean(), Is.EqualTo(admin));
                Assert.That(body.RootElement.GetProperty("scheme_guest").GetBoolean(), Is.EqualTo(guest));
                return ApiTestHttp.Json(HttpStatusCode.OK, Response("scheme"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateChannelMemberSchemeRolesAsync("channel", "me", user, admin, guest);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task InvalidArguments_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateChannelMemberRolesAsync(id!, "me", "channel_user"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateChannelMemberSchemeRolesAsync("channel", id!, true, false));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateChannelMemberNotifyPropsAsync(id!, "me", new Dictionary<string, string>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.ViewChannelAsync(id!, "channel"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.MarkChannelsReadAsync(id!, new[] { "channel" }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.MarkChannelsReadAsync("me", new[] { id! }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetChannelsMemberCountsAsync(new[] { id! }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.SearchGroupChannelsAsync(id!));
        }

        [Test]
        public async Task NullValuesAndEmptyBatches_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateChannelMemberRolesAsync("channel", "me", null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateChannelMemberNotifyPropsAsync("channel", "me", null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.ViewChannelAsync("me", null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.MarkChannelsReadAsync("me", null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetChannelsMemberCountsAsync(Array.Empty<string>()));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateChannelMemberNotifyPropsAsync("channel", "me", new Dictionary<string, string> { ["desktop"] = null! }));
        }

        [Test]
        public async Task Errors_PreserveServerDetails(
            [Values("roles", "scheme", "notify", "view", "read", "counts", "search")] string operation,
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
            [Values("roles", "scheme", "notify", "view", "read", "counts", "search")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp(
            [Values("roles", "scheme", "notify", "view", "read", "counts", "search")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp(
            [Values("roles", "scheme", "notify", "view", "read", "counts", "search")] string operation)
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
            Task pending = InvokeAsync(client, operation, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "roles": case "scheme": case "notify": return "{\"status\":\"OK\"}";
                case "view": case "read": return "{\"status\":\"OK\",\"last_viewed_at_times\":{\"channel-1\":4200000000123}}";
                case "counts": return "{\"channel-1\":4200000000}";
                case "search": return "[{\"id\":\"group-1\",\"type\":\"G\",\"display_name\":\"Group chat\"}]";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            const string channel = " channel/1?# ";
            const string user = " user/1?# ";
            string[] ids = { channel, "channel-2" };
            switch (operation)
            {
                case "roles": return client.UpdateChannelMemberRolesAsync(channel, user, "channel_user channel_admin", token);
                case "scheme": return client.UpdateChannelMemberSchemeRolesAsync(channel, user, true, true, cancellationToken: token);
                case "notify": return client.UpdateChannelMemberNotifyPropsAsync(channel, user,
                    new Dictionary<string, string> { ["desktop"] = "mention", ["mark_unread"] = "mention" }, token);
                case "view": return client.ViewChannelAsync(user, channel, " previous/1 ", true, token);
                case "read": return client.MarkChannelsReadAsync(user, ids, token);
                case "counts": return client.GetChannelsMemberCountsAsync(ids, token);
                case "search": return client.SearchGroupChannelsAsync(" member name ✨ ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
