using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelMembershipUpdateTests
    {
        [Test]
        public async Task Set_UsesNativeBodyAndBatches([Values(false, true)] bool explicitAdmins)
        {
            const string response = "{\"added\":[\"user-2\"],\"removed\":[\"user-3\"]}\n{\"added\":[],\"removed\":[],\"promoted\":[\"user-2\"],\"demoted\":[\"user-1\"],\"errors\":[{\"user_id\":\"user-4\",\"id\":\"native.id\",\"error\":\"native text\"}]}\n";
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1/members?batch_size=1&batch_delay_ms=0"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("members").EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "user-1", "user-2", "user-2" }));
                Assert.That(body.RootElement.TryGetProperty("channel_admins", out JsonElement admins), Is.EqualTo(explicitAdmins));
                if (explicitAdmins) { Assert.That(admins.EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "user-2" })); }
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(explicitAdmins ? 2 : 1));
                return ApiTestHttp.Json(HttpStatusCode.OK, response);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<ChannelMembersUpdateResult> results = await client.SetChannelMembersAsync(" channel/1 ", new[] { " user-1 ", "user-2", "user-2" },
                explicitAdmins ? new[] { " user-2 " } : null, batchSize: 1, batchDelayMilliseconds: 0);
            Assert.That(results, Has.Count.EqualTo(2));
            Assert.That(results[0].Added, Is.EqualTo(new[] { "user-2" }));
            Assert.That(results[0].Removed, Is.EqualTo(new[] { "user-3" }));
            Assert.That(results[0].Errors, Is.Empty);
            Assert.That(results[1].Promoted, Is.EqualTo(new[] { "user-2" }));
            Assert.That(results[1].Demoted, Is.EqualTo(new[] { "user-1" }));
            Assert.That(results[1].Errors.Single().UserId, Is.EqualTo("user-4"));
            Assert.That(results[1].Errors.Single().Id, Is.EqualTo("native.id"));
            Assert.That(results[1].Errors.Single().Error, Is.EqualTo("native text"));
        }

        [Test]
        public async Task Set_DefaultsAndEmptyAdminListRemainDistinct()
        {
            Assert.That(MattermostApiLimits.DefaultChannelMemberBatchSize, Is.EqualTo(100));
            Assert.That(MattermostApiLimits.DefaultChannelMemberBatchDelayMilliseconds, Is.EqualTo(500));
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?batch_size=100&batch_delay_ms=500"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("members").GetArrayLength(), Is.Zero);
                Assert.That(body.RootElement.GetProperty("channel_admins").GetArrayLength(), Is.Zero);
                return ApiTestHttp.Json(HttpStatusCode.OK, "");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.SetChannelMembersAsync("channel-1", Array.Empty<string>(), Array.Empty<string>()), Is.Empty);
        }

        [Test]
        public async Task Set_CopiesCollectionsBeforeAuthentication()
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("members")[0].GetString(), Is.EqualTo("user-1"));
                Assert.That(body.RootElement.GetProperty("channel_admins")[0].GetString(), Is.EqualTo("user-2"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"added\":[],\"removed\":[]}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            List<string> members = new List<string> { "user-1" };
            List<string> admins = new List<string> { "user-2" };
            Task<IList<ChannelMembersUpdateResult>> pending = client.SetChannelMembersAsync("channel-1", members, admins);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            members.Clear();
            admins.Clear();
            release.SetResult();
            Assert.That(await pending.WaitAsync(TimeSpan.FromSeconds(5)), Has.Count.EqualTo(1));
        }

        [TestCase(1, 0)]
        [TestCase(1000, 10000)]
        public async Task Set_AcceptsNativeBoundaryValues(int size, int delay)
        {
            Assert.That(MattermostApiLimits.MaxChannelMemberBatchSize, Is.EqualTo(1000));
            Assert.That(MattermostApiLimits.MaxChannelMemberBatchDelayMilliseconds, Is.EqualTo(10000));
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"added\":[],\"removed\":[]}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.SetChannelMembersAsync("channel-1", new[] { "user-1" }, batchSize: size, batchDelayMilliseconds: delay), Has.Count.EqualTo(1));
        }

        [TestCase(0, 500, "batchSize")]
        [TestCase(-1, 500, "batchSize")]
        [TestCase(1001, 500, "batchSize")]
        [TestCase(100, -1, "batchDelayMilliseconds")]
        [TestCase(100, 10001, "batchDelayMilliseconds")]
        public async Task Set_RejectsNativeLimitViolations(int size, int delay, string parameter)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentOutOfRangeException? error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.SetChannelMembersAsync("channel-1", new[] { "user-1" }, batchSize: size, batchDelayMilliseconds: delay));
            Assert.That(error!.ParamName, Is.EqualTo(parameter));
        }

        [Test]
        public async Task Set_RejectsBlankChannel([Values(null, "", " \t ")] string? channel)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SetChannelMembersAsync(channel!, Array.Empty<string>()));
        }

        [Test]
        public async Task Set_RejectsNullMembers()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentNullException? error = await Assert.ThrowsAsync<ArgumentNullException>(() => client.SetChannelMembersAsync("channel-1", null!));
            Assert.That(error!.ParamName, Is.EqualTo("members"));
        }

        [Test]
        public async Task Set_RejectsBlankCollectionItems([Values(false, true)] bool admin,
            [Values(null, "", " \t ")] string? identifier)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(() => client.SetChannelMembersAsync("channel-1",
                admin ? Array.Empty<string>() : new[] { identifier! }, admin ? new[] { identifier! } : null));
            Assert.That(error!.ParamName, Is.EqualTo(admin ? "channelAdmins" : "members"));
        }

        [Test]
        public async Task Set_PreservesNativeHttpErrors([Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"membership update denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.SetChannelMembersAsync("channel-1", Array.Empty<string>()));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("membership update denied"));
        }

        [TestCase("invalid JSON")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{\"added\":[],\"removed\":null}")]
        [TestCase("{\"added\":[],\"removed\":[],\"errors\":null}")]
        [TestCase("{\"added\":[],\"removed\":[],\"promoted\":null}")]
        [TestCase("{\"added\":[],\"removed\":[],\"demoted\":null}")]
        [TestCase("{\"error\":null}")]
        [TestCase("{\"error\":\"\"}")]
        public async Task Set_RejectsMalformedBatch(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => client.SetChannelMembersAsync("channel-1", Array.Empty<string>()));
        }

        [Test]
        public async Task Set_PreservesTerminalErrorEvenWithHttpSuccess()
        {
            const string response = "{\"added\":[\"user-1\"],\"removed\":[]}\n{\"error\":\"operation interrupted\"}\n";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.SetChannelMembersAsync("channel-1", new[] { "user-1" }));
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(error.Message, Is.EqualTo("operation interrupted"));
            Assert.That(error.ResponseJson, Is.EqualTo(response));
            Assert.That(error.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(error.RequestUri, Does.Contain("/channels/channel-1/members?"));
        }

        [TestCase("")]
        [TestCase(" \r\n \n")]
        [TestCase("\r\n{\"added\":[],\"removed\":[]}\r\n\r\n")]
        public async Task Set_AcceptsEmptyResponseAndNativeLineEndings(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<ChannelMembersUpdateResult> results = await client.SetChannelMembersAsync("channel-1", Array.Empty<string>(), Array.Empty<string>());
            Assert.That(results.Count, Is.EqualTo(response.Contains("added", StringComparison.Ordinal) ? 1 : 0));
        }

        [Test]
        public async Task Set_RejectsCanceledAndDisposedClient()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SetChannelMembersAsync("channel-1", Array.Empty<string>(), cancellationToken: cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.SetChannelMembersAsync("channel-1", Array.Empty<string>()));
        }

        [Test]
        public async Task Set_PassesRequestCancellation()
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Expected request cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => client.SetChannelMembersAsync("channel-1", Array.Empty<string>(), cancellationToken: cancellation.Token));
        }
    }
}
