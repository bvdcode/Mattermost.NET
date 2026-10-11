using Mattermost.Exceptions;
using Mattermost.Models.SlashCommands;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class CommandTests
    {
        [TestCase("list", "GET", "/commands?team_id=team%2F1&custom_only=false")]
        [TestCase("get", "GET", "/commands/command%2F1%3F%23")]
        [TestCase("create", "POST", "/commands")]
        [TestCase("update", "PUT", "/commands/command%2F1%3F%23")]
        [TestCase("delete", "DELETE", "/commands/command%2F1%3F%23")]
        [TestCase("move", "PUT", "/commands/command%2F1%3F%23/move")]
        [TestCase("token", "PUT", "/commands/command%2F1%3F%23/regen_token")]
        [TestCase("autocomplete", "GET", "/teams/team%2F1/commands/autocomplete")]
        [TestCase("suggestions", "GET", "/teams/team%2F1/commands/autocomplete_suggestions?user_input=%2Fhello%20")]
        public async Task Requests_UseNativeMethodsAndEscapedRoutes(string operation, string method, string path)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                switch (operation)
                {
                    case "create": case "update":
                        Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                        using (JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token)))
                        {
                            Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/1"));
                            Assert.That(body.RootElement.GetProperty("trigger").GetString(), Is.EqualTo("hello"));
                            Assert.That(body.RootElement.GetProperty("method").GetString(), Is.EqualTo("P"));
                            Assert.That(body.RootElement.GetProperty("auto_complete").GetBoolean(), Is.True);
                            Assert.That(body.RootElement.TryGetProperty("token", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("create_at", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("update_at", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("delete_at", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("plugin_id", out _), Is.False);
                            Assert.That(body.RootElement.TryGetProperty("autocomplete_data", out _), Is.False);
                            if (operation == "create")
                            {
                                Assert.That(body.RootElement.TryGetProperty("id", out _), Is.False);
                                Assert.That(body.RootElement.GetProperty("creator_id").GetString(), Is.EqualTo("creator"));
                            }
                            else
                            {
                                Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("command/1?#"));
                                Assert.That(body.RootElement.TryGetProperty("creator_id", out _), Is.False);
                            }
                        }
                        break;
                    case "move":
                        Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{\"team_id\":\"destination/1\"}"));
                        break;
                    default: Assert.That(request.Content, Is.Null); break;
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task CustomOnly_IsExplicitlySent()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?team_id=team%26%2B&custom_only=true"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.GetCommandsAsync(" team&+ ", customOnly: true);
        }

        [Test]
        public async Task Suggestions_KeepTrailingSpaceAndEncodeThreadContext()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query,
                    Is.EqualTo("?user_input=%2Fhello%20a%26%2B%20&channel_id=channel%2F1&root_id=root%3F1"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                    "[{\"Complete\":\"/hello alice\",\"Suggestion\":\"alice\",\"Hint\":\"[name]\",\"Description\":\"Help ✨\",\"IconData\":\"svg\"}]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            CommandAutocompleteSuggestion result = (await client.GetCommandAutocompleteSuggestionsAsync(
                "team", "/hello a&+ ", " channel/1 ", " root?1 ")).Single();
            Assert.That(result.Complete, Is.EqualTo("/hello alice"));
            Assert.That(result.Suggestion, Is.EqualTo("alice"));
            Assert.That(result.Hint, Is.EqualTo("[name]"));
            Assert.That(result.Description, Is.EqualTo("Help ✨"));
            Assert.That(result.IconData, Is.EqualTo("svg"));
        }

        [TestCase("create")]
        [TestCase("update")]
        public async Task Writes_SnapshotSettingsBeforeAuthentication(string operation)
        {
            SlashCommand command = Command();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    command.Trigger = "changed";
                    command.TeamId = "changed";
                    command.Description = "changed";
                    command.CreatorId = "changed";
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("trigger").GetString(), Is.EqualTo("hello"));
                Assert.That(body.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team/1"));
                Assert.That(body.RootElement.GetProperty("description").GetString(), Is.EqualTo("Description"));
                if (operation == "create") { Assert.That(body.RootElement.GetProperty("creator_id").GetString(), Is.EqualTo("creator")); }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "create": await client.CreateCommandAsync(command); break;
                case "update": await client.UpdateCommandAsync(command); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task NativeModel_HandlesHiddenStringsAndPluginMetadata()
        {
            string json = "{\"id\":\"command\",\"token\":\"\",\"method\":\"\",\"url\":\"\",\"creator_id\":\"\",\"create_at\":1790000000123,\"update_at\":1790000000456,\"delete_at\":0,\"plugin_id\":\"plugin\",\"autocomplete_icon_data\":\"svg\",\"autocomplete_data\":{\"Trigger\":\"hello\",\"Hint\":\"[name]\",\"HelpText\":\"Help\",\"RoleID\":\"system_user\",\"Arguments\":[{\"Name\":\"\",\"HelpText\":\"Name\",\"Type\":\"TextInput\",\"Required\":true,\"Data\":{\"Hint\":\"[name]\",\"Pattern\":\".*\"}}],\"SubCommands\":[]}}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            SlashCommand result = await client.GetCommandAsync("command");
            Assert.That(result.Method, Is.Empty);
            Assert.That(result.Token, Is.Empty);
            Assert.That(result.CreatedAt, Is.EqualTo(1790000000123L));
            Assert.That(result.UpdatedAt, Is.EqualTo(1790000000456L));
            Assert.That(result.PluginId, Is.EqualTo("plugin"));
            Assert.That(result.AutocompleteIconData, Is.EqualTo("svg"));
            Assert.That(result.AutocompleteData!.RoleId, Is.EqualTo("system_user"));
            CommandAutocompleteArgument argument = result.AutocompleteData.Arguments!.Single();
            Assert.That(argument.Type, Is.EqualTo("TextInput"));
            Assert.That(argument.Required, Is.True);
            Assert.That(argument.Data!.Value.GetProperty("Hint").GetString(), Is.EqualTo("[name]"));
        }

        [Test]
        public async Task TokenRegeneration_ReturnsSecret()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"token\":\"new-token\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.RegenerateCommandTokenAsync("command"), Is.EqualTo("new-token"));
        }

        [TestCase("{}")]
        [TestCase("{\"token\":null}")]
        [TestCase("{\"token\":\"\"}")]
        [TestCase("{\"token\":\" \"}")]
        [TestCase("{\"token\":123}")]
        public async Task TokenRegeneration_RejectsMissingOrInvalidSecret(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.RegenerateCommandTokenAsync("command"));
        }

        [Test]
        public async Task NullLists_AreEmpty([Values("list", "autocomplete", "suggestions")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            switch (operation)
            {
                case "list": Assert.That(await client.GetCommandsAsync("team"), Is.Empty); break;
                case "autocomplete": Assert.That(await client.GetAutocompleteCommandsAsync("team"), Is.Empty); break;
                case "suggestions": Assert.That(await client.GetCommandAutocompleteSuggestionsAsync("team", "/"), Is.Empty); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        [Test]
        public async Task Errors_PreserveDetails([Values("list", "get", "create", "update", "delete", "move", "token", "autocomplete", "suggestions")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedJson_IsRejected([Values("list", "get", "create", "update", "token", "autocomplete", "suggestions")] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "invalid JSON")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("list", "get", "create", "update", "delete", "move", "token", "autocomplete", "suggestions")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp([Values("list", "get", "create", "update", "delete", "move", "token", "autocomplete", "suggestions")] string operation)
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

        private static SlashCommand Command() => new SlashCommand
        {
            Id = " command/1?# ", TeamId = " team/1 ", Trigger = " hello ", Method = "P", Url = " https://example.invalid/hello ",
            CreatorId = " creator ", Token = "ignored", CreatedAt = 1, UpdatedAt = 2, DeletedAt = 3,
            PluginId = "ignored", AutocompleteData = new CommandAutocompleteData { Trigger = "ignored" },
            AutoComplete = true, AutoCompleteDescription = "Say hello", AutoCompleteHint = "[name]", Description = "Description"
        };

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "list": case "autocomplete": case "suggestions": return "[]";
                case "get": case "create": case "update": return "{\"id\":\"command\",\"team_id\":\"team\"}";
                case "token": return "{\"token\":\"new-token\"}";
                case "delete": case "move": return "{\"status\":\"OK\"}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetCommandsAsync(" team/1 ", cancellationToken: token);
                case "get": return client.GetCommandAsync(" command/1?# ", token);
                case "create": return client.CreateCommandAsync(Command(), token);
                case "update": return client.UpdateCommandAsync(Command(), token);
                case "delete": return client.DeleteCommandAsync(" command/1?# ", token);
                case "move": return client.MoveCommandAsync(" command/1?# ", " destination/1 ", token);
                case "token": return client.RegenerateCommandTokenAsync(" command/1?# ", token);
                case "autocomplete": return client.GetAutocompleteCommandsAsync(" team/1 ", token);
                case "suggestions": return client.GetCommandAutocompleteSuggestionsAsync(" team/1 ", "/hello ", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
