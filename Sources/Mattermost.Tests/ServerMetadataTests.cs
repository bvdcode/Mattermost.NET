using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ServerMetadataTests
    {
        private static readonly string[] Operations = { "config", "environment", "migrations", "onboarding", "release" };

        [Test]
        public async Task Reads_UseAuthenticatedBodylessGet([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat" + Route(operation)));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, Body(operation)));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await ReadAsync(client, operation);
        }

        [Test]
        public async Task Config_PreservesNativeNamesAndValueTypes([Values(false, true)] bool removeMasked,
            [Values(false, true)] bool removeDefaults)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/config?remove_masked="
                    + removeMasked.ToString().ToLowerInvariant() + "&remove_defaults=" + removeDefaults.ToString().ToLowerInvariant()));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                    "{\"ServiceSettings\":{\"SiteURL\":\"https://example.org\",\"EnableBots\":true,\"MaximumLoginAttempts\":7,\"Optional\":null},"
                    + "\"PluginSettings\":{\"Plugins\":{\"example\":{\"items\":[1,false,\"x\"]}}},\"FutureSection\":{}}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IDictionary<string, JsonElement> config = await client.GetServerConfigAsync(removeMasked, removeDefaults);
            Assert.That(config.Keys, Is.EquivalentTo(new[] { "ServiceSettings", "PluginSettings", "FutureSection" }));
            Assert.That(config["ServiceSettings"].GetProperty("SiteURL").GetString(), Is.EqualTo("https://example.org"));
            Assert.That(config["ServiceSettings"].GetProperty("EnableBots").GetBoolean(), Is.True);
            Assert.That(config["ServiceSettings"].GetProperty("MaximumLoginAttempts").GetInt32(), Is.EqualTo(7));
            Assert.That(config["ServiceSettings"].GetProperty("Optional").ValueKind, Is.EqualTo(JsonValueKind.Null));
            Assert.That(config["PluginSettings"].GetProperty("Plugins").GetProperty("example").GetProperty("items").GetArrayLength(), Is.EqualTo(3));
            Assert.That(config.ContainsKey("servicesettings"), Is.False);
        }

        [TestCase("{}", 0)]
        [TestCase("null", 0)]
        [TestCase("{\"ServiceSettings\":{\"SiteURL\":true},\"SqlSettings\":{\"DataSource\":true}}", 2)]
        public async Task Environment_PreservesPermissionFilteredFlags(string body, int count)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            IDictionary<string, JsonElement> config = await client.GetEnvironmentConfigAsync();
            Assert.That(config, Has.Count.EqualTo(count));
            if (count > 0)
            {
                Assert.That(config["ServiceSettings"].GetProperty("SiteURL").GetBoolean(), Is.True);
            }
        }

        [Test]
        public async Task Migrations_ReadNativeVersionsAndNames()
        {
            using HttpClient http = Respond(HttpStatusCode.OK, "[{\"version\":4294967297,\"name\":\"migration_one\"},{\"version\":2,\"name\":\"migration_two\"}]");
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<AppliedSchemaMigration> migrations = await client.GetAppliedSchemaMigrationsAsync();
            Assert.That(migrations, Has.Count.EqualTo(2));
            Assert.That(migrations[0].Version, Is.EqualTo(4294967297L));
            Assert.That(migrations[0].Name, Is.EqualTo("migration_one"));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(migrations[0]));
            Assert.That(encoded.RootElement.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[] { "version", "name" }));
            Assert.That(encoded.RootElement.GetProperty("version").GetInt64(), Is.EqualTo(4294967297L));
        }

        [Test]
        public async Task Migrations_EmptyArrayIsEmptyList()
        {
            using HttpClient http = Respond(HttpStatusCode.OK, "[]");
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetAppliedSchemaMigrationsAsync(), Is.Empty);
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public async Task Onboarding_ReadsNativeStringBoolean(string value, bool expected)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, "{\"name\":\"FirstAdminSetupComplete\",\"value\":\"" + value + "\"}");
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetFirstAdminOnboardingCompleteAsync(), Is.EqualTo(expected));
        }

        [TestCase("{}")]
        [TestCase("{\"value\":true}")]
        [TestCase("{\"value\":null}")]
        [TestCase("{\"value\":\"True\"}")]
        [TestCase("{\"value\":\"\"}")]
        [TestCase("{\"value\":\"unexpected\"}")]
        [TestCase("[]")]
        public async Task Onboarding_RejectsInvalidNativeStatus(string body)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetFirstAdminOnboardingCompleteAsync());
        }

        [Test]
        public async Task Release_ReadsAndSerializesAllNativeFields()
        {
            const string body = "{\"id\":4294967297,\"tag_name\":\"v12.0.1\",\"name\":\"Mattermost 12.0.1\","
                + "\"created_at\":\"2026-10-01T01:02:03Z\",\"published_at\":\"2026-10-02T04:05:06Z\","
                + "\"body\":\"Release notes\\nSecond line\",\"html_url\":\"https://example.org/releases/12.0.1\"}";
            using HttpClient http = Respond(HttpStatusCode.OK, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            ServerRelease release = await client.GetLatestServerReleaseAsync();
            Assert.That(release.Id, Is.EqualTo(4294967297L));
            Assert.That(release.TagName, Is.EqualTo("v12.0.1"));
            Assert.That(release.Name, Is.EqualTo("Mattermost 12.0.1"));
            Assert.That(release.CreatedAt, Is.EqualTo("2026-10-01T01:02:03Z"));
            Assert.That(release.PublishedAt, Is.EqualTo("2026-10-02T04:05:06Z"));
            Assert.That(release.Body, Is.EqualTo("Release notes\nSecond line"));
            Assert.That(release.Url, Is.EqualTo("https://example.org/releases/12.0.1"));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(release));
            using JsonDocument original = JsonDocument.Parse(body);
            Assert.That(JsonElement.DeepEquals(encoded.RootElement, original.RootElement), Is.True);
        }

        [Test]
        public async Task Errors_PreserveNativeDetails([ValueSource(nameof(Operations))] string operation,
            [Values(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = Respond(status, "{\"message\":\"metadata unavailable\"}");
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => ReadAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("metadata unavailable"));
            Assert.That(error.RequestMethod, Is.EqualTo("GET"));
        }

        [Test]
        public async Task MalformedResponses_Throw([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, "invalid JSON");
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [TestCase("config")]
        [TestCase("migrations")]
        [TestCase("onboarding")]
        [TestCase("release")]
        public async Task MissingResponse_Throws(string operation)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, "null");
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [TestCase("config", "[]")]
        [TestCase("environment", "[]")]
        [TestCase("migrations", "{}")]
        [TestCase("release", "[]")]
        public async Task WrongResponseShape_Throws(string operation, string body)
        {
            using HttpClient http = Respond(HttpStatusCode.OK, body);
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([ValueSource(nameof(Operations))] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => ReadAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => ReadAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_ReachesHttp([ValueSource(nameof(Operations))] string operation)
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
            Task pending = ReadAsync(client, operation, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static HttpClient Respond(HttpStatusCode status, string body) =>
            ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, body)));

        private static string Route(string operation)
        {
            switch (operation)
            {
                case "config": return "/api/v4/config?remove_masked=false&remove_defaults=false";
                case "environment": return "/api/v4/config/environment";
                case "migrations": return "/api/v4/system/schema/version";
                case "onboarding": return "/api/v4/system/onboarding/complete";
                case "release": return "/api/v4/latest_version";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static string Body(string operation)
        {
            switch (operation)
            {
                case "config": return "{}";
                case "environment": return "{}";
                case "migrations": return "[]";
                case "onboarding": return "{\"value\":\"false\"}";
                case "release": return "{\"id\":1}";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static async Task<object> ReadAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "config": return await client.GetServerConfigAsync(cancellationToken: token);
                case "environment": return await client.GetEnvironmentConfigAsync(token);
                case "migrations": return await client.GetAppliedSchemaMigrationsAsync(token);
                case "onboarding": return await client.GetFirstAdminOnboardingCompleteAsync(token);
                case "release": return await client.GetLatestServerReleaseAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
