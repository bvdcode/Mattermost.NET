using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ServerLogOperationTests
    {
        [Test]
        public async Task AddClientLog_PreservesInputAndReadsNativeMessage([Values(false, true)] bool error,
            [Values("", " spaces & text ", "long")] string input)
        {
            string message = input == "long" ? new string('x', 1000) : input;
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/logs"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "level", "message" }));
                Assert.That(body.RootElement.GetProperty("level").GetString(), Is.EqualTo(error ? "ERROR" : "DEBUG"));
                Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo(message));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"level\":\"ERROR\",\"message\":\"Native prefixed message\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.AddClientLogAsync(message, error), Is.EqualTo("Native prefixed message"));
        }

        [Test]
        public async Task AddClientLog_RejectsNullMessage()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.AddClientLogAsync(null!));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{\"message\":null}")]
        [TestCase("{\"message\":1}")]
        [TestCase("invalid JSON")]
        public async Task AddClientLog_RejectsInvalidResponse(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.AddClientLogAsync("message"));
        }

        [Test]
        public async Task Query_PreservesDateOffsetsFiltersAndNativeObjects()
        {
            DateTimeOffset from = new DateTimeOffset(2026, 10, 11, 12, 13, 14, 123, TimeSpan.FromHours(-7));
            DateTimeOffset to = new DateTimeOffset(2026, 10, 12, 0, 1, 2, 0, TimeSpan.FromHours(5.5));
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/logs/query?page=2&logs_per_page=100"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "server_names", "log_levels", "date_from", "date_to" }));
                Assert.That(body.RootElement.GetProperty("server_names").EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "node/1" }));
                Assert.That(body.RootElement.GetProperty("log_levels").EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "error", "warn" }));
                Assert.That(body.RootElement.GetProperty("date_from").GetString(), Is.EqualTo("2026-10-11 12:13:14.123 -07:00"));
                Assert.That(body.RootElement.GetProperty("date_to").GetString(), Is.EqualTo("2026-10-12 00:01:02.000 +05:30"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"node/1\":[{\"level\":\"error\",\"message\":\"native\",\"custom\":{\"count\":4294967297}}],\"node-2\":[]}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IDictionary<string, IList<JsonElement>> result = await client.QueryServerLogsAsync(2, 100, new[] { " node/1 ", "node/1" },
                new[] { " error ", "warn", "error" }, from, to);
            Assert.That(result.Keys, Is.EquivalentTo(new[] { "node/1", "node-2" }));
            Assert.That(result["node/1"][0].GetProperty("custom").GetProperty("count").GetInt64(), Is.EqualTo(4294967297L));
            Assert.That(result["node-2"], Is.Empty);
        }

        [Test]
        public async Task Query_DefaultsAreUnboundedAndEmptyIsNotNull()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?page=0&logs_per_page=10000"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("server_names").GetArrayLength(), Is.Zero);
                Assert.That(body.RootElement.GetProperty("log_levels").GetArrayLength(), Is.Zero);
                Assert.That(body.RootElement.GetProperty("date_from").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty("date_to").GetString(), Is.Empty);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.QueryServerLogsAsync(), Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Query_AllowsEitherDateBoundIndependently(bool upper)
        {
            DateTimeOffset bound = new DateTimeOffset(2026, 10, 11, 0, 0, 0, TimeSpan.Zero);
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty(upper ? "date_from" : "date_to").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty(upper ? "date_to" : "date_from").GetString(), Is.EqualTo("2026-10-11 00:00:00.000 +00:00"));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            if (upper) { await client.QueryServerLogsAsync(to: bound); }
            else { await client.QueryServerLogsAsync(from: bound); }
        }

        [TestCase(-1, 1)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 10001)]
        public async Task Query_RejectsInvalidPaging(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.QueryServerLogsAsync(page, perPage));
        }

        [Test]
        public async Task Query_RejectsBlankFilters([Values(false, true)] bool levels, [Values(null, "", " ")] string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            if (levels) { await Assert.ThrowsAsync<ArgumentException>(() => client.QueryServerLogsAsync(logLevels: new[] { value! })); }
            else { await Assert.ThrowsAsync<ArgumentException>(() => client.QueryServerLogsAsync(serverNames: new[] { value! })); }
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{\"node\":{}}")]
        [TestCase("invalid JSON")]
        public async Task Query_RejectsInvalidResults(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.QueryServerLogsAsync());
        }
    }
}
