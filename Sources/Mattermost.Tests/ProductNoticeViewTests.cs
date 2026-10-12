using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ProductNoticeViewTests
    {
        [Test]
        public async Task MarkProductNoticesViewed_SendsTrimmedSnapshotAsNativeArray()
        {
            List<string> ids = new List<string> { " notice-1 ", " notice/2?x#y ", "notice-1" };
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    ids.Clear();
                    ids.Add("changed");
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri.AbsolutePath, Is.EqualTo("/chat/api/v4/system/notices/view"));
                Assert.That(request.RequestUri.Query, Is.Empty);
                Assert.That(request.Headers.Authorization!.Parameter, Is.EqualTo("test-token"));
                Assert.That(request.Content!.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
                using JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
                Assert.That(body.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
                Assert.That(body.RootElement.EnumerateArray().Select(item => item.GetString()), Is.EqualTo(new[] { "notice-1", "notice/2?x#y", "notice-1" }));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.MarkProductNoticesViewedAsync(ids);
        }

        [Test]
        public void MarkProductNoticesViewed_NullOrEmptyCollectionIsRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentNullException>(() => client.MarkProductNoticesViewedAsync(null!))!.ParamName, Is.EqualTo("noticeIds"));
            Assert.That(Assert.Throws<ArgumentException>(() => client.MarkProductNoticesViewedAsync(Array.Empty<string>()))!.ParamName, Is.EqualTo("noticeIds"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void MarkProductNoticesViewed_BlankElementIsRejected(string? value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.MarkProductNoticesViewedAsync(new[] { "notice-1", value! }))!.ParamName, Is.EqualTo("noticeIds"));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task MarkProductNoticesViewed_PropagatesNativeErrorsWithoutRetry(HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot record views\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.MarkProductNoticesViewedAsync(new[] { "notice-1" })))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot record views"));
            Assert.That(error.RequestMethod, Is.EqualTo("PUT"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void MarkProductNoticesViewed_CancelledOrDisposedCallSendsNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.MarkProductNoticesViewedAsync(new[] { "notice-1" }, cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.MarkProductNoticesViewedAsync(new[] { "notice-1" }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task MarkProductNoticesViewed_CancellationStopsAuthenticationOrRequest(bool authentication)
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!authentication && request.RequestUri!.AbsolutePath.EndsWith("/users/me", StringComparison.Ordinal))
                {
                    return ApiTestHttp.Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("Expected cancellation.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            Task pending = client.MarkProductNoticesViewedAsync(new[] { "notice-1" }, cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending);
        }
    }
}
