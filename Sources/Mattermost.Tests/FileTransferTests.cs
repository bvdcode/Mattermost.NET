using Mattermost.Exceptions;
using Mattermost.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class FileTransferTests
    {
        [TestCase("headers", "HEAD", "/files/id%2F1%3F%23")]
        [TestCase("preview", "HEAD", "/files/id%2F1%3F%23/preview")]
        [TestCase("thumbnail", "HEAD", "/files/id%2F1%3F%23/thumbnail")]
        [TestCase("create", "POST", "/uploads")]
        [TestCase("get", "GET", "/uploads/id%2F1%3F%23")]
        [TestCase("data", "POST", "/uploads/id%2F1%3F%23")]
        public async Task Requests_UseNativeRoutesMethodsAndBodies(string operation, string method, string route)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method.Method, Is.EqualTo(method));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4" + route));
                switch (operation)
                {
                    case "create":
                        using (JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token)))
                        {
                            Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name),
                                Is.EquivalentTo(new[] { "channel_id", "filename", "file_size" }));
                            Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel/1"));
                            Assert.That(body.RootElement.GetProperty("filename").GetString(), Is.EqualTo("upload.txt"));
                            Assert.That(body.RootElement.GetProperty("file_size").GetInt64(), Is.EqualTo(4200000000L));
                        }
                        break;
                    case "data":
                        Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/octet-stream"));
                        Assert.That(request.Content.Headers.ContentLength, Is.EqualTo(3));
                        Assert.That(await request.Content.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                        break;
                    default: Assert.That(request.Content, Is.Null); break;
                }
                return ApiTestHttp.Json(HttpStatusCode.OK, Response(operation));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await InvokeAsync(client, operation);
        }

        [Test]
        public async Task Headers_ReadMetadataWithoutReadingBodyAndDisposeResponse([Values("headers", "preview", "thumbnail")] string operation)
        {
            using FileHeadResponseContent content = new FileHeadResponseContent();
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") { FileName = "image.png" };
            content.Headers.LastModified = DateTimeOffset.FromUnixTimeSeconds(1790000000);
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                response.Headers.ETag = new EntityTagHeaderValue("\"version-1\"");
                return Task.FromResult(response);
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            FileHeaders result = await HeadersAsync(client, operation);
            Assert.That(result.ContentLength, Is.EqualTo(321));
            Assert.That(result.ContentType, Is.EqualTo("image/png"));
            Assert.That(result.ContentDisposition, Does.Contain("image.png"));
            Assert.That(result.LastModified, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1790000000)));
            Assert.That(result.ETag, Is.EqualTo("\"version-1\""));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => content.ReadAsByteArrayAsync());
        }

        [Test]
        public async Task Headers_KeepMissingOptionalMetadataNull()
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            using MattermostClient client = ApiTestHttp.Client(http);
            FileHeaders result = await client.GetFileHeadersAsync("file");
            Assert.That(result.ContentType, Is.Null);
            Assert.That(result.ContentDisposition, Is.Null);
            Assert.That(result.LastModified, Is.Null);
            Assert.That(result.ETag, Is.Null);
        }

        [Test]
        public async Task UploadSession_ReadsNativeFieldsAndLongSizes()
        {
            string json = "{\"id\":\"upload\",\"type\":\"attachment\",\"create_at\":1790000000123,\"user_id\":\"user\",\"channel_id\":\"channel\",\"filename\":\"upload.txt\",\"file_size\":4200000000,\"file_offset\":3200000000,\"remote_id\":\"remote\",\"req_file_id\":\"requested\"}";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            UploadSession session = await client.GetUploadAsync("upload");
            Assert.That(session.Id, Is.EqualTo("upload"));
            Assert.That(session.Type, Is.EqualTo(UploadType.Attachment));
            Assert.That(session.CreatedAt, Is.EqualTo(1790000000123L));
            Assert.That(session.UserId, Is.EqualTo("user"));
            Assert.That(session.ChannelId, Is.EqualTo("channel"));
            Assert.That(session.FileName, Is.EqualTo("upload.txt"));
            Assert.That(session.FileSize, Is.EqualTo(4200000000L));
            Assert.That(session.FileOffset, Is.EqualTo(3200000000L));
            Assert.That(session.RemoteId, Is.EqualTo("remote"));
            Assert.That(session.RequestedFileId, Is.EqualTo("requested"));
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(session));
            Assert.That(serialized.RootElement.GetProperty("type").GetString(), Is.EqualTo("attachment"));
        }

        [Test]
        public async Task IncompleteUpload_ReturnsNullAndDisposesResponse()
        {
            using ByteArrayContent content = new ByteArrayContent(Array.Empty<byte>());
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent) { Content = content }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.UploadDataAsync("upload", new byte[] { 1 }), Is.Null);
            await Assert.ThrowsAsync<ObjectDisposedException>(() => content.ReadAsByteArrayAsync());
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.Created)]
        public async Task CompleteUpload_ReturnsFileDetails(HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status,
                "{\"id\":\"file\",\"name\":\"upload.txt\",\"size\":3,\"mime_type\":\"text/plain\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            FileDetails? result = await client.UploadDataAsync("upload", new byte[] { 1, 2, 3 });
            Assert.That(result!.Id, Is.EqualTo("file"));
            Assert.That(result.Name, Is.EqualTo("upload.txt"));
            Assert.That(result.Size, Is.EqualTo(3));
        }

        [Test]
        public async Task UploadData_SnapshotsBufferBeforeAuthentication()
        {
            byte[] data = new byte[] { 1, 2, 3 };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    Array.Fill(data, (byte)9);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                Assert.That(await request.Content!.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UploadDataAsync("upload", data);
        }

        [Test]
        public async Task InvalidSuccessfulJson_IsRejected([Values("create", "get", "data")] string operation,
            [Values("null", "invalid JSON")] string json)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, json)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(client, operation));
        }

        [Test]
        public async Task Errors_PreserveDetails([Values("headers", "preview", "thumbnail", "create", "get", "data")] string operation,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.RequestEntityTooLarge)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"rejected\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, operation));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("rejected"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeHttp([Values("headers", "preview", "thumbnail", "create", "get", "data")] string operation)
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
        public async Task InFlightCancellation_ReachesHttp([Values("headers", "preview", "thumbnail", "create", "get", "data")] string operation)
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

        private static string Response(string operation)
        {
            switch (operation)
            {
                case "create": case "get": return "{\"id\":\"upload\",\"type\":\"attachment\"}";
                case "data": return "{\"id\":\"file\"}";
                case "headers": case "preview": case "thumbnail": return "";
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task InvokeAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "create": return client.CreateUploadAsync(" channel/1 ", " upload.txt ", 4200000000L, token);
                case "get": return client.GetUploadAsync(" id/1?# ", token);
                case "data": return client.UploadDataAsync(" id/1?# ", new byte[] { 1, 2, 3 }, token);
                case "headers": case "preview": case "thumbnail": return HeadersAsync(client, operation, token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static Task<FileHeaders> HeadersAsync(IMattermostClient client, string operation, CancellationToken token = default)
        {
            switch (operation)
            {
                case "headers": return client.GetFileHeadersAsync(" id/1?# ", token);
                case "preview": return client.GetFilePreviewHeadersAsync(" id/1?# ", token);
                case "thumbnail": return client.GetFileThumbnailHeadersAsync(" id/1?# ", token);
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
