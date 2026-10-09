using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class MediaReadTests
    {
        [TestCase("preview", "/files/id%2F1%3F%23/preview")]
        [TestCase("thumbnail", "/files/id%2F1%3F%23/thumbnail")]
        [TestCase("profile", "/users/id%2F1%3F%23/image")]
        [TestCase("default", "/users/id%2F1%3F%23/image/default")]
        public async Task BinaryReads_EscapeIdentifiersAndPreserveBytes(string operation, string path)
        {
            byte[] bytes = { 0, 255, 128, 1, 34, 10 };
            using ByteArrayContent content = new ByteArrayContent(bytes);
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + path));
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            byte[] actual = await ReadBinaryAsync(client, operation, " id/1?# ");
            Assert.That(actual, Is.EqualTo(bytes));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => content.ReadAsByteArrayAsync());
        }

        [Test]
        public async Task PublicLink_ReturnsServerUrl()
        {
            const string url = "https://files.example/public/image?download=1&h=abc";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.Content, Is.Null);
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/files/id%2F1%3F%23/link"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, JsonSerializer.Serialize(new { link = url })));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetPublicFileLinkAsync(" id/1?# "), Is.EqualTo(url));
        }

        [TestCase("null")]
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase("{\"link\":null}")]
        [TestCase("{\"link\":\"\"}")]
        [TestCase("{\"link\":\" \"}")]
        [TestCase("{\"link\":42}")]
        public async Task PublicLink_InvalidResponsesAreRejected(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetPublicFileLinkAsync("file-1"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task BlankIdentifiers_AreRejected(string? id)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            foreach (string operation in new[] { "preview", "thumbnail", "profile", "default", "link" })
            {
                await Assert.ThrowsAsync<ArgumentException>(() => InvokeAsync(client, operation, id!));
            }
        }

        [Test]
        public async Task ApiErrors_PreserveDetailsAndDisposeContent(
            [Values("preview", "thumbnail", "profile", "default", "link")] string operation,
            [Values(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound)] HttpStatusCode status)
        {
            const string json = "{\"message\":\"inaccessible\"}";
            using StringContent content = new StringContent(json);
            string? uri = null;
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                uri = request.RequestUri!.ToString();
                return Task.FromResult(new HttpResponseMessage(status) { Content = content });
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "id-1"));
            Assert.That(exception!.StatusCode, Is.EqualTo(status));
            Assert.That(exception.Message, Is.EqualTo("inaccessible"));
            Assert.That(exception.ResponseJson, Is.EqualTo(json));
            Assert.That(exception.RequestMethod, Is.EqualTo("GET"));
            Assert.That(exception.RequestUri, Is.EqualTo(uri));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => content.ReadAsStringAsync());
        }

        [Test]
        public async Task NonJsonErrors_PreserveRawResponse([Values("preview", "thumbnail", "profile", "default", "link")] string operation)
        {
            const string body = "<html>upstream failed</html>";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.BadGateway, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? exception = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation, "id-1"));
            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(exception.ResponseJson, Is.EqualTo(body));
        }

        [Test]
        public async Task InFlightCancellation_ReachesHttp([Values("preview", "thumbnail", "profile", "default", "link")] string operation)
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
            Task request = InvokeAsync(client, operation, "id-1", source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => request);
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("preview", "thumbnail", "profile", "default", "link")] string operation)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, operation, "id-1", source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, operation, "id-1"));
        }

        private static Task InvokeAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            if (operation == "link") { return client.GetPublicFileLinkAsync(id, token); }
            return ReadBinaryAsync(client, operation, id, token);
        }

        private static Task<byte[]> ReadBinaryAsync(MattermostClient client, string operation, string id, CancellationToken token = default)
        {
            switch (operation)
            {
                case "preview": return client.GetFilePreviewAsync(id, token);
                case "thumbnail": return client.GetFileThumbnailAsync(id, token);
                case "profile": return client.GetUserImageAsync(id, token);
                case "default": return client.GetDefaultUserImageAsync(id, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
