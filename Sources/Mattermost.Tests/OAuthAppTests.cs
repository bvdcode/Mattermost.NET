using Mattermost.Exceptions;
using Mattermost.Models.OAuth;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal partial class OAuthAppTests
    {
        private static readonly string[] Operations = { "list", "get", "info", "create", "update", "delete", "secret", "authorized" };
        private const string AppJson = "{\"id\":\"app\",\"creator_id\":\"creator\",\"create_at\":5000000000,\"update_at\":6000000000,\"client_secret\":\"secret\",\"name\":\"Name\",\"description\":\"Description\",\"icon_url\":\"https://example.org/icon\",\"callback_urls\":[\"https://example.org/callback\",\"http://localhost:8080/callback\"],\"homepage\":\"https://example.org\",\"is_trusted\":true,\"mattermost_app_id\":\"associated-app\",\"is_dynamically_registered\":true}";

        [TestCase("list", "GET", "/chat/api/v4/oauth/apps?page=2&per_page=3")]
        [TestCase("get", "GET", "/chat/api/v4/oauth/apps/app%2F1")]
        [TestCase("info", "GET", "/chat/api/v4/oauth/apps/app%2F1/info")]
        [TestCase("create", "POST", "/chat/api/v4/oauth/apps")]
        [TestCase("update", "PUT", "/chat/api/v4/oauth/apps/app%2F1")]
        [TestCase("delete", "DELETE", "/chat/api/v4/oauth/apps/app%2F1")]
        [TestCase("secret", "POST", "/chat/api/v4/oauth/apps/app%2F1/regen_secret")]
        [TestCase("authorized", "GET", "/chat/api/v4/users/user%2F1/oauth/apps/authorized?page=2&per_page=3")]
        public async Task Operations_UseNativeRoutesAndMethods(string operation, string method, string route)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(route));
                if (operation != "create" && operation != "update") { Assert.That(request.Content, Is.Null); }
                string json = operation == "list" || operation == "authorized" ? "[" + AppJson + "]" : AppJson;
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Application_PreservesEveryKnownNativeField()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, AppJson)));
            using MattermostClient client = ApiTestHttp.Client(http);
            OAuthApp app = await client.GetOAuthAppAsync("app");
            Assert.Multiple(() =>
            {
                Assert.That(app.Id, Is.EqualTo("app"));
                Assert.That(app.CreatorId, Is.EqualTo("creator"));
                Assert.That(app.CreatedAt, Is.EqualTo(5000000000));
                Assert.That(app.UpdatedAt, Is.EqualTo(6000000000));
                Assert.That(app.ClientSecret, Is.EqualTo("secret"));
                Assert.That(app.Name, Is.EqualTo("Name"));
                Assert.That(app.Description, Is.EqualTo("Description"));
                Assert.That(app.IconUrl, Is.EqualTo("https://example.org/icon"));
                Assert.That(app.CallbackUrls, Is.EqualTo(new[] { "https://example.org/callback", "http://localhost:8080/callback" }));
                Assert.That(app.Homepage, Is.EqualTo("https://example.org"));
                Assert.That(app.IsTrusted, Is.True);
                Assert.That(app.MattermostAppId, Is.EqualTo("associated-app"));
                Assert.That(app.IsDynamicallyRegistered, Is.True);
            });
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(app));
            using JsonDocument native = JsonDocument.Parse(AppJson);
            Assert.That(serialized.RootElement.EnumerateObject().Count(), Is.EqualTo(13));
            foreach (JsonProperty field in native.RootElement.EnumerateObject())
            {
                Assert.That(serialized.RootElement.GetProperty(field.Name).GetRawText(), Is.EqualTo(field.Value.GetRawText()));
            }
        }

        [TestCase("info")]
        [TestCase("authorized")]
        public async Task InformationResponses_PreserveAbsentSecretAndRegistrationFlag(string operation)
        {
            string json = "{\"id\":\"app\",\"client_secret\":\"\",\"callback_urls\":[\"https://example.org/callback\"]}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                operation == "authorized" ? "[" + json + "]" : json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            OAuthApp app = operation == "authorized" ? (await client.GetAuthorizedOAuthAppsAsync("me")).Single()
                : await client.GetOAuthAppInfoAsync("app");
            Assert.That(app.ClientSecret, Is.Empty);
            Assert.That(app.IsDynamicallyRegistered, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Writes_OnlySendEditableFieldsAndDoNotMutateInput(bool update)
        {
            OAuthApp input = Application();
            input.Id = " app/1 ";
            input.Name = " Name ";
            input.Homepage = " https://example.org ";
            input.IconUrl = " https://example.org/icon ";
            input.CallbackUrls = new List<string> { " https://example.org/callback ", "http://localhost:8080/callback" };
            input.Description = "";
            input.CreatorId = "readonly-creator";
            input.CreatedAt = 5000000000;
            input.UpdatedAt = 6000000000;
            input.ClientSecret = "readonly-secret";
            input.IsDynamicallyRegistered = true;
            input.MattermostAppId = "associated-app";
            string original = JsonSerializer.Serialize(input);
            using HttpClient http = ApiTestHttp.Create(async (request, requestToken) =>
            {
                using JsonDocument payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(requestToken));
                JsonElement body = payload.RootElement;
                Assert.That(body.GetProperty("name").GetString(), Is.EqualTo("Name"));
                Assert.That(body.GetProperty("description").GetString(), Is.Empty);
                Assert.That(body.GetProperty("homepage").GetString(), Is.EqualTo("https://example.org"));
                Assert.That(body.GetProperty("icon_url").GetString(), Is.EqualTo("https://example.org/icon"));
                Assert.That(body.GetProperty("is_trusted").GetBoolean(), Is.False);
                Assert.That(body.GetProperty("callback_urls").EnumerateArray().Select(value => value.GetString()),
                    Is.EqualTo(new[] { "https://example.org/callback", "http://localhost:8080/callback" }));
                Assert.That(body.EnumerateObject().Count(), Is.EqualTo(update ? 8 : 6));
                foreach (string key in new[] { "creator_id", "create_at", "update_at", "client_secret", "is_dynamically_registered", "is_public" })
                {
                    Assert.That(body.TryGetProperty(key, out _), Is.False, key);
                }
                if (update)
                {
                    Assert.That(body.GetProperty("id").GetString(), Is.EqualTo("app/1"));
                    Assert.That(body.GetProperty("mattermost_app_id").GetString(), Is.EqualTo("associated-app"));
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, AppJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (update) { await client.UpdateOAuthAppAsync(input); }
            else { await client.CreateOAuthAppAsync(input); }
            Assert.That(JsonSerializer.Serialize(input), Is.EqualTo(original));
        }

        [TestCase("list", "null")]
        [TestCase("list", "[]")]
        [TestCase("authorized", "null")]
        [TestCase("authorized", "[]")]
        public async Task Lists_NormalizeNativeEmptyResults(string operation, string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<OAuthApp> apps = operation == "list" ? await client.GetOAuthAppsAsync() : await client.GetAuthorizedOAuthAppsAsync("me");
            Assert.That(apps, Is.Empty);
        }

        [TestCase("cursor://oauth/callback")]
        [TestCase("com.example.client:/callback")]
        public async Task Updates_PreserveDynamicClientUrlsWithoutSendingRegistrationState(string callback)
        {
            OAuthApp input = Application();
            input.IsDynamicallyRegistered = true;
            input.Homepage = "";
            input.CallbackUrls = new List<string> { callback };
            using HttpClient http = ApiTestHttp.Create(async (request, requestToken) =>
            {
                using JsonDocument payload = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(requestToken));
                Assert.That(payload.RootElement.GetProperty("homepage").GetString(), Is.Empty);
                Assert.That(payload.RootElement.GetProperty("callback_urls")[0].GetString(), Is.EqualTo(callback));
                Assert.That(payload.RootElement.TryGetProperty("is_dynamically_registered", out _), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, AppJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateOAuthAppAsync(input);
            Assert.That(input.CallbackUrls[0], Is.EqualTo(callback));
        }

        [TestCaseSource(nameof(Operations))]
        public async Task Operations_PreserveNativeErrors(string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.Forbidden,
                "{\"id\":\"permission_denied\",\"message\":\"Forbidden\",\"status_code\":403}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(async () => await InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        [TestCase("get")]
        [TestCase("info")]
        [TestCase("create")]
        [TestCase("update")]
        [TestCase("secret")]
        public async Task ObjectResponses_RejectJsonNull(string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(async () => await InvokeAsync(client, operation));
        }

        [TestCase("list", "{}")]
        [TestCase("authorized", "{}")]
        [TestCase("get", "[]")]
        [TestCase("info", "[]")]
        [TestCase("create", "{\"id\":12}")]
        [TestCase("update", "{\"id\":12}")]
        [TestCase("secret", "{\"id\":12}")]
        [TestCase("get", "{invalid")]
        public async Task Responses_RejectMalformedJsonOrIncorrectTypes(string operation, string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(async () => await InvokeAsync(client, operation));
        }

        private static OAuthApp Application() => new OAuthApp
        {
            Id = " app/1 ", Name = "Name", Homepage = "https://example.org",
            CallbackUrls = new List<string> { "https://example.org/callback" }
        };

        private static Task InvokeAsync(IMattermostIntegrationsClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "list": return client.GetOAuthAppsAsync(2, 3, token);
                case "get": return client.GetOAuthAppAsync(" app/1 ", token);
                case "info": return client.GetOAuthAppInfoAsync(" app/1 ", token);
                case "create": return client.CreateOAuthAppAsync(Application(), token);
                case "update": return client.UpdateOAuthAppAsync(Application(), token);
                case "delete": return client.DeleteOAuthAppAsync(" app/1 ", token);
                case "secret": return client.RegenerateOAuthAppSecretAsync(" app/1 ", token);
                case "authorized": return client.GetAuthorizedOAuthAppsAsync(" user/1 ", 2, 3, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
