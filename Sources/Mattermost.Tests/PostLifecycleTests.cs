using Mattermost.Models.Channels;
using Mattermost.Models.Posts;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class PostLifecycleTests
    {
        [TestCase("history", "/chat/api/v4/posts/id%2F1/edit_history", "GET")]
        [TestCase("info", "/chat/api/v4/posts/id%2F1/info", "GET")]
        [TestCase("restore", "/chat/api/v4/posts/id%2F1/restore/version%2F2", "POST")]
        public async Task HistoryAndLocation_UseNativeRoutes(string operation, string route, string method)
        {
            string json = operation == "history" ? "[{\"id\":\"old\",\"original_id\":\"current\",\"message\":\"original\"}]" : "{\"id\":\"current\",\"message\":\"original\",\"channel_id\":\"channel\"}";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object? result = await InvokeAsync(client, operation, " id/1 ");
            switch (operation)
            {
                case "history":
                    Post historical = ((IList<Post>)result!).Single();
                    Assert.That(historical.Id, Is.EqualTo("old"));
                    Assert.That(historical.OriginalId, Is.EqualTo("current"));
                    Assert.That(historical.Text, Is.EqualTo("original"));
                    break;
                case "restore":
                    Assert.That(((Post)result!).Id, Is.EqualTo("current"));
                    Assert.That(((Post)result).Text, Is.EqualTo("original"));
                    break;
                case "info":
                    Assert.That(((PostLocationInfo)result!).ChannelId, Is.EqualTo("channel"));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task PostLocation_MapsMembershipAndDirectChannels([Values(false, true)] bool direct)
        {
            string json = direct
                ? "{\"channel_id\":\"channel\",\"channel_type\":\"D\",\"channel_display_name\":\"Direct\",\"has_joined_channel\":true,\"team_id\":\"\",\"team_type\":\"\",\"team_display_name\":\"\",\"has_joined_team\":false}"
                : "{\"channel_id\":\"channel\",\"channel_type\":\"O\",\"channel_display_name\":\"Public\",\"has_joined_channel\":false,\"team_id\":\"team\",\"team_type\":\"I\",\"team_display_name\":\"Team\",\"has_joined_team\":true}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            PostLocationInfo location = await client.GetPostInfoAsync("post");
            Assert.Multiple(() =>
            {
                Assert.That(location.ChannelId, Is.EqualTo("channel"));
                Assert.That(location.ChannelType, Is.EqualTo(direct ? "D" : "O"));
                Assert.That(location.ChannelDisplayName, Is.EqualTo(direct ? "Direct" : "Public"));
                Assert.That(location.HasJoinedChannel, Is.EqualTo(direct));
                Assert.That(location.TeamId, Is.EqualTo(direct ? "" : "team"));
                Assert.That(location.TeamType, Is.EqualTo(direct ? "" : "I"));
                Assert.That(location.TeamDisplayName, Is.EqualTo(direct ? "" : "Team"));
                Assert.That(location.HasJoinedTeam, Is.EqualTo(!direct));
            });
        }

        [Test]
        public async Task MarkUnread_SendsOnlyThreadOptionAndReadsBoundary([Values(false, true)] bool collapsed)
        {
            const long counter = 5000000001;
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/me/posts/id%2F1/set_unread"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("collapsed_threads_supported").GetBoolean(), Is.EqualTo(collapsed));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"team_id\":\"team\",\"user_id\":\"user\",\"channel_id\":\"channel\",\"msg_count\":5000000001,\"mention_count\":5000000002,\"mention_count_root\":5000000003,\"urgent_mention_count\":5000000004,\"msg_count_root\":5000000005,\"last_viewed_at\":1800000000000}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelReadState state = await client.MarkPostUnreadAsync(" me ", " id/1 ", collapsed);
            Assert.Multiple(() =>
            {
                Assert.That(state.TeamId, Is.EqualTo("team"));
                Assert.That(state.UserId, Is.EqualTo("user"));
                Assert.That(state.ChannelId, Is.EqualTo("channel"));
                Assert.That(state.MessageCount, Is.EqualTo(counter));
                Assert.That(state.MentionCount, Is.EqualTo(counter + 1));
                Assert.That(state.RootMentionCount, Is.EqualTo(counter + 2));
                Assert.That(state.UrgentMentionCount, Is.EqualTo(counter + 3));
                Assert.That(state.RootMessageCount, Is.EqualTo(counter + 4));
                Assert.That(state.LastViewedAt, Is.EqualTo(1800000000000L));
            });
        }

        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(5000000001L)]
        public async Task Reminders_UseUnixSecondsAndPreserveInstant(long seconds)
        {
            DateTimeOffset remindAt = DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(123).ToOffset(TimeSpan.FromHours(5));
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F2/posts/post%2F1/reminder"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.RootElement.GetProperty("target_time").GetInt64(), Is.EqualTo(seconds));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SetPostReminderAsync(" user/2 ", " post/1 ", remindAt);
        }

        [Test]
        public async Task MarkUnread_DefaultsToUnsupportedCollapsedThreads()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Assert.That(body.RootElement.GetProperty("collapsed_threads_supported").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.MarkPostUnreadAsync("me", "post");
        }

        [TestCase("null")]
        [TestCase("[]")]
        public async Task EditHistory_NormalizesEmptyCollections(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetPostEditHistoryAsync("post"), Is.Empty);
        }

        private static async Task<object?> InvokeAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "history": return await client.GetPostEditHistoryAsync(id, token);
                case "info": return await client.GetPostInfoAsync(id, token);
                case "restore": return await client.RestorePostVersionAsync(id, " version/2 ", token);
                case "unread": return await client.MarkPostUnreadAsync("me", id, cancellationToken: token);
                case "reminder":
                    await client.SetPostReminderAsync("me", id, DateTimeOffset.FromUnixTimeSeconds(1800000000), token);
                    return null;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
