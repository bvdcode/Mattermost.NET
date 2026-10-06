using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelReadTests
    {
        [TestCase(false, 0L)]
        [TestCase(true, 0L)]
        [TestCase(true, 1790000000000L)]
        [TestCase(false, 1790000000000L)]
        [SetCulture("ar-SA")]
        public async Task GetUserChannels_EscapesUserAndSendsFilters(bool includeDeleted, long lastDeleteAt)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                string expectedQuery = FormattableString.Invariant($"?include_deleted={includeDeleted.ToString().ToLowerInvariant()}&last_delete_at={lastDeleteAt}");
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3F%23/channels" + expectedQuery));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK,
                    "[{\"id\":\"channel-1\",\"team_id\":\"team-1\",\"type\":\"G\",\"display_name\":\"Group chat\",\"delete_at\":1790000000000}]"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            IList<Channel> channels = await api.GetUserChannelsAsync(" user/1?# ", includeDeleted, lastDeleteAt);

            Assert.That(channels, Has.Count.EqualTo(1));
            Assert.That(channels[0].Id, Is.EqualTo("channel-1"));
            Assert.That(channels[0].TeamId, Is.EqualTo("team-1"));
            Assert.That(channels[0].ChannelType, Is.EqualTo(ChannelType.Group));
            Assert.That(channels[0].DisplayName, Is.EqualTo("Group chat"));
            Assert.That(channels[0].DeletedAt, Is.EqualTo(1790000000000L));
        }

        [Test]
        public async Task GetUserChannels_DefaultsAndCurrentUser_AreSent()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.PathAndQuery,
                    Is.EqualTo("/chat/api/v4/users/me/channels?include_deleted=false&last_delete_at=0"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = CreateClient(http);

            Assert.That(await client.GetUserChannelsAsync(" me "), Is.Empty);
        }

        [TestCase(0, 60)]
        [TestCase(2, 15)]
        [TestCase(0, 200)]
        public async Task GetChannelMembers_EscapesChannelAndPages(int page, int perPage)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo($"https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/members?page={page}&per_page={perPage}"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[" + MemberJson(1790000000000L) + "]"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            IList<ChannelUserInfo> members = await api.GetChannelMembersAsync(" channel/1?# ", page, perPage);

            Assert.That(members, Has.Count.EqualTo(1));
            AssertMember(members[0], 1790000000000L);
        }

        [TestCase(-1L)]
        [TestCase(2147483647L)]
        [TestCase(2147483648L)]
        [TestCase(1790000000000L)]
        [TestCase(long.MaxValue)]
        public async Task GetChannelMember_EscapesIdentifiersAndPreservesInt64(long timestamp)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsoluteUri,
                    Is.EqualTo("https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/members/user%2F1%3F%23"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, MemberJson(timestamp)));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            ChannelUserInfo member = await api.GetChannelMemberAsync(" channel/1?# ", " user/1?# ");

            AssertMember(member, timestamp);
        }

        [Test]
        public async Task GetChannelMembers_EmptyPage_ReturnsEmptyList()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60"));
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = CreateClient(http);

            Assert.That(await client.GetChannelMembersAsync("channel-1"), Is.Empty);
        }

        [Test]
        public void BlankIdentifiers_DoNotSendRequests(
            [Values("channels", "members", "member-channel", "member-user")] string operation,
            [Values(null, "", " \t")] string? id)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            switch (operation)
            {
                case "channels":
                    Assert.That(Assert.Throws<ArgumentException>(() => client.GetUserChannelsAsync(id!))!.ParamName, Is.EqualTo("userId"));
                    break;
                case "members":
                    Assert.That(Assert.Throws<ArgumentException>(() => client.GetChannelMembersAsync(id!))!.ParamName, Is.EqualTo("channelId"));
                    break;
                case "member-channel":
                    Assert.That(Assert.Throws<ArgumentException>(() => client.GetChannelMemberAsync(id!, "user-1"))!.ParamName, Is.EqualTo("channelId"));
                    break;
                case "member-user":
                    Assert.That(Assert.Throws<ArgumentException>(() => client.GetChannelMemberAsync("channel-1", id!))!.ParamName, Is.EqualTo("userId"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [TestCase(-1, 60, "page")]
        [TestCase(0, 0, "perPage")]
        [TestCase(0, -1, "perPage")]
        [TestCase(0, 201, "perPage")]
        public void InvalidPaging_DoesNotSendRequests(int page, int perPage, string parameter)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetChannelMembersAsync("channel-1", page, perPage))!.ParamName,
                Is.EqualTo(parameter));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NegativeDeletionTimestamp_DoesNotSendRequests(bool includeDeleted)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetUserChannelsAsync("user-1", includeDeleted, -1))!.ParamName,
                Is.EqualTo("lastDeleteAt"));
        }

        [Test]
        public async Task HttpFailures_ArePropagated(
            [Values("channels", "members", "member")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Request rejected\"}")));
            using MattermostClient client = CreateClient(http);

            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;

            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Request rejected"));
        }

        [Test]
        public async Task InvalidSuccessBody_IsNotHidden(
            [Values("channels", "members", "member")] string operation,
            [Values("null", "invalid-json")] string body)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);

            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("channels")]
        [TestCase("members")]
        [TestCase("member")]
        public void CancelledAndDisposedCalls_DoNotSendRequests(string operation)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_StopsPendingRequest(
            [Values("channels", "members", "member")] string operation, [Values] bool duringAuthentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            cancellation.Cancel();

            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static string MemberJson(long timestamp) => JsonSerializer.Serialize(new
        {
            channel_id = "channel-1",
            user_id = "user-1",
            roles = "channel_user channel_admin",
            last_viewed_at = timestamp,
            last_update_at = timestamp,
            msg_count = 2147483648L,
            mention_count = long.MaxValue,
            notify_props = new { desktop = "mention", push = "none" }
        });

        private static void AssertMember(ChannelUserInfo member, long timestamp)
        {
            Assert.That(member.ChannelId, Is.EqualTo("channel-1"));
            Assert.That(member.UserId, Is.EqualTo("user-1"));
            Assert.That(member.Roles, Is.EqualTo("channel_user channel_admin"));
            Assert.That(member.LastViewedAt, Is.EqualTo(timestamp));
            Assert.That(member.UpdatedAt, Is.EqualTo(timestamp));
            Assert.That(member.MessageCount, Is.EqualTo(2147483648L));
            Assert.That(member.MentionCount, Is.EqualTo(long.MaxValue));
            Assert.That(member.NotifyProps.Desktop, Is.EqualTo("mention"));
            Assert.That(member.NotifyProps.Push, Is.EqualTo("none"));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "channels": return client.GetUserChannelsAsync("user-1", cancellationToken: token);
                case "members": return client.GetChannelMembersAsync("channel-1", cancellationToken: token);
                case "member": return client.GetChannelMemberAsync("channel-1", "user-1", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static MattermostClient CreateClient(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);

        private static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
