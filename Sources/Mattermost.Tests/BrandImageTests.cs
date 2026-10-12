using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class BrandImageTests
    {
        [Test]
        public async Task Set_UsesNativeMultipartImageAndAcceptsEmptyCreatedBody()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/brand/image"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("multipart/form-data"));
                MultipartFormDataContent content = (MultipartFormDataContent)request.Content;
                HttpContent image = content.Single();
                Assert.That(image.Headers.ContentDisposition!.Name!.Trim('"'), Is.EqualTo("image"));
                Assert.That(image.Headers.ContentDisposition.FileName!.Trim('"'), Is.EqualTo("brand.png"));
                Assert.That(await image.ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                return ApiTestHttp.Json(HttpStatusCode.Created, "");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SetBrandImageAsync(new byte[] { 1, 2, 3 }, " brand.png ");
        }

        [Test]
        public async Task Set_CopiesImageBeforeAuthentication()
        {
            TaskCompletionSource entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    entered.SetResult();
                    await release.Task.WaitAsync(token);
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                MultipartFormDataContent content = (MultipartFormDataContent)request.Content!;
                Assert.That(await content.Single().ReadAsByteArrayAsync(token), Is.EqualTo(new byte[] { 1, 2, 3 }));
                return ApiTestHttp.Json(HttpStatusCode.Created, "");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            byte[] image = new byte[] { 1, 2, 3 };
            Task pending = client.SetBrandImageAsync(image, "brand.png");
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            image[0] = 9;
            release.SetResult();
            await pending.WaitAsync(TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task Set_RejectsNullAndEmptyImageBeforeAuthentication()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SetBrandImageAsync(null!, "brand.png"));
            await Assert.ThrowsAsync<ArgumentException>(() => client.SetBrandImageAsync(Array.Empty<byte>(), "brand.png"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Set_RejectsBlankFilenameBeforeAuthentication(string? fileName)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.SetBrandImageAsync(new byte[] { 1 }, fileName!));
        }

        [Test]
        public async Task Delete_UsesBodylessNativeRequest()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/brand/image"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.DeleteBrandImageAsync();
        }

        [Test]
        public async Task Delete_RejectsMalformedResponse([Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.DeleteBrandImageAsync());
        }

        [Test]
        public async Task Errors_PreserveNativeStatus([Values(false, true)] bool delete,
            [Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.NotImplemented)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"brand operation denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => InvokeAsync(client, delete));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("brand operation denied"));
        }

        [Test]
        public async Task CancellationAndDisposal_RejectBeforeAuthentication([Values(false, true)] bool delete)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => InvokeAsync(client, delete, source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => InvokeAsync(client, delete));
        }

        [Test]
        public async Task Cancellation_ReachesRequest([Values(false, true)] bool delete)
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
            Task pending = InvokeAsync(client, delete, source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task InvokeAsync(MattermostClient client, bool delete, CancellationToken token = default)
        {
            if (delete) { return client.DeleteBrandImageAsync(token); }
            return client.SetBrandImageAsync(new byte[] { 1 }, "brand.png", token);
        }
    }
}
