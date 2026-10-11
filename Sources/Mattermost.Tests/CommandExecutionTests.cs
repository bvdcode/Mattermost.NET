using Mattermost.Exceptions;
using Mattermost.Models.SlashCommands;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class CommandExecutionTests
    {
        [Test]
        public async Task Execute_UsesNativeRouteAndPreservesArguments()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/commands/execute"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name),
                    Is.EquivalentTo(new[] { "channel_id", "command", "team_id", "root_id" }));
                Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1"));
                Assert.That(body.RootElement.GetProperty("command").GetString(), Is.EqualTo("/hello  alice ✨ "));
                Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/1"));
                Assert.That(body.RootElement.GetProperty("root_id").GetString(), Is.EqualTo("root/1"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"response_type\":\"\",\"trigger_id\":\"trigger\",\"goto_location\":\"/help\",\"extra_responses\":[{\"response_type\":\"\",\"text\":\"extra\"}]}" );
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            SlashCommandResponse result = await client.ExecuteCommandAsync(" channel/1 ", "/hello  alice ✨ ", " team/1 ", " root/1 ");
            Assert.That(result.ResponseType, Is.Null);
            Assert.That(result.TriggerId, Is.EqualTo("trigger"));
            Assert.That(result.GotoLocation, Is.EqualTo("/help"));
            Assert.That(result.ExtraResponses!.Single().ResponseType, Is.Null);
        }

        [Test]
        public async Task Execute_OmitsUnspecifiedContexts()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{\"channel_id\":\"channel\",\"command\":\"/help\"}"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.ExecuteCommandAsync("channel", "/help");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("hello")]
        [TestCase("/")]
        [TestCase(" /hello")]
        public async Task Execute_RejectsInvalidCommandBeforeHttp(string? command)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteCommandAsync("channel", command!));
        }

        [Test]
        public async Task Execute_RejectsBlankIdentifiersBeforeHttp([Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteCommandAsync(value!, "/help"));
            if (value is not null)
            {
                await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteCommandAsync("channel", "/help", teamId: value));
                await Assert.ThrowsAsync<ArgumentException>(() => client.ExecuteCommandAsync("channel", "/help", rootId: value));
            }
        }

        [Test]
        public async Task Execute_PropagatesServerErrors([Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.ExecuteCommandAsync("channel", "/help"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [TestCase("null")]
        [TestCase("invalid JSON")]
        public async Task Execute_RejectsInvalidResponse(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.ExecuteCommandAsync("channel", "/help"));
        }

        [Test]
        public async Task Execute_CancellationAndDisposalRejectBeforeHttp()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => client.ExecuteCommandAsync("channel", "/help", cancellationToken: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.ExecuteCommandAsync("channel", "/help"));
        }

        [Test]
        public async Task Execute_InFlightCancellationReachesHttp()
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
            Task pending = client.ExecuteCommandAsync("channel", "/help", cancellationToken: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        [TestCase(SlashCommandResponseType.Ephemeral)]
        [TestCase(SlashCommandResponseType.InChannel)]
        [TestCase((SlashCommandResponseType)7)]
        public void OutgoingVisibility_MatchesOriginalStringEnumSerialization(SlashCommandResponseType visibility)
        {
            JsonSerializerOptions original = new JsonSerializerOptions();
            original.Converters.Add(new JsonStringEnumConverter());
            string expected = JsonSerializer.Serialize(visibility, original);
            using JsonDocument response = JsonDocument.Parse(JsonSerializer.Serialize(new SlashCommandResponse
                { Text = "Done", ResponseType = visibility, ExtraResponses = new[] { new SlashCommandResponseItem { ResponseType = visibility } } }));
            Assert.That(response.RootElement.GetProperty("response_type").GetRawText(), Is.EqualTo(expected));
            Assert.That(response.RootElement.GetProperty("extra_responses")[0].GetProperty("response_type").GetRawText(), Is.EqualTo(expected));
            Assert.That(response.RootElement.TryGetProperty("trigger_id", out _), Is.False);
        }

        [TestCase("\"ephemeral\"")]
        [TestCase("\"in_channel\"")]
        [TestCase("null")]
        [TestCase("0")]
        [TestCase("7")]
        public void IncomingVisibility_MatchesOriginalEnumBehavior(string json)
        {
            JsonSerializerOptions original = new JsonSerializerOptions();
            original.Converters.Add(new JsonStringEnumConverter());
            SlashCommandResponseType? expected = JsonSerializer.Deserialize<SlashCommandResponseType?>(json, original);
            SlashCommandResponse result = JsonSerializer.Deserialize<SlashCommandResponse>("{\"response_type\":" + json + "}")!;
            Assert.That(result.ResponseType, Is.EqualTo(expected));
        }

        [TestCase("\"unexpected\"")]
        [TestCase("\"EPHEMERAL\"")]
        [TestCase("\" \"")]
        [TestCase("true")]
        [TestCase("{}")]
        public void InvalidVisibility_IsStillRejected(string json)
        {
            JsonSerializerOptions original = new JsonSerializerOptions();
            original.Converters.Add(new JsonStringEnumConverter());
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SlashCommandResponseType?>(json, original));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SlashCommandResponse>("{\"response_type\":" + json + "}"));
        }

        [Test]
        public void UnsetFields_AreStillOmitted()
        {
            Assert.That(JsonSerializer.Serialize(new SlashCommandResponse { Text = "Done" }), Is.EqualTo("{\"text\":\"Done\"}"));
            Assert.That(JsonSerializer.Deserialize<SlashCommandResponse>("{\"response_type\":\"\"}")!.ResponseType, Is.Null);
        }
    }
}
