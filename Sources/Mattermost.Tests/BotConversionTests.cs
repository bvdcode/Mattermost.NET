using Mattermost.Exceptions;
using Mattermost.Models.Bots;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class BotConversionTests
    {
        private const string BotJson = "{\"user_id\":\"converted-user\",\"owner_id\":\"converted-user\",\"username\":\"converted\",\"display_name\":\"Display\",\"description\":\"Description\",\"create_at\":5000000000,\"update_at\":6000000000,\"delete_at\":0}";
        private const string UserJson = "{\"id\":\"converted-user\",\"username\":\"converted\",\"email\":\"converted@example.org\",\"roles\":\"system_user\",\"create_at\":5000000000}";

        [Test]
        public async Task UserToBot_UsesBodylessPostAndNativeBotResponse()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1/convert_to_bot"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, BotJson));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Bot bot = await client.ConvertUserToBotAsync(" user/1 ");
            Assert.Multiple(() =>
            {
                Assert.That(bot.UserId, Is.EqualTo("converted-user"));
                Assert.That(bot.OwnerId, Is.EqualTo("converted-user"));
                Assert.That(bot.Username, Is.EqualTo("converted"));
                Assert.That(bot.DisplayName, Is.EqualTo("Display"));
                Assert.That(bot.CreatedAt, Is.EqualTo(5000000000));
                Assert.That(bot.UpdatedAt, Is.EqualTo(6000000000));
            });
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public async Task BotToUser_UsesNativePayloadAndRoleOption(bool admin, bool replaceEmail)
        {
            string? email = replaceEmail ? " converted@example.org " : null;
            const string password = "  Correct-password-9!  ";
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/bots/bot%2F1/convert_to_user?set_system_admin="
                    + admin.ToString().ToLowerInvariant()));
                using JsonDocument payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(payload.RootElement.GetProperty("password").GetString(), Is.EqualTo(password));
                Assert.That(payload.RootElement.EnumerateObject().Count(), Is.EqualTo(replaceEmail ? 2 : 1));
                if (replaceEmail) { Assert.That(payload.RootElement.GetProperty("email").GetString(), Is.EqualTo("converted@example.org")); }
                else { Assert.That(payload.RootElement.TryGetProperty("email", out _), Is.False); }
                return ApiTestHttp.Json(HttpStatusCode.OK, UserJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            User user = await client.ConvertBotToUserAsync(" bot/1 ", password, email, admin);
            Assert.That(user.Id, Is.EqualTo("converted-user"));
            Assert.That(user.Username, Is.EqualTo("converted"));
            Assert.That(user.Email, Is.EqualTo("converted@example.org"));
        }

        [TestCase(false, HttpStatusCode.BadRequest)]
        [TestCase(false, HttpStatusCode.Forbidden)]
        [TestCase(false, HttpStatusCode.NotFound)]
        [TestCase(false, HttpStatusCode.InternalServerError)]
        [TestCase(true, HttpStatusCode.BadRequest)]
        [TestCase(true, HttpStatusCode.Forbidden)]
        [TestCase(true, HttpStatusCode.NotFound)]
        [TestCase(true, HttpStatusCode.InternalServerError)]
        public async Task Conversions_PreserveNativeErrors(bool toUser, HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status,
                "{\"id\":\"conversion_failed\",\"message\":\"Conversion failed\",\"status_code\":" + (int)status + "}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(async () => await InvokeAsync(client, toUser));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
        }

        [TestCase(false, "null")]
        [TestCase(false, "[]")]
        [TestCase(false, "{invalid")]
        [TestCase(true, "null")]
        [TestCase(true, "[]")]
        [TestCase(true, "{invalid")]
        public async Task Conversions_RejectMissingOrMalformedObjectResponses(bool toUser, string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(async () => await InvokeAsync(client, toUser));
        }

        [TestCase(false, null)]
        [TestCase(false, "")]
        [TestCase(false, " ")]
        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(true, " ")]
        public async Task Conversions_RejectInvalidIdentifiersBeforeRequests(bool toUser, string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                if (toUser) { await client.ConvertBotToUserAsync(id!, "password"); }
                else { await client.ConvertUserToBotAsync(id!); }
            });
            Assert.That(error!.ParamName, Is.EqualTo(toUser ? "botUserId" : "userId"));
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task BotToUser_RejectsMissingPasswordBeforeRequests(string? password)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(async () => await client.ConvertBotToUserAsync("bot", password!));
            Assert.That(error!.ParamName, Is.EqualTo("password"));
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task BotToUser_RejectsBlankSuppliedEmailBeforeRequests(string email)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(async () => await client.ConvertBotToUserAsync("bot", "password", email));
            Assert.That(error!.ParamName, Is.EqualTo("email"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Conversions_RejectDisposedClient(bool toUser)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await InvokeAsync(client, toUser));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Conversions_CancelBeforeAuthenticationOrRequests(bool toUser)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await InvokeAsync(client, toUser, cancellation.Token));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Conversions_CancelInFlightRequest(bool toUser)
        {
            TaskCompletionSource<bool> entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                entered.SetResult(true);
                await Task.Delay(Timeout.Infinite, token);
                throw new AssertionException("Expected cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            Task request = InvokeAsync(client, toUser, cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await request);
        }

        private static Task InvokeAsync(IMattermostUsersClient client, bool toUser, CancellationToken token = default)
        {
            if (toUser) { return client.ConvertBotToUserAsync("bot", "password", cancellationToken: token); }
            return client.ConvertUserToBotAsync("user", token);
        }
    }
}
