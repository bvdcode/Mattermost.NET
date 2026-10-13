using Mattermost.Exceptions;
using Mattermost.Models.Roles;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class AccountOperationTests
    {
        private const string RoleJson = "{\"id\":\"role\",\"name\":\"channel_user\",\"permissions\":[\"read_channel\"]}";

        [TestCase("role", "PUT", "/roles/role%2F1/patch", "{\"permissions\":[\"read_channel\",\"read_channel\"]}")]
        [TestCase("invite", "POST", "/teams/team%2F1/invite/email", "{\"emails\":[\"Person@example.invalid\",\"second@example.invalid\"]}")]
        [TestCase("reset", "POST", "/users/password/reset/send", "{\"email\":\"Person@example.invalid\"}")]
        [TestCase("verify", "POST", "/users/email/verify/send?r=%2Fteam%3Fa%3D1%26b%3D2", "{\"email\":\"Person@example.invalid\"}")]
        public async Task Requests_UseNativeFieldsAndAuthentication(string operation, string method, string route, string json)
        {
            using HttpClient http = CreateHttp(operation, async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(json));
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Role_EmptyClearsAndResponseIsTyped()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo("{\"permissions\":[]}"));
                return ApiTestHttp.Json(HttpStatusCode.OK, RoleJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Role role = await client.UpdateRolePermissionsAsync("role", Array.Empty<string>());
            Assert.That(role.Id, Is.EqualTo("role"));
            Assert.That(role.Permissions, Is.EqualTo(new[] { "read_channel" }));
        }

        [TestCase("role")]
        [TestCase("invite")]
        public async Task Collections_AreCopiedBeforeAuthentication(string operation)
        {
            List<string> input = new List<string> { operation == "role" ? " read_channel " : " person@example.invalid " };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    input.Clear();
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                JsonElement values = body.RootElement.GetProperty(operation == "role" ? "permissions" : "emails");
                Assert.That(values.GetArrayLength(), Is.EqualTo(1));
                Assert.That(values[0].GetString(), Is.EqualTo(operation == "role" ? "read_channel" : "person@example.invalid"));
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            if (operation == "role") { await client.UpdateRolePermissionsAsync("role", input); }
            else { await client.InviteUsersToTeamAsync("team", input); }
        }

        [TestCase(null, "/users/email/verify/send")]
        [TestCase("", "/users/email/verify/send?r=")]
        [TestCase(" /team?x=1#section ", "/users/email/verify/send?r=%2Fteam%3Fx%3D1%23section")]
        public async Task VerificationRedirect_DistinguishesOmittedAndEmpty(string? redirect, string route)
        {
            using HttpClient http = CreateHttp("verify", (request, _) =>
            {
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = new MattermostClient("https://mattermost.example/chat", http);
            await client.SendVerificationEmailAsync("person@example.invalid", redirect);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void InvalidArguments_RejectBeforeHttp(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.SendPasswordResetEmailAsync(value!));
            Assert.Throws<ArgumentException>(() => client.SendVerificationEmailAsync(value!));
            Assert.Throws<ArgumentException>(() => client.UpdateRolePermissionsAsync(value!, new[] { "read_channel" }));
            Assert.Throws<ArgumentException>(() => client.UpdateRolePermissionsAsync("role", new[] { value! }));
            Assert.Throws<ArgumentException>(() => client.InviteUsersToTeamAsync(value!, new[] { "person@example.invalid" }));
            Assert.Throws<ArgumentException>(() => client.InviteUsersToTeamAsync("team", new[] { value! }));
        }

        [Test]
        public void NullAndEmptyCollections_AreRejectedWhereRequired()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentNullException>(() => client.UpdateRolePermissionsAsync("role", null!));
            Assert.Throws<ArgumentNullException>(() => client.InviteUsersToTeamAsync("team", null!));
            Assert.Throws<ArgumentException>(() => client.InviteUsersToTeamAsync("team", Array.Empty<string>()));
        }

        [Test]
        public async Task Role_RejectsMalformedResponses([Values("null", "[]", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.UpdateRolePermissionsAsync("role", new[] { "read_channel" }));
        }

        [Test]
        public async Task Errors_ArePreserved([Values("role", "invite", "reset", "verify")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            const string json = "{\"message\":\"request rejected\"}";
            using HttpClient http = CreateHttp(operation, (_, _) => Task.FromResult(ApiTestHttp.Json(status, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.ResponseJson, Is.EqualTo(json));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("role", "invite", "reset", "verify")] string operation)
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
        public async Task InFlightCancellation_ReachesRequest([Values("role", "invite", "reset", "verify")] string operation)
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = CreateHttp(operation, async (_, token) =>
            {
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancellation was not propagated.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, operation, source.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static HttpClient CreateHttp(string operation, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            if (operation == "role" || operation == "invite") { return ApiTestHttp.Create(respond); }
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization, Is.Null);
                Assert.That(request.RequestUri!.AbsolutePath, Does.Not.EndWith("/users/me"));
                return respond(request, token);
            }));
        }

        private static string Response(string operation) => operation == "role" ? RoleJson : "{\"status\":\"OK\"}";

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "role": return client.UpdateRolePermissionsAsync(" role/1 ", new[] { " read_channel ", "read_channel" }, token);
                case "invite": return client.InviteUsersToTeamAsync(" team/1 ", new[] { " Person@example.invalid ", "second@example.invalid" }, token);
                case "reset": return client.SendPasswordResetEmailAsync(" Person@example.invalid ", token);
                case "verify": return client.SendVerificationEmailAsync(" Person@example.invalid ", " /team?a=1&b=2 ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
