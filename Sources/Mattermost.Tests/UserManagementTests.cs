using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserManagementTests
    {
        [TestCase("create", "POST", "/chat/api/v4/users", "{\"username\":\"user\",\"email\":\"user@example.test\",\"password\":\" password \"}")]
        [TestCase("patch", "PUT", "/chat/api/v4/users/user%2F1/patch", "{\"nickname\":\"\"}")]
        [TestCase("deactivate", "DELETE", "/chat/api/v4/users/user%2F1", null)]
        [TestCase("active", "PUT", "/chat/api/v4/users/user%2F1/active", "{\"active\":false}")]
        [TestCase("roles", "PUT", "/chat/api/v4/users/user%2F1/roles", "{\"roles\":\"system_user\"}")]
        [TestCase("password", "PUT", "/chat/api/v4/users/user%2F1/password", "{\"new_password\":\" password \"}")]
        [TestCase("reset", "POST", "/chat/api/v4/users/user%2F1/reset_failed_attempts", null)]
        [TestCase("verify", "POST", "/chat/api/v4/users/user%2F1/email/verify/member", null)]
        [TestCase("session", "POST", "/chat/api/v4/users/user%2F1/sessions/revoke", "{\"session_id\":\"session/1\"}")]
        [TestCase("all-sessions", "POST", "/chat/api/v4/users/user%2F1/sessions/revoke/all", null)]
        public async Task Mutations_UseOnlyNativeFields(string operation, string method, string route, string? body)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                if (body is null) { Assert.That(request.Content, Is.Null); }
                else { Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(body)); }
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\",\"username\":\"user\",\"email\":\"user@example.test\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await MutateAsync(client, operation, " user/1 ");
        }

        [Test]
        public async Task UserResponses_DeserializeAndRejectNull([Values("create", "patch", "verify")] string operation)
        {
            bool nullResponse = false;
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                nullResponse ? "null" : "{\"id\":\"new-user\",\"username\":\"user\",\"email\":\"user@example.test\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            User result = await UserMutationAsync(client, operation);
            Assert.That(result.Id, Is.EqualTo("new-user"));
            Assert.That(result.Username, Is.EqualTo("user"));
            Assert.That(result.Email, Is.EqualTo("user@example.test"));
            nullResponse = true;
            await Assert.ThrowsAsync<JsonException>(() => UserMutationAsync(client, operation));
        }

        [Test]
        public async Task Patch_PreservesFieldsAndSendsCompleteMaps()
        {
            UserPatch patch = new UserPatch
            {
                Username = " user ", Email = " user@example.test ", Password = " confirmation ", Locale = " en ",
                FirstName = " First ", LastName = " Last ", Nickname = " nickname ", Position = " position ",
                Props = new Dictionary<string, string>(), NotifyProps = new Dictionary<string, string> { ["desktop"] = "mention" },
                Timezone = new Timezone { AutomaticTimezone = "UTC", ManualTimezone = "UTC", UseAutomaticTimezone = "true" }
            };
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement body = document.RootElement;
                Assert.That(body.EnumerateObject().Count(), Is.EqualTo(11));
                Assert.That(body.GetProperty("username").GetString(), Is.EqualTo("user"));
                Assert.That(body.GetProperty("email").GetString(), Is.EqualTo("user@example.test"));
                Assert.That(body.GetProperty("password").GetString(), Is.EqualTo(" confirmation "));
                Assert.That(body.GetProperty("locale").GetString(), Is.EqualTo("en"));
                Assert.That(body.GetProperty("first_name").GetString(), Is.EqualTo("First"));
                Assert.That(body.GetProperty("last_name").GetString(), Is.EqualTo("Last"));
                Assert.That(body.GetProperty("nickname").GetString(), Is.EqualTo("nickname"));
                Assert.That(body.GetProperty("position").GetString(), Is.EqualTo("position"));
                Assert.That(body.GetProperty("props").EnumerateObject(), Is.Empty);
                Assert.That(body.GetProperty("notify_props").EnumerateObject().Count(), Is.EqualTo(1));
                Assert.That(body.GetProperty("notify_props").GetProperty("desktop").GetString(), Is.EqualTo("mention"));
                Assert.That(body.GetProperty("timezone").GetProperty("automaticTimezone").GetString(), Is.EqualTo("UTC"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.PatchUserAsync("user", patch);
            Assert.That(patch.Username, Is.EqualTo(" user "));
            Assert.That(patch.FirstName, Is.EqualTo(" First "));
            Assert.That(patch.Password, Is.EqualTo(" confirmation "));
        }

        [Test]
        public async Task Patch_SnapshotsMutableFieldsBeforeAuthentication()
        {
            UserPatch patch = new UserPatch
            {
                Props = new Dictionary<string, string> { ["key"] = "original" },
                NotifyProps = new Dictionary<string, string> { ["desktop"] = "mention" },
                Timezone = new Timezone { AutomaticTimezone = "UTC", ManualTimezone = "Europe/Berlin", UseAutomaticTimezone = "true" }
            };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    patch.Props.Clear();
                    patch.NotifyProps["desktop"] = "none";
                    patch.Timezone.AutomaticTimezone = "changed";
                    patch.Timezone.ManualTimezone = "changed";
                    patch.Timezone.UseAutomaticTimezone = "false";
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(3));
                Assert.That(body.RootElement.GetProperty("props").GetProperty("key").GetString(), Is.EqualTo("original"));
                Assert.That(body.RootElement.GetProperty("notify_props").GetProperty("desktop").GetString(), Is.EqualTo("mention"));
                JsonElement timezone = body.RootElement.GetProperty("timezone");
                Assert.That(timezone.GetProperty("automaticTimezone").GetString(), Is.EqualTo("UTC"));
                Assert.That(timezone.GetProperty("manualTimezone").GetString(), Is.EqualTo("Europe/Berlin"));
                Assert.That(timezone.GetProperty("useAutomaticTimezone").GetString(), Is.EqualTo("true"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.PatchUserAsync("user", patch);
        }

        [Test]
        public async Task PasswordAndActivation_PreserveExplicitValues()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                if (request.RequestUri!.AbsolutePath.EndsWith("/password", StringComparison.Ordinal))
                {
                    Assert.That(document.RootElement.GetProperty("new_password").GetString(), Is.EqualTo(" new password "));
                    Assert.That(document.RootElement.GetProperty("current_password").GetString(), Is.EqualTo(" old password "));
                    Assert.That(document.RootElement.EnumerateObject().Count(), Is.EqualTo(2));
                }
                else { Assert.That(document.RootElement.GetProperty("active").GetBoolean(), Is.True); }
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateUserPasswordAsync("user", " new password ", " old password ");
            await client.UpdateUserActiveAsync("user", true);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task RequiredIdentifiers_RejectBeforeHttp(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in Operations.Where(item => item != "create"))
            {
                await Assert.ThrowsAsync<ArgumentException>(() => MutateAsync(client, operation, value!));
            }
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUserAsync(value!, "user@example.test", "password"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUserAsync("user", value!, "password"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateUserRolesAsync("user", value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.RevokeUserSessionAsync("user", value!));
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task Passwords_RejectEmpty(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateUserAsync("user", "user@example.test", value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateUserPasswordAsync("user", value!));
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateUserPasswordAsync("user", "password", ""));
        }

        [Test]
        public async Task Patch_RejectsMissingFieldsAndInvalidConfirmation()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.PatchUserAsync("user", null!));
            foreach (UserPatch patch in new[] { new UserPatch(), new UserPatch { Password = "password" },
                new UserPatch { Nickname = "name", Password = "password" }, new UserPatch { Email = "user@example.test", Password = "" },
                new UserPatch { Username = " " }, new UserPatch { Email = " " }, new UserPatch { Locale = " " } })
            {
                await Assert.ThrowsAsync<ArgumentException>(() => client.PatchUserAsync("user", patch));
            }
        }

        [Test]
        public async Task Errors_PreserveServerDetails([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            const string json = "{\"id\":\"api.user.error\",\"message\":\"not allowed\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => MutateAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
            Assert.That(error.Message, Is.EqualTo("not allowed"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => MutateAsync(client, operation, token: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => MutateAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([ValueSource(nameof(Operations))] string operation)
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
            Task pending = MutateAsync(client, operation, token: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static readonly string[] Operations = { "create", "patch", "deactivate", "active", "roles", "password", "reset", "verify", "session", "all-sessions" };

        private static Task<User> UserMutationAsync(MattermostClient client, string operation)
        {
            switch (operation)
            {
                case "create": return client.CreateUserAsync("user", "user@example.test", "password");
                case "patch": return client.PatchUserAsync("user", new UserPatch { Nickname = "" });
                case "verify": return client.VerifyUserEmailAsync("user");
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task MutateAsync(MattermostClient client, string operation, string userId = "user", CancellationToken token = default)
        {
            switch (operation)
            {
                case "create": return client.CreateUserAsync(" user ", " user@example.test ", " password ", token);
                case "patch": return client.PatchUserAsync(userId, new UserPatch { Nickname = "" }, token);
                case "deactivate": return client.DeactivateUserAsync(userId, token);
                case "active": return client.UpdateUserActiveAsync(userId, false, token);
                case "roles": return client.UpdateUserRolesAsync(userId, " system_user ", token);
                case "password": return client.UpdateUserPasswordAsync(userId, " password ", cancellationToken: token);
                case "reset": return client.ResetUserFailedAttemptsAsync(userId, token);
                case "verify": return client.VerifyUserEmailAsync(userId, token);
                case "session": return client.RevokeUserSessionAsync(userId, " session/1 ", token);
                case "all-sessions": return client.RevokeAllUserSessionsAsync(userId, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
