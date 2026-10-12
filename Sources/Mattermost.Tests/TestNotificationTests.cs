using Mattermost.Exceptions;
using System.Net;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TestNotificationTests
    {
        [Test]
        public async Task SendTestNotification_UsesNativeRouteWithoutBody()
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/notifications/test"));
                Assert.That(request.RequestUri.Query, Is.Empty);
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.SendTestNotificationAsync();
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task SendTestNotification_PropagatesNativeErrorsWithoutRetry(HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot send test\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.SendTestNotificationAsync()))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot send test"));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void SendTestNotification_CancelledOrDisposedCallSendsNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.SendTestNotificationAsync(cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.SendTestNotificationAsync());
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task SendTestNotification_CancellationStopsAuthenticationOrRequest(bool authentication)
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
            Task pending = client.SendTestNotificationAsync(cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending);
        }
    }
}
