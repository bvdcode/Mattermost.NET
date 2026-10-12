using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class AdministrativeUtilityTests
    {
        [Test]
        public async Task LogDownload_UsesNativeGetAndPreservesBytes([Values(false, true)] bool empty)
        {
            byte[] bytes = empty ? Array.Empty<byte>() : new byte[] { 0x00, 0x80, 0xff, 0x0d, 0x0a };
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/logs/download"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.DownloadServerLogsAsync(), Is.EqualTo(bytes));
        }

        [Test]
        public async Task Permissions_SendNativeArrayAndPreserveResultOrder()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/permissions/ancillary"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateArray().Select(value => value.GetString()),
                    Is.EqualTo(new[] { "sysconsole_read_user_management_users", "future_permission", "future_permission" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[\"read_other_users_teams\",\"future_permission\",\"sysconsole_read_user_management_users\",\"read_other_users_teams\"]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetAncillaryPermissionsAsync(new[] { " sysconsole_read_user_management_users ", "future_permission", "future_permission" }),
                Is.EqualTo(new[] { "read_other_users_teams", "future_permission", "sysconsole_read_user_management_users", "read_other_users_teams" }));
        }

        [Test]
        public async Task Permissions_RejectNullOrEmptyCollection([Values(false, true)] bool isNull)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<ArgumentException>(() => client.GetAncillaryPermissionsAsync(isNull ? null! : Array.Empty<string>()));
        }

        [Test]
        public async Task Permissions_RejectBlankNames([Values(null, "", " ", "\t")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAncillaryPermissionsAsync(new[] { "known", value! }));
        }

        [Test]
        public async Task Permissions_CopyInputBeforeAuthentication()
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user1\"}");
                }
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateArray().Select(value => value.GetString()), Is.EqualTo(new[] { "sysconsole_read_user_management_users" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[\"sysconsole_read_user_management_users\"]");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            List<string> permissions = new List<string> { "sysconsole_read_user_management_users" };
            Task<IList<string>> pending = client.GetAncillaryPermissionsAsync(permissions);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            permissions.Clear();
            release.SetResult();
            Assert.That(await pending.WaitAsync(TimeSpan.FromSeconds(5)), Is.EqualTo(new[] { "sysconsole_read_user_management_users" }));
        }

        [Test]
        public async Task Permissions_RejectMalformedArray([Values("null", "{}", "[1]", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetAncillaryPermissionsAsync(new[] { "known" }));
        }

        [Test]
        public async Task Errors_PreserveNativeStatus([Values("logs", "permissions")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"utility unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("utility unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("logs", "permissions")] string operation)
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
        public async Task Cancellation_ReachesTransport([Values("logs", "permissions")] string operation)
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

        private static Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "logs": return client.DownloadServerLogsAsync(token);
                case "permissions": return client.GetAncillaryPermissionsAsync(new[] { "sysconsole_read_user_management_users" }, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
