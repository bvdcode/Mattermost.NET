using Mattermost.Models.SlashCommands;
using System.Net;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class CommandValidationTests
    {
        [Test]
        public async Task BlankIdentifiers_AreRejected([Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandsAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.DeleteCommandAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.MoveCommandAsync(value!, "team"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.MoveCommandAsync("command", value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.RegenerateCommandTokenAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAutocompleteCommandsAsync(value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandAutocompleteSuggestionsAsync(value!, "/"));
        }

        [Test]
        public async Task Suggestions_RejectBlankContext([Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandAutocompleteSuggestionsAsync("team", value!));
            if (value is not null)
            {
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandAutocompleteSuggestionsAsync("team", "/", channelId: value));
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetCommandAutocompleteSuggestionsAsync("team", "/", rootId: value));
            }
        }

        [TestCase("team", " ")]
        [TestCase("trigger", "")]
        [TestCase("trigger", "/hello")]
        [TestCase("url", "/relative")]
        [TestCase("url", "ftp://example.invalid")]
        [TestCase("url", " ")]
        [TestCase("method", "POST")]
        [TestCase("method", "")]
        public async Task Writes_RejectInvalidRequiredSettings(string field, string value)
        {
            SlashCommand command = new SlashCommand { Id = "command", TeamId = "team", Trigger = "hello", Url = "https://example.invalid/hello" };
            switch (field)
            {
                case "team": command.TeamId = value; break;
                case "trigger": command.Trigger = value; break;
                case "url": command.Url = value; break;
                case "method": command.Method = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateCommandAsync(command));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateCommandAsync(command));
        }

        [Test]
        public async Task NullAndMissingIdentity_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateCommandAsync(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.UpdateCommandAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateCommandAsync(new SlashCommand
                { TeamId = "team", Trigger = "hello", Url = "https://example.invalid/hello" }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateCommandAsync(new SlashCommand
                { TeamId = "team", Trigger = "hello", Url = "https://example.invalid/hello", CreatorId = " " }));
        }

        [Test]
        public async Task GetMethodAndDefaultCreator_AreSupported()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                string body = await request.Content!.ReadAsStringAsync(token);
                Assert.That(body, Does.Contain("\"method\":\"G\""));
                Assert.That(body, Does.Not.Contain("creator_id"));
                return ApiTestHttp.Json(HttpStatusCode.Created, "{\"id\":\"command\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.CreateCommandAsync(new SlashCommand { TeamId = "team", Trigger = "hello", Method = "G", Url = "http://example.invalid/hello" });
        }
    }
}
