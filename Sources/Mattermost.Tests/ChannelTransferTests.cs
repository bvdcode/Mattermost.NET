using Mattermost.Exceptions;
using Mattermost.Models.Channels;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelTransferTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task Move_SendsDestinationAndExplicitForce(bool force)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1/move"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[] { "team_id", "force" }));
                Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/2"));
                Assert.That(body.RootElement.GetProperty("force").GetBoolean(), Is.EqualTo(force));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"channel/1\",\"team_id\":\"team/2\",\"type\":\"O\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Channel channel = await client.MoveChannelAsync(" channel/1 ", " team/2 ", force);
            Assert.That(channel.Id, Is.EqualTo("channel/1"));
            Assert.That(channel.TeamId, Is.EqualTo("team/2"));
            Assert.That(channel.Type, Is.EqualTo("O"));
        }

        [Test]
        public async Task Move_DefaultForceIsFalse()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("force").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"channel-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.MoveChannelAsync("channel-1", "team-2");
        }

        [Test]
        public async Task Conversion_SendsMatchingChannelIdentifierAndNames()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/channels/channel%2F1/convert_to_channel"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(new[] { "channel_id", "team_id", "name", "display_name" }));
                Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1"));
                Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/2"));
                Assert.That(body.RootElement.GetProperty("name").GetString(), Is.EqualTo("new-channel"));
                Assert.That(body.RootElement.GetProperty("display_name").GetString(), Is.EqualTo("Channel ✨"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"channel/1\",\"team_id\":\"team/2\",\"type\":\"P\",\"name\":\"new-channel\",\"display_name\":\"Channel ✨\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Channel channel = await client.ConvertGroupMessageToChannelAsync(" channel/1 ", " team/2 ", " new-channel ", " Channel ✨ ");
            Assert.That(channel.Id, Is.EqualTo("channel/1"));
            Assert.That(channel.TeamId, Is.EqualTo("team/2"));
            Assert.That(channel.Type, Is.EqualTo("P"));
            Assert.That(channel.Name, Is.EqualTo("new-channel"));
            Assert.That(channel.DisplayName, Is.EqualTo("Channel ✨"));
        }

        [Test]
        public async Task Identifiers_MustBeNonblank([Values(false, true)] bool conversion,
            [Values(false, true)] bool invalidTeam, [Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            string channel = "channel-1";
            string team = "team-2";
            if (invalidTeam) { team = value!; }
            else { channel = value!; }
            await Assert.CatchAsync<ArgumentException>(() => InvokeAsync(client, conversion, channel, team));
        }

        [Test]
        public async Task Conversion_NamesMustBeNonblank([Values(false, true)] bool invalidDisplayName,
            [Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            string name = "new-channel";
            string displayName = "New channel";
            if (invalidDisplayName) { displayName = value!; }
            else { name = value!; }
            await Assert.CatchAsync<ArgumentException>(() => client.ConvertGroupMessageToChannelAsync("channel-1", "team-2", name, displayName));
        }

        [Test]
        public async Task ServerErrors_ArePreserved([Values(false, true)] bool conversion,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"transfer rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, conversion));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("transfer rejected"));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
        }

        [Test]
        public async Task InvalidResponses_Throw([Values(false, true)] bool conversion, [Values("null", "[]", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, conversion));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values(false, true)] bool conversion)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, conversion, token: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, conversion));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool conversion)
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
            Task pending = InvokeAsync(client, conversion, token: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task<Channel> InvokeAsync(MattermostClient client, bool conversion, string channelId = "channel-1",
            string teamId = "team-2", CancellationToken token = default)
        {
            if (conversion) { return client.ConvertGroupMessageToChannelAsync(channelId, teamId, "new-channel", "New channel", token); }
            return client.MoveChannelAsync(channelId, teamId, cancellationToken: token);
        }
    }
}
