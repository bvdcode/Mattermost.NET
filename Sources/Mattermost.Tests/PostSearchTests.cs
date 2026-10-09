using Mattermost.Exceptions;
using Mattermost.Models.Responses;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PostSearchTests
    {
        private const string PostsJson = "{\"order\":[\"post-1\"],\"posts\":{\"post-1\":{\"id\":\"post-1\",\"message\":\"hello\"}},\"matches\":{\"post-1\":[\"hello\"]},\"has_next\":true}";

        [TestCase(false, "/posts/search")]
        [TestCase(true, "/teams/team%2F1%3F%23/posts/search")]
        public async Task Search_SendsExactBodyAndReadsMatches(bool teamSearch, string path)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(6));
                Assert.That(body.RootElement.GetProperty("terms").GetString(), Is.EqualTo("hello in:town-square"));
                Assert.That(body.RootElement.GetProperty("is_or_search").GetBoolean(), Is.True);
                Assert.That(body.RootElement.GetProperty("page").GetInt32(), Is.EqualTo(2));
                Assert.That(body.RootElement.GetProperty("per_page").GetInt32(), Is.EqualTo(17));
                Assert.That(body.RootElement.GetProperty("time_zone_offset").GetInt32(), Is.EqualTo(-25200));
                Assert.That(body.RootElement.GetProperty("include_deleted_channels").GetBoolean(), Is.True);
                return ApiTestHttp.Json(HttpStatusCode.OK, PostsJson);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            PostSearchResponse result;
            if (teamSearch) { result = await client.SearchTeamPostsAsync(" team/1?# ", " hello in:town-square ", true, 2, 17, -25200, true); }
            else { result = await client.SearchPostsAsync(" hello in:town-square ", true, 2, 17, -25200, true); }
            Assert.That(result.Order, Is.EqualTo(new[] { "post-1" }));
            Assert.That(result.Posts["post-1"].Text, Is.EqualTo("hello"));
            Assert.That(result.Matches!["post-1"], Is.EqualTo(new[] { "hello" }));
            Assert.That(result.HasNext, Is.True);
        }

        [Test]
        public async Task Search_DefaultsAndNullMatchesAreSupported()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("is_or_search").GetBoolean(), Is.False);
                Assert.That(body.RootElement.GetProperty("page").GetInt32(), Is.Zero);
                Assert.That(body.RootElement.GetProperty("per_page").GetInt32(), Is.EqualTo(60));
                Assert.That(body.RootElement.GetProperty("time_zone_offset").GetInt32(), Is.Zero);
                Assert.That(body.RootElement.GetProperty("include_deleted_channels").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"order\":[],\"posts\":{},\"matches\":null}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            PostSearchResponse result = await client.SearchPostsAsync("missing");
            Assert.That(result.Order, Is.Empty);
            Assert.That(result.Posts, Is.Empty);
            Assert.That(result.Matches, Is.Null);
        }

        [TestCase(null, null, "")]
        [TestCase(" team/1?# ", null, "&team_id=team%2F1%3F%23")]
        [TestCase(null, " channel/1?# ", "&channel_id=channel%2F1%3F%23")]
        [TestCase("team", "channel", "&team_id=team&channel_id=channel")]
        [TestCase(" \t", " \t", "")]
        public async Task Flagged_EncodesOptionalFilters(string? team, string? channel, string query)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/user%2F1%3F%23/posts/flagged?page=2&per_page=17" + query));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, PostsJson));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            ChannelPostsResponse result = await client.GetFlaggedPostsAsync(" user/1?# ", team, channel, 2, 17);
            Assert.That(result.Posts["post-1"].Id, Is.EqualTo("post-1"));
        }

        [TestCase(0, 1, false)]
        [TestCase(200, 200, true)]
        public async Task Unread_UsesInvariantLimitsAndExactOptionNames(int before, int after, bool threads)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
                using HttpClient http = ApiTestHttp.Create((request, _) =>
                {
                    Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                    Assert.That(request.Content, Is.Null);
                    string flag = threads.ToString().ToLowerInvariant();
                    string query = "limit_before=" + before.ToString(CultureInfo.InvariantCulture) + "&limit_after=" + after.ToString(CultureInfo.InvariantCulture)
                        + "&skipFetchThreads=" + flag + "&collapsedThreads=" + flag + "&collapsedThreadsExtended=" + flag;
                    Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/me/channels/channel%2F1%3F%23/posts/unread?" + query));
                    return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, PostsJson));
                });
                using MattermostClient client = ApiTestHttp.Client(http);
                ChannelPostsResponse result = await client.GetUnreadPostsAsync("me", " channel/1?# ", before, after, threads, threads, threads);
                Assert.That(result.Order.Single(), Is.EqualTo("post-1"));
            }
            finally { CultureInfo.CurrentCulture = original; }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankRequiredValues_AreRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentException>(() => client.SearchPostsAsync(value!));
            Assert.Throws<ArgumentException>(() => client.SearchTeamPostsAsync(value!, "hello"));
            Assert.Throws<ArgumentException>(() => client.SearchTeamPostsAsync("team", value!));
            Assert.Throws<ArgumentException>(() => client.GetFlaggedPostsAsync(value!));
            Assert.Throws<ArgumentException>(() => client.GetUnreadPostsAsync(value!, "channel"));
            Assert.Throws<ArgumentException>(() => client.GetUnreadPostsAsync("me", value!));
        }

        [TestCase(-1, 60)]
        [TestCase(0, 0)]
        [TestCase(0, -1)]
        public void InvalidPaging_AreRejected(int page, int perPage)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentOutOfRangeException>(() => client.SearchPostsAsync("hello", page: page, perPage: perPage));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.SearchTeamPostsAsync("team", "hello", page: page, perPage: perPage));
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetFlaggedPostsAsync("me", page: page, perPage: perPage));
        }

        [TestCase(-1, 60)]
        [TestCase(201, 60)]
        [TestCase(60, 0)]
        [TestCase(60, -1)]
        [TestCase(60, 201)]
        public void InvalidUnreadLimits_AreRejected(int before, int after)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.Throws<ArgumentOutOfRangeException>(() => client.GetUnreadPostsAsync("me", "channel", before, after));
        }

        [Test]
        public async Task ErrorsAndCancellation_ArePreserved([Values("all", "team", "flagged", "unread")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(exception!.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task MalformedResponses_Throw([Values("all", "team", "flagged", "unread")] string operation,
            [Values("null", "not json")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("all", "team", "flagged", "unread")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => InvokeAsync(client, operation, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        private static Task InvokeAsync(MattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "all": return client.SearchPostsAsync("hello", cancellationToken: token);
                case "team": return client.SearchTeamPostsAsync("team", "hello", cancellationToken: token);
                case "flagged": return client.GetFlaggedPostsAsync("me", cancellationToken: token);
                case "unread": return client.GetUnreadPostsAsync("me", "channel", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
