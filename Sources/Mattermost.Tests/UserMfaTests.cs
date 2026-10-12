using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UserMfaTests
    {
        [Test]
        public void MfaSecret_UsesExactNativeCredentialFields()
        {
            const string json = "{\"secret\":\"TEST-SECRET\",\"qr_code\":\"cG5n\"}";
            MfaSecret model = JsonSerializer.Deserialize<MfaSecret>(json)!;
            Assert.That(JsonSerializer.Serialize(model), Is.EqualTo(json));
        }

        [Test]
        public async Task GenerateMfaSecret_UsesNativePostAndReturnsBothCredentials()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3Fx%23y/mfa/generate"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"secret\":\"TEST-SECRET\",\"qr_code\":\"cG5n\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MfaSecret result = await client.GenerateMfaSecretAsync(" user/1?x#y ");
            Assert.That(result.Secret, Is.EqualTo("TEST-SECRET"));
            Assert.That(result.QRCode, Is.EqualTo("cG5n"));
        }

        [TestCase(true, " 123456 ", "{\"activate\":true,\"code\":\" 123456 \"}")]
        [TestCase(false, null, "{\"activate\":false}")]
        [TestCase(false, "ignored", "{\"activate\":false}")]
        public async Task UpdateUserMfa_SendsNativeFieldsAndDoesNotSendInactiveCode(bool activate, string? code, string expected)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/users/user%2F1%3Fx%23y/mfa"));
                Assert.That(await request.Content!.ReadAsStringAsync(token), Is.EqualTo(expected));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateUserMfaAsync(" user/1?x#y ", activate, code);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Mfa_CurrentUserAliasIsPreserved(bool generate)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.AbsolutePath, Does.Contain("/users/me/mfa"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"secret\":\"test\",\"qr_code\":\"test\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, generate, "me");
        }

        [TestCase(false, null)]
        [TestCase(false, "")]
        [TestCase(false, " \t")]
        [TestCase(true, null)]
        [TestCase(true, "")]
        [TestCase(true, " \t")]
        public void Mfa_BlankUserIsRejected(bool generate, string? userId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => InvokeAsync(client, generate, userId!))!;
            Assert.That(error.ParamName, Is.EqualTo("userId"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void UpdateUserMfa_ActivationWithoutCodeIsRejected(string? code)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            ArgumentException error = Assert.Throws<ArgumentException>(() => client.UpdateUserMfaAsync("me", true, code))!;
            Assert.That(error.ParamName, Is.EqualTo("code"));
        }

        [TestCase(false, HttpStatusCode.BadRequest)]
        [TestCase(false, HttpStatusCode.Unauthorized)]
        [TestCase(false, HttpStatusCode.Forbidden)]
        [TestCase(false, HttpStatusCode.NotFound)]
        [TestCase(false, HttpStatusCode.InternalServerError)]
        [TestCase(false, HttpStatusCode.NotImplemented)]
        [TestCase(true, HttpStatusCode.BadRequest)]
        [TestCase(true, HttpStatusCode.Unauthorized)]
        [TestCase(true, HttpStatusCode.Forbidden)]
        [TestCase(true, HttpStatusCode.NotFound)]
        [TestCase(true, HttpStatusCode.InternalServerError)]
        [TestCase(true, HttpStatusCode.NotImplemented)]
        public async Task Mfa_NativeErrorIsPropagatedWithoutRetry(bool generate, HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"MFA rejected\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, generate)))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("MFA rejected"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{broken")]
        public async Task GenerateMfaSecret_InvalidResponseIsRejected(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => client.GenerateMfaSecretAsync("me"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Mfa_CancelledAndDisposedCallsSendNothing(bool generate)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => InvokeAsync(client, generate, token: cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => InvokeAsync(client, generate));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public async Task Mfa_CancellationStopsAuthenticationOrRequest(bool generate, bool authentication)
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
            Task pending = InvokeAsync(client, generate, token: cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostUsersClient client, bool generate, string userId = "user-1", CancellationToken token = default)
        {
            if (generate) { return client.GenerateMfaSecretAsync(userId, token); }
            return client.UpdateUserMfaAsync(userId, true, "123456", token);
        }
    }
}
