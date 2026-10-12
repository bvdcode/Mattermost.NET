using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserProfileUpdateTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task UpdateUser_SendsCompleteEditableProfile(bool marketing)
        {
            UserProfileUpdate input = Profile();
            input.Username = " account "; input.Email = " account@example.invalid "; input.Locale = " en ";
            input.Nickname = " nickname "; input.FirstName = " first "; input.LastName = " last "; input.Position = " position ";
            input.Password = " confirmation "; input.AllowMarketing = marketing;
            input.Timezone = new Timezone { AutomaticTimezone = "UTC", ManualTimezone = "Europe/Berlin", UseAutomaticTimezone = "false" };
            input.Props = new Dictionary<string, string> { ["key"] = "value" };
            input.NotifyProps = new Dictionary<string, string> { ["comments"] = "never", ["desktop"] = "mention" };
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3Fx%23y"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(item => item.Name), Is.EquivalentTo(new[]
                {
                    "id", "username", "email", "locale", "nickname", "first_name", "last_name", "position", "password", "timezone", "props", "notify_props", "allow_marketing"
                }));
                Assert.That(body.RootElement.GetProperty("id").GetString(), Is.EqualTo("user/1?x#y"));
                Assert.That(body.RootElement.GetProperty("username").GetString(), Is.EqualTo("account"));
                Assert.That(body.RootElement.GetProperty("email").GetString(), Is.EqualTo("account@example.invalid"));
                Assert.That(body.RootElement.GetProperty("locale").GetString(), Is.EqualTo("en"));
                Assert.That(body.RootElement.GetProperty("nickname").GetString(), Is.EqualTo("nickname"));
                Assert.That(body.RootElement.GetProperty("first_name").GetString(), Is.EqualTo("first"));
                Assert.That(body.RootElement.GetProperty("last_name").GetString(), Is.EqualTo("last"));
                Assert.That(body.RootElement.GetProperty("position").GetString(), Is.EqualTo("position"));
                Assert.That(body.RootElement.GetProperty("password").GetString(), Is.EqualTo(" confirmation "));
                Assert.That(body.RootElement.GetProperty("allow_marketing").GetBoolean(), Is.EqualTo(marketing));
                Assert.That(body.RootElement.GetProperty("timezone").GetProperty("manualTimezone").GetString(), Is.EqualTo("Europe/Berlin"));
                Assert.That(body.RootElement.GetProperty("props").GetProperty("key").GetString(), Is.EqualTo("value"));
                Assert.That(body.RootElement.GetProperty("notify_props").GetProperty("comments").GetString(), Is.EqualTo("never"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\",\"username\":\"account\",\"email\":\"account@example.invalid\",\"locale\":\"en\",\"nickname\":\"nickname\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IMattermostUsersClient api = client;
            User result = await api.UpdateUserAsync(" user/1?x#y ", input);
            Assert.That(result.Id, Is.EqualTo("user-1"));
            Assert.That(result.Username, Is.EqualTo("account"));
            Assert.That(result.Email, Is.EqualTo("account@example.invalid"));
            Assert.That(result.Nickname, Is.EqualTo("nickname"));
            Assert.That(input.Username, Is.EqualTo(" account "));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task UpdateUser_UnspecifiedTextClearsAndCollectionsAreSent(bool empty)
        {
            UserProfileUpdate input = Profile();
            if (empty) { input.Props = new Dictionary<string, string>(); input.NotifyProps = new Dictionary<string, string>(); }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                foreach (string field in new[] { "nickname", "first_name", "last_name", "position" })
                {
                    Assert.That(body.RootElement.GetProperty(field).GetString(), Is.Empty);
                }
                Assert.That(body.RootElement.GetProperty("timezone").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(body.RootElement.GetProperty("password").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(body.RootElement.GetProperty("allow_marketing").GetBoolean(), Is.False);
                JsonValueKind expected = JsonValueKind.Null;
                if (empty) { expected = JsonValueKind.Object; }
                Assert.That(body.RootElement.GetProperty("props").ValueKind, Is.EqualTo(expected));
                Assert.That(body.RootElement.GetProperty("notify_props").ValueKind, Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateUserAsync("user-1", input);
        }

        [Test]
        public async Task UpdateUser_SnapshotsInputBeforeAuthentication()
        {
            UserProfileUpdate input = Profile();
            input.Timezone = new Timezone { ManualTimezone = "UTC" };
            input.Props = new Dictionary<string, string> { ["key"] = "original" };
            input.NotifyProps = new Dictionary<string, string> { ["desktop"] = "none" };
            input.AllowMarketing = true;
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    input.Username = "changed"; input.Locale = "changed"; input.AllowMarketing = false;
                    input.Timezone.ManualTimezone = "changed"; input.Props.Clear(); input.NotifyProps.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("username").GetString(), Is.EqualTo("account"));
                Assert.That(body.RootElement.GetProperty("locale").GetString(), Is.EqualTo("en"));
                Assert.That(body.RootElement.GetProperty("allow_marketing").GetBoolean(), Is.True);
                Assert.That(body.RootElement.GetProperty("timezone").GetProperty("manualTimezone").GetString(), Is.EqualTo("UTC"));
                Assert.That(body.RootElement.GetProperty("props").GetProperty("key").GetString(), Is.EqualTo("original"));
                Assert.That(body.RootElement.GetProperty("notify_props").GetProperty("desktop").GetString(), Is.EqualTo("none"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateUserAsync("user-1", input);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        [TestCase(" me ")]
        public void UpdateUser_InvalidIdentifierIsRejected(string? userId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => client.UpdateUserAsync(userId!, Profile()))!;
            Assert.That(error.ParamName, Is.EqualTo("userId"));
        }

        [TestCase("Username", null)]
        [TestCase("Username", "")]
        [TestCase("Username", " \t")]
        [TestCase("Email", null)]
        [TestCase("Email", "")]
        [TestCase("Email", " \t")]
        [TestCase("Locale", null)]
        [TestCase("Locale", "")]
        [TestCase("Locale", " \t")]
        public void UpdateUser_MissingRequiredFieldIsRejected(string field, string? value)
        {
            UserProfileUpdate input = Profile();
            switch (field)
            {
                case "Username": input.Username = value!; break;
                case "Email": input.Email = value!; break;
                case "Locale": input.Locale = value!; break;
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => client.UpdateUserAsync("user-1", input))!;
            Assert.That(error.ParamName, Is.EqualTo(field));
        }

        [Test]
        public void UpdateUser_NullProfileAndEmptyConfirmationPasswordAreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentNullException>(() => client.UpdateUserAsync("user-1", null!));
            UserProfileUpdate input = Profile(); input.Password = string.Empty;
            Assert.Throws<ArgumentException>(() => client.UpdateUserAsync("user-1", input));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Conflict)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task UpdateUser_NativeErrorIsPropagatedWithoutRetry(HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot update profile\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.UpdateUserAsync("user-1", Profile())))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot update profile"));
            Assert.That(error.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{broken")]
        public async Task UpdateUser_InvalidResponseIsRejected(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => client.UpdateUserAsync("user-1", Profile()));
        }

        [Test]
        public void UpdateUser_CancelledAndDisposedCallsSendNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.UpdateUserAsync("user-1", Profile(), cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.UpdateUserAsync("user-1", Profile()));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task UpdateUser_CancellationStopsAuthenticationOrRequest(bool authentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("Expected cancellation.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task<User> pending = client.UpdateUserAsync("user-1", Profile(), cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static UserProfileUpdate Profile() => new UserProfileUpdate { Username = "account", Email = "account@example.invalid", Locale = "en" };
    }
}
