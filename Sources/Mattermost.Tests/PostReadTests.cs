using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class PostReadTests
    {
        [Test]
        public async Task GetPostsByIds_SendsOneArrayRequest_EnumeratesOnce()
        {
            int enumerations = 0;
            int requests = 0;
            IEnumerable<string> PostIds()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return " post-1 ";
                yield return "post-2";
            }

            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                requests++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/posts/ids"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                string[]? ids = JsonSerializer.Deserialize<string[]>(await request.Content.ReadAsStringAsync(token));
                Assert.That(ids, Is.EqualTo(new[] { "post-1", "post-2" }));
                return JsonResponse(HttpStatusCode.OK, "[{\"id\":\"post-2\",\"channel_id\":\"channel-1\",\"message\":\"Reply\"}]");
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            IList<Post> posts = await api.GetPostsByIdsAsync(PostIds());

            Assert.That(requests, Is.EqualTo(1));
            Assert.That(posts, Has.Count.EqualTo(1));
            Assert.That(posts[0].Id, Is.EqualTo("post-2"));
            Assert.That(posts[0].ChannelId, Is.EqualTo("channel-1"));
            Assert.That(posts[0].Text, Is.EqualTo("Reply"));
        }

        [Test]
        public async Task GetPostsByIds_MaximumBatch_IsAllowed()
        {
            using HttpClient http = CreateHttpClient(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetArrayLength(), Is.EqualTo(MattermostApiLimits.MaxPostIdsPerRequest));
                return JsonResponse(HttpStatusCode.OK, "[]");
            });
            using MattermostClient client = CreateClient(http);

            IList<Post> posts = await client.GetPostsByIdsAsync(
                Enumerable.Range(0, MattermostApiLimits.MaxPostIdsPerRequest).Select(index => "post-" + index));

            Assert.That(posts, Is.Empty);
        }

        [TestCaseSource(nameof(InvalidPostIds))]
        public void GetPostsByIds_InvalidCollection_DoesNotSendRequests(string[]? ids)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            ArgumentException exception = Assert.Catch<ArgumentException>(() => client.GetPostsByIdsAsync(ids!))!;

            Assert.That(exception.ParamName, Is.EqualTo("postIds"));
        }

        private static IEnumerable<TestCaseData> InvalidPostIds()
        {
            yield return new TestCaseData(new object?[] { null });
            yield return new TestCaseData(new object[] { Array.Empty<string>() });
            yield return new TestCaseData(new object[] { new string[] { null! } });
            yield return new TestCaseData(new object[] { new[] { "post-1", "" } });
            yield return new TestCaseData(new object[] { new[] { " \t" } });
            yield return new TestCaseData(new object[] { Enumerable.Repeat("post-1", MattermostApiLimits.MaxPostIdsPerRequest + 1).ToArray() });
        }

        [Test]
        public async Task GetPinnedPosts_EscapesChannelAndReadsPostList()
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/channels/channel%2F1%3F%23/pinned"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(JsonResponse(HttpStatusCode.OK,
                    "{\"order\":[\"post-1\"],\"posts\":{\"post-1\":{\"id\":\"post-1\",\"message\":\"Pinned\",\"is_pinned\":true}}}"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            ChannelPostsResponse posts = await api.GetPinnedPostsAsync(" channel/1?# ");

            Assert.That(posts.Order, Is.EqualTo(new[] { "post-1" }));
            Assert.That(posts.Posts["post-1"].Text, Is.EqualTo("Pinned"));
            Assert.That(posts.Posts["post-1"].IsPinned, Is.True);
        }

        [TestCase(false, "")]
        [TestCase(true, "?include_deleted=true")]
        public async Task GetPostFiles_EscapesPostAndReadsMetadata(bool includeDeleted, string query)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://mattermost.example/chat/api/v4/posts/post%2F1%3F%23/files/info" + query));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(JsonResponse(HttpStatusCode.OK,
                    "[{\"id\":\"file-1\",\"name\":\"report.pdf\",\"size\":1234,\"mime_type\":\"application/pdf\",\"create_at\":1790000000000}]"));
            });
            using MattermostClient client = CreateClient(http);
            IMattermostClient api = client;

            IList<FileDetails> files = await api.GetPostFilesAsync(" post/1?# ", includeDeleted);

            Assert.That(files, Has.Count.EqualTo(1));
            Assert.That(files[0].Id, Is.EqualTo("file-1"));
            Assert.That(files[0].Name, Is.EqualTo("report.pdf"));
            Assert.That(files[0].Size, Is.EqualTo(1234));
            Assert.That(files[0].MimeType, Is.EqualTo("application/pdf"));
            Assert.That(files[0].CreateAt, Is.EqualTo(1790000000000));
        }

        [TestCase(2147483647L)]
        [TestCase(2147483648L)]
        [TestCase(long.MaxValue)]
        public async Task GetPostFiles_PreservesLargeFileSize(long size)
        {
            string body = JsonSerializer.Serialize(new[] { new { id = "file-1", size } });
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, body)));
            using MattermostClient client = CreateClient(http);

            IList<FileDetails> files = await client.GetPostFilesAsync("post-1");

            Assert.That(files, Has.Count.EqualTo(1));
            Assert.That(files[0].Size, Is.EqualTo(size));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void BlankIdentifiers_DoNotSendRequests(string? id)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);

            Assert.That(Assert.Throws<ArgumentException>(() => client.GetPinnedPostsAsync(id!))!.ParamName, Is.EqualTo("channelId"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetPostFilesAsync(id!))!.ParamName, Is.EqualTo("postId"));
        }

        [TestCase("[]")]
        [TestCase("null")]
        public async Task EmptyResults_RemainEmpty(string fileResponse)
        {
            using HttpClient http = CreateHttpClient((request, _) =>
            {
                string body = "[]";
                if (request.RequestUri!.AbsolutePath.EndsWith("/pinned", StringComparison.Ordinal))
                {
                    body = "{\"order\":[],\"posts\":{}}";
                }
                else if (request.RequestUri.AbsolutePath.EndsWith("/files/info", StringComparison.Ordinal))
                {
                    body = fileResponse;
                }
                return Task.FromResult(JsonResponse(HttpStatusCode.OK, body));
            });
            using MattermostClient client = CreateClient(http);

            Assert.That(await client.GetPostsByIdsAsync(new[] { "post-1" }), Is.Empty);
            Assert.That(await client.GetPostFilesAsync("post-1"), Is.Empty);
            ChannelPostsResponse pinned = await client.GetPinnedPostsAsync("channel-1");
            Assert.That(pinned.Order, Is.Empty);
            Assert.That(pinned.Posts, Is.Empty);
        }

        [Test]
        public async Task HttpFailures_ArePropagated(
            [Values("batch", "pinned", "files")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable)] HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status, "{\"message\":\"Access denied\"}")));
            using MattermostClient client = CreateClient(http);

            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation)))!;

            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("Access denied"));
        }

        [TestCase("batch")]
        [TestCase("pinned")]
        [TestCase("files")]
        public void CancelledAndDisposedCalls_DoNotSendRequests(string operation)
        {
            using HttpClient http = RejectRequests();
            using MattermostClient client = CreateClient(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() => InvokeAsync(client, operation, cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Cancellation_StopsPendingRequest(
            [Values("batch", "pinned", "files")] string operation, [Values] bool duringAuthentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("The request must be cancelled.");
            }));
            using MattermostClient client = CreateClient(http);

            Task pending = InvokeAsync(client, operation, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();

            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "batch": return client.GetPostsByIdsAsync(new[] { "post-1" }, token);
                case "pinned": return client.GetPinnedPostsAsync("channel-1", token);
                case "files": return client.GetPostFilesAsync("post-1", cancellationToken: token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static MattermostClient CreateClient(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);

        private static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
