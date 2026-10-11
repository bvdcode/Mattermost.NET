using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class UserAccessTokenTests
    {
        private const string MetadataJson = "{\"id\":\"token-id\",\"user_id\":\"user\",\"description\":\"Description\",\"is_active\":true,\"expires_at\":1800000000000}";

        [TestCase("create", "/chat/api/v4/users/id%2F1/tokens", "POST")]
        [TestCase("user-list", "/chat/api/v4/users/id%2F1/tokens?page=2&per_page=3", "GET")]
        [TestCase("all-list", "/chat/api/v4/users/tokens?page=2&per_page=3", "GET")]
        [TestCase("get", "/chat/api/v4/users/tokens/id%2F1", "GET")]
        [TestCase("search", "/chat/api/v4/users/tokens/search", "POST")]
        [TestCase("disable", "/chat/api/v4/users/tokens/disable", "POST")]
        [TestCase("enable", "/chat/api/v4/users/tokens/enable", "POST")]
        [TestCase("count", "/chat/api/v4/users/tokens/non_compliant/count", "GET")]
        public async Task Methods_UseNativeContracts(string operation, string route, string method)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, _) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                string response;
                switch (operation)
                {
                    case "create":
                        await AssertBodyAsync(request, "description", "Description");
                        response = MetadataJson.TrimEnd('}') + ",\"token\":\"example-secret\"}";
                        break;
                    case "search":
                        await AssertBodyAsync(request, "term", "id/1");
                        response = "[" + MetadataJson + "]";
                        break;
                    case "disable":
                    case "enable":
                        await AssertBodyAsync(request, "token_id", "id/1");
                        response = "{\"status\":\"OK\"}";
                        break;
                    case "count":
                        Assert.That(request.Content, Is.Null);
                        response = "{\"count\":9007199254740991}";
                        break;
                    case "user-list":
                    case "all-list":
                        Assert.That(request.Content, Is.Null);
                        response = "[" + MetadataJson + "]";
                        break;
                    case "get":
                        Assert.That(request.Content, Is.Null);
                        response = MetadataJson;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(operation));
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, response);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            object? result = await InvokeAsync(client, operation, " id/1 ");
            if (operation == "disable" || operation == "enable") { Assert.That(result, Is.Null); return; }
            if (operation == "count") { Assert.That(result, Is.EqualTo(9007199254740991L)); return; }
            UserAccessToken token;
            switch (operation)
            {
                case "create":
                case "get": token = (UserAccessToken)result!; break;
                case "user-list":
                case "all-list":
                case "search": token = ((IList<UserAccessToken>)result!).Single(); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
            Assert.Multiple(() =>
            {
                Assert.That(token.Id, Is.EqualTo("token-id"));
                Assert.That(token.UserId, Is.EqualTo("user"));
                Assert.That(token.Description, Is.EqualTo("Description"));
                Assert.That(token.IsActive, Is.True);
                Assert.That(token.ExpiresAt, Is.EqualTo(1800000000000L));
                Assert.That(token.Token, Is.EqualTo(operation == "create" ? "example-secret" : null));
            });
        }

        [Test]
        public async Task EmptyCollections_NormalizeNativeNull(
            [Values("user-list", "all-list", "search")] string operation, [Values("null", "[]")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await InvokeAsync(client, operation, "id"), Is.Empty);
        }

        [Test]
        public async Task Paging_DefaultsToFirstPage([Values(false, true)] bool all)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&per_page=60"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (all) { await client.GetAllUserAccessTokensAsync(); }
            else { await client.GetUserAccessTokensAsync("me"); }
        }

        [Test]
        public void MetadataSerialization_DoesNotInventASecret()
        {
            UserAccessToken token = JsonSerializer.Deserialize<UserAccessToken>(MetadataJson)!;
            using JsonDocument result = JsonDocument.Parse(JsonSerializer.Serialize(token));
            Assert.That(result.RootElement.TryGetProperty("token", out _), Is.False);
        }

        [TestCase("{}")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{\"count\":null}")]
        [TestCase("{\"count\":\"1\"}")]
        [TestCase("{\"count\":1.5}")]
        [TestCase("{\"count\":9223372036854775808}")]
        public async Task PolicyCount_RejectsInvalidShapes(string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetNonCompliantUserAccessTokenCountAsync());
        }

        private static async Task AssertBodyAsync(HttpRequestMessage request, string field, string expected)
        {
            using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
            Assert.That(body.RootElement.GetProperty(field).GetString(), Is.EqualTo(expected));
        }

        private static async Task<object?> InvokeAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "create": return await client.CreateUserAccessTokenAsync(id, " Description ", token);
                case "user-list": return await client.GetUserAccessTokensAsync(id, 2, 3, token);
                case "all-list": return await client.GetAllUserAccessTokensAsync(2, 3, token);
                case "get": return await client.GetUserAccessTokenAsync(id, token);
                case "search": return await client.SearchUserAccessTokensAsync(id, token);
                case "count": return await client.GetNonCompliantUserAccessTokenCountAsync(token);
                case "disable": await client.DisableUserAccessTokenAsync(id, token); return null;
                case "enable": await client.EnableUserAccessTokenAsync(id, token); return null;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
