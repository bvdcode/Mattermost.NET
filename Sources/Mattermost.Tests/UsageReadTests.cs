using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class UsageReadTests
    {
        [Test]
        public async Task Counters_UseNativeRoutesAndLongValues([Values(false, true)] bool storage, [Values(0L, 4294967297L)] long value)
        {
            string route = storage ? "/storage" : "/posts";
            string field = storage ? "bytes" : "count";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/usage" + route));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"" + field + "\":" + value + "}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            long actual = storage ? await client.GetStorageUsageAsync() : await client.GetPostsUsageAsync();
            Assert.That(actual, Is.EqualTo(value));
        }

        [Test]
        public async Task Limits_ReadsSingleObjectWithAllNativeCamelCaseFields()
        {
            const string json = "{\"maxUsersLimit\":4294967297,\"maxUsersHardLimit\":4294967298,\"activeUserCount\":4294967299,"
                + "\"singleChannelGuestCount\":4294967300,\"singleChannelGuestLimit\":4294967301,\"postHistoryLimit\":4294967302,\"lastAccessiblePostTime\":4294967303}";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/limits/server"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ServerLimits result = await client.GetServerLimitsAsync();
            Assert.That(result.MaxUsersLimit, Is.EqualTo(4294967297L));
            Assert.That(result.MaxUsersHardLimit, Is.EqualTo(4294967298L));
            Assert.That(result.ActiveUserCount, Is.EqualTo(4294967299L));
            Assert.That(result.SingleChannelGuestCount, Is.EqualTo(4294967300L));
            Assert.That(result.SingleChannelGuestLimit, Is.EqualTo(4294967301L));
            Assert.That(result.PostHistoryLimit, Is.EqualTo(4294967302L));
            Assert.That(result.LastAccessiblePostTime, Is.EqualTo(4294967303L));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(result));
            using JsonDocument native = JsonDocument.Parse(json);
            Assert.That(JsonElement.DeepEquals(encoded.RootElement, native.RootElement), Is.True);
        }

        [Test]
        public async Task Teams_ReadsOnlyTheTwoNativeCounters()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/usage/teams"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"active\":4294967297,\"cloud_archived\":4294967298}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            TeamsUsage result = await client.GetTeamsUsageAsync();
            Assert.That(result.Active, Is.EqualTo(4294967297L));
            Assert.That(result.CloudArchived, Is.EqualTo(4294967298L));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(result));
            Assert.That(encoded.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "active", "cloud_archived" }));
        }

        [Test]
        public async Task LimitsAndTeams_PreserveNativeZeroValues([Values(false, true)] bool limits)
        {
            string json = limits
                ? "{\"maxUsersLimit\":0,\"maxUsersHardLimit\":0,\"activeUserCount\":0,\"singleChannelGuestCount\":0,\"singleChannelGuestLimit\":0,\"postHistoryLimit\":0,\"lastAccessiblePostTime\":0}"
                : "{\"active\":0,\"cloud_archived\":0}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            if (limits)
            {
                ServerLimits result = await client.GetServerLimitsAsync();
                Assert.That(result.ActiveUserCount, Is.Zero);
                Assert.That(result.MaxUsersLimit, Is.Zero);
                Assert.That(result.PostHistoryLimit, Is.Zero);
            }
            else
            {
                TeamsUsage result = await client.GetTeamsUsageAsync();
                Assert.That(result.Active, Is.Zero);
                Assert.That(result.CloudArchived, Is.Zero);
            }
        }

        [Test]
        public async Task Counters_RejectInvalidNativeValues([Values(false, true)] bool storage,
            [Values("null", "[]", "{}", "null-counter", "text-counter", "fraction-counter", "overflow-counter", "invalid JSON")] string input)
        {
            string field = storage ? "bytes" : "count";
            string body = input switch
            {
                "null-counter" => "{\"" + field + "\":null}",
                "text-counter" => "{\"" + field + "\":\"1\"}",
                "fraction-counter" => "{\"" + field + "\":1.5}",
                "overflow-counter" => "{\"" + field + "\":9223372036854775808}",
                _ => input
            };
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, storage ? "storage" : "posts"));
        }

        [Test]
        public async Task Models_RejectMalformedResponses([Values("limits", "teams")] string operation,
            [Values("null", "[]", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Errors_ArePreserved([Values("posts", "storage", "limits", "teams")] string operation,
            [Values(HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"usage unavailable\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("usage unavailable"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("posts", "storage", "limits", "teams")] string operation)
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
        public async Task Cancellation_ReachesHttp([Values("posts", "storage", "limits", "teams")] string operation)
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
                case "posts": return client.GetPostsUsageAsync(token);
                case "storage": return client.GetStorageUsageAsync(token);
                case "limits": return client.GetServerLimitsAsync(token);
                case "teams": return client.GetTeamsUsageAsync(token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
