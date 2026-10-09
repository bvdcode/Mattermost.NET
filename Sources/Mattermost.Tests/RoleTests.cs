using Mattermost.Exceptions;
using Mattermost.Models.Roles;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class RoleTests
    {
        private const string RoleJson = "{\"id\":\"role-1\",\"name\":\"system_user\",\"display_name\":\"User\",\"description\":\"A user\",\"permissions\":[\"read_channel\"],\"create_at\":2147483648,\"update_at\":9223372036854775807,\"delete_at\":0,\"scheme_managed\":true,\"built_in\":true,\"scheme_id\":null}";

        [TestCase("id", "/roles/role%2F1%3F%23")]
        [TestCase("name", "/roles/name/system_user%2F%3F%23")]
        [TestCase("all", "/roles")]
        public async Task ReadRole_SendsRouteAndReadsAllMetadata(string operation, string path)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                string body = RoleJson;
                if (operation == "all")
                {
                    body = "[" + body + "]";
                }
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Role role;
            switch (operation)
            {
                case "id": role = await client.GetRoleAsync(" role/1?# "); break;
                case "name": role = await client.GetRoleByNameAsync(" system_user/?# "); break;
                case "all": role = (await client.GetRolesAsync()).Single(); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
            Assert.That(role.Id, Is.EqualTo("role-1"));
            Assert.That(role.Name, Is.EqualTo("system_user"));
            Assert.That(role.DisplayName, Is.EqualTo("User"));
            Assert.That(role.Description, Is.EqualTo("A user"));
            Assert.That(role.Permissions, Is.EqualTo(new[] { "read_channel" }));
            Assert.That(role.CreatedAt, Is.EqualTo(2147483648L));
            Assert.That(role.UpdatedAt, Is.EqualTo(long.MaxValue));
            Assert.That(role.DeletedAt, Is.Zero);
            Assert.That(role.IsBuiltIn, Is.True);
            Assert.That(role.IsSchemeManaged, Is.True);
            Assert.That(role.SchemeId, Is.Null);
        }

        [Test]
        public async Task BulkRoles_TrimsAndDeduplicatesNamesAndEnumeratesOnce()
        {
            int enumerations = 0;
            IEnumerable<string> Names()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " system_user ";
                yield return "system_user";
                yield return "channel_user";
            }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/roles/names"));
                string[]? names = JsonSerializer.Deserialize<string[]>(await request.Content!.ReadAsStringAsync(token));
                Assert.That(names, Is.EqualTo(new[] { "system_user", "channel_user" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[" + RoleJson + "]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That((await client.GetRolesByNamesAsync(Names())).Single().Name, Is.EqualTo("system_user"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankIdentifiers_AreRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetRoleAsync(value!))!.ParamName, Is.EqualTo("roleId"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetRoleByNameAsync(value!))!.ParamName, Is.EqualTo("roleName"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetRolesByNamesAsync(new[] { value! }))!.ParamName, Is.EqualTo("roleNames"));
        }

        [Test]
        public void EmptyNullAndOversizedBatches_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentNullException>(() => client.GetRolesByNamesAsync(null!));
            Assert.Throws<ArgumentException>(() => client.GetRolesByNamesAsync(Array.Empty<string>()));
            Assert.Throws<ArgumentException>(() => client.GetRolesByNamesAsync(Enumerable.Range(0, 101).Select(index => "role_" + index)));
        }

        [Test]
        public async Task BulkLimit_CountsDistinctNames()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                string[]? names = JsonSerializer.Deserialize<string[]>(await request.Content!.ReadAsStringAsync(token));
                Assert.That(names, Has.Length.EqualTo(100));
                return ApiTestHttp.Json(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            string[] values = Enumerable.Range(0, 100).Select(index => "role_" + index).ToArray();
            Assert.That(await client.GetRolesByNamesAsync(values.Concat(values)), Is.Empty);
        }

        [Test]
        public async Task Errors_ArePropagated([Values("all", "id", "name", "batch")] string operation,
            [Values(400, 401, 403, 404, 503)] int status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json((HttpStatusCode)status, "{\"message\":\"Denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;
            Assert.That(exception.StatusCode, Is.EqualTo((HttpStatusCode)status));
            Assert.That(exception.Message, Is.EqualTo("Denied"));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("all", "id", "name", "batch")] string operation,
            [Values("null", "invalid", "42")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_AreRespected([Values("all", "id", "name", "batch")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, new CancellationToken(true)));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task InFlightCancellation_ReachesBothRequests([Values("all", "id", "name", "batch")] string operation,
            [Values(false, true)] bool duringAuthentication)
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("Request must be cancelled.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = InvokeAsync(client, operation, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "all": return client.GetRolesAsync(token);
                case "id": return client.GetRoleAsync("role-1", token);
                case "name": return client.GetRoleByNameAsync("system_user", token);
                case "batch": return client.GetRolesByNamesAsync(new[] { "system_user" }, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
