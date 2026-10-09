using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PreferenceTests
    {
        private const string PreferenceJson = "{\"user_id\":\"user-1\",\"category\":\"test\",\"name\":\"key\",\"value\":\"  value  \"}";

        [TestCase("all", "/preferences")]
        [TestCase("category", "/preferences/%20test%2F%3F%23%20")]
        [TestCase("single", "/preferences/%20test%2F%3F%23%20/name/%20key%2F%3F%23%20")]
        public async Task Reads_EscapeRouteAndPreserveValues(string operation, string path)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23" + path));
                string body = PreferenceJson;
                if (operation != "single")
                {
                    body = "[" + body + "]";
                }
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Preference preference;
            switch (operation)
            {
                case "all": preference = (await client.GetPreferencesAsync(" user/1?# ")).Single(); break;
                case "category": preference = (await client.GetPreferencesByCategoryAsync(" user/1?# ", " test/?# ")).Single(); break;
                case "single": preference = await client.GetPreferenceAsync(" user/1?# ", " test/?# ", " key/?# "); break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
            Assert.That(preference.UserId, Is.EqualTo("user-1"));
            Assert.That(preference.Category, Is.EqualTo("test"));
            Assert.That(preference.Name, Is.EqualTo("key"));
            Assert.That(preference.Value, Is.EqualTo("  value  "));
        }

        [Test]
        public async Task Writes_SendArrayOnceWithEmptyNamesAndUntrimmedValues([Values(false, true)] bool delete)
        {
            int enumerations = 0;
            IEnumerable<Preference> Preferences()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return new Preference { UserId = "user-1", Category = "test", Value = "  value  " };
            }
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                HttpMethod method = HttpMethod.Put;
                string path = "/chat/api/v4/users/me/preferences";
                if (delete)
                {
                    method = HttpMethod.Post;
                    path += "/delete";
                }
                Assert.That(request.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(path));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                Preference[]? preferences = JsonSerializer.Deserialize<Preference[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(preferences, Has.Length.EqualTo(1));
                Assert.That(preferences![0].UserId, Is.EqualTo("user-1"));
                Assert.That(preferences[0].Name, Is.Empty);
                Assert.That(preferences[0].Value, Is.EqualTo("  value  "));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (delete) { await client.DeletePreferencesAsync(" me ", Preferences()); }
            else { await client.UpdatePreferencesAsync(" me ", Preferences()); }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankKeys_AreRejected(string? key)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.GetPreferencesAsync(key!));
            Assert.Throws<ArgumentException>(() => client.GetPreferencesByCategoryAsync("me", key!));
            Assert.Throws<ArgumentException>(() => client.GetPreferenceAsync("me", "test", key!));
            Assert.Throws<ArgumentException>(() => client.UpdatePreferencesAsync(key!, new[] { ValidPreference() }));
            Assert.Throws<ArgumentException>(() => client.DeletePreferencesAsync(key!, new[] { ValidPreference() }));
        }

        [Test]
        public void InvalidBatches_AreRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (bool delete in new[] { false, true })
            {
                Assert.Throws<ArgumentNullException>(() => Write(client, delete, null!));
                Assert.Throws<ArgumentException>(() => Write(client, delete, Array.Empty<Preference>()));
                Assert.Throws<ArgumentException>(() => Write(client, delete, new Preference[] { null! }));
                Assert.Throws<ArgumentException>(() => Write(client, delete, new[] { new Preference { UserId = "user-1" } }));
                Assert.Throws<ArgumentException>(() => Write(client, delete, Enumerable.Repeat(ValidPreference(), 101)));
            }
        }

        [Test]
        public async Task BatchBoundary_Accepts100Preferences([Values(false, true)] bool delete)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetArrayLength(), Is.EqualTo(100));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await Write(client, delete, Enumerable.Repeat(ValidPreference(), 100));
        }

        [Test]
        public async Task Errors_ArePropagated([Values("all", "category", "single", "update", "delete")] string operation,
            [Values(400, 401, 403, 404, 503)] int status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json((HttpStatusCode)status, "{\"message\":\"Denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;
            Assert.That(exception.StatusCode, Is.EqualTo((HttpStatusCode)status));
        }

        [Test]
        public async Task InvalidJson_IsRejected([Values("all", "category", "single")] string operation,
            [Values("invalid", "42")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [TestCase("null")]
        [TestCase("[]")]
        public async Task EmptyPreferenceList_ReturnsEmptyCollection(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetPreferencesAsync("me"), Is.Empty);
        }

        [TestCase("category")]
        [TestCase("single")]
        public async Task NullSpecificPreferenceResponse_IsRejected(string operation)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "null")));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_AreRespected([Values("all", "category", "single", "update", "delete")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, new CancellationToken(true)));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        private static Preference ValidPreference() => new Preference { UserId = "user-1", Category = "test", Name = "key", Value = "value" };

        private static Task Write(IMattermostClient client, bool delete, IEnumerable<Preference> preferences)
        {
            if (delete) { return client.DeletePreferencesAsync("me", preferences); }
            return client.UpdatePreferencesAsync("me", preferences);
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "all": return client.GetPreferencesAsync("me", token);
                case "category": return client.GetPreferencesByCategoryAsync("me", "test", token);
                case "single": return client.GetPreferenceAsync("me", "test", "key", token);
                case "update": return client.UpdatePreferencesAsync("me", new[] { ValidPreference() }, token);
                case "delete": return client.DeletePreferencesAsync("me", new[] { ValidPreference() }, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
