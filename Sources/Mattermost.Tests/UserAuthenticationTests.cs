using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserAuthenticationTests
    {
        [Test]
        public async Task Get_PreservesAndEncodesExactIdentifier()
        {
            const string identifier = " 外部 +/?#&= ";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/auth_data?value=" + Uri.EscapeDataString(identifier)));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\",\"auth_service\":\"saml\",\"auth_data\":\"exact\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            User user = await client.GetUserByAuthDataAsync(identifier);
            Assert.That(user.Id, Is.EqualTo("user-1"));
            Assert.That(user.AuthService, Is.EqualTo("saml"));
            Assert.That(user.AuthData, Is.EqualTo("exact"));
        }

        [Test]
        public async Task Update_UsesOnlyNativeAuthFields()
        {
            const string identifier = " exact +/?#&= ";
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/users/user%2F1/auth"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EqualTo(new[] { "auth_service", "auth_data" }));
                Assert.That(body.RootElement.GetProperty("auth_service").GetString(), Is.EqualTo("saml"));
                Assert.That(body.RootElement.GetProperty("auth_data").GetString(), Is.EqualTo(identifier));
                return ApiTestHttp.Json(HttpStatusCode.OK, body.RootElement.GetRawText());
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserAuthentication result = await client.UpdateUserAuthenticationAsync(" user/1 ", " saml ", identifier);
            Assert.That(result.AuthService, Is.EqualTo("saml"));
            Assert.That(result.AuthData, Is.EqualTo(identifier));
        }

        [Test]
        public async Task Update_EmailSendsNullDataAndReadsOmittedMetadata()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("auth_service").GetString(), Is.EqualTo("email"));
                Assert.That(body.RootElement.GetProperty("auth_data").ValueKind, Is.EqualTo(JsonValueKind.Null));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            UserAuthentication result = await client.UpdateUserAuthenticationAsync("user-1", "email");
            Assert.That(result.AuthService, Is.Empty);
            Assert.That(result.AuthData, Is.Null);
        }

        [Test]
        public async Task Identifiers_AcceptNativeUtf8Boundary([Values(false, true)] bool update,
            [Values(false, true)] bool unicode)
        {
            string identifier = unicode ? new string('é', 64) : new string('x', 128);
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, update, identifier);
        }

        [Test]
        public async Task Identifiers_RejectEmptyAndUtf8Overflow([Values(false, true)] bool update,
            [Values(null, "", "ascii-overflow", "unicode-overflow")] string? input)
        {
            string? identifier = input;
            if (input == "ascii-overflow") { identifier = new string('x', 129); }
            if (input == "unicode-overflow") { identifier = new string('é', 65); }
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException? error = await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, update, identifier!));
            Assert.That(error!.ParamName, Is.EqualTo("authData"));
        }

        [Test]
        public async Task Update_RejectsBlankUserOrService([Values(false, true)] bool user,
            [Values(null, "", " \t")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateUserAuthenticationAsync(user ? value! : "user-1", user ? "saml" : value!, "identifier"));
        }

        [Test]
        public async Task Update_EmailRejectsNonNullData([Values("", "identifier")] string data)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateUserAuthenticationAsync("user-1", "email", data));
        }

        [Test]
        public async Task NativeErrors_ArePreserved([Values(false, true)] bool update,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"authentication operation denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, update, "identifier"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("authentication operation denied"));
        }

        [Test]
        public async Task MalformedResponses_AreRejected([Values(false, true)] bool update,
            [Values("null", "invalid JSON", "[]")] string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, update, "identifier"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeAuthentication([Values(false, true)] bool update)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => InvokeAsync(client, update, "identifier", cancellation.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, update, "identifier"));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([Values(false, true)] bool update)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
                throw new InvalidOperationException("Expected request cancellation.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, update, "identifier", cancellation.Token));
        }

        private static Task InvokeAsync(MattermostClient client, bool update, string identifier, CancellationToken token = default)
        {
            if (update) { return client.UpdateUserAuthenticationAsync("user-1", "saml", identifier, token); }
            return client.GetUserByAuthDataAsync(identifier, token);
        }
    }
}
