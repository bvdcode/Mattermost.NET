using Mattermost.Exceptions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class TypingNotificationTests
    {
        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase(" root-id ", "root-id")]
        public async Task SendTyping_PublishesForCurrentUser_WithoutWebSocket(string? parentId, string expectedParentId)
        {
            int notifications = 0;
            using HttpClient http = CreateHttpClient(async (request, cancellationToken) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri, Is.EqualTo(new Uri("https://mattermost.example/chat/api/v4/users/me/typing")));
                Assert.That(request.Content?.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                Assert.That(body.RootElement.GetProperty("channel_id").GetString(), Is.EqualTo("channel-id"));
                Assert.That(body.RootElement.GetProperty("parent_id").GetString(), Is.EqualTo(expectedParentId));
                notifications++;
                return JsonResponse(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = new MattermostClient("https://mattermost.example/chat/", "test-token", http);

            IMattermostClient api = client;
            await api.SendTypingAsync(" channel-id ", parentId);
            await client.StopReceivingAsync();
            await api.SendTypingAsync("channel-id", parentId);

            Assert.That(notifications, Is.EqualTo(2));
            Assert.That(client.IsConnected, Is.False);
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task SendTyping_ServerRejectsRequest_ReportsFailure(HttpStatusCode status)
        {
            using HttpClient http = CreateHttpClient((_, _) => Task.FromResult(JsonResponse(status,
                "{\"message\":\"Typing request rejected\",\"id\":\"typing.rejected\"}")));
            using MattermostClient client = new MattermostClient("https://mattermost.example/chat/", "test-token", http);

            MattermostClientException exception = (await Assert.ThrowsAsync<MattermostClientException>(() => client.SendTypingAsync("channel-id")))!;
            Assert.That(exception.StatusCode, Is.EqualTo(status));
            Assert.That(exception.RequestMethod, Is.EqualTo(HttpMethod.Post.Method));
            Assert.That(exception.Message, Is.EqualTo("Typing request rejected"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void SendTyping_InvalidChannel_DoesNotSendRequest(string? channelId)
        {
            using MattermostClient client = new MattermostClient();
            Assert.Throws<ArgumentException>(() => client.SendTypingAsync(channelId!));
        }

        [Test]
        public void SendTyping_CancelledBeforeCall_DoesNotAuthenticate()
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                throw new AssertionException("An already-cancelled call must not send HTTP requests.")));
            using MattermostClient client = new MattermostClient("https://mattermost.example", "test-token", http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.Throws<OperationCanceledException>(() => client.SendTypingAsync("channel-id", cancellationToken: cancellation.Token));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task SendTyping_CancellationStopsPendingHttpRequest(bool duringAuthentication)
        {
            using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            TaskCompletionSource<bool> started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (request, token) =>
            {
                if (!duringAuthentication && request.Method == HttpMethod.Get)
                {
                    return JsonResponse(HttpStatusCode.OK, "{\"id\":\"current-user\"}");
                }
                started.TrySetResult(true);
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new AssertionException("The pending request should be cancelled.");
            }));
            using MattermostClient client = new MattermostClient("https://mattermost.example", "test-token", http);

            Task pending = client.SendTypingAsync("channel-id", cancellationToken: cancellation.Token);
            await started.Task.WaitAsync(deadline.Token);
            cancellation.Cancel();

            await Assert.CatchAsync<OperationCanceledException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public async Task SendTyping_ConcurrentCalls_CompleteIndependently()
        {
            const int count = 8;
            int arrived = 0;
            using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            TaskCompletionSource<bool> ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = CreateHttpClient(async (_, token) =>
            {
                if (Interlocked.Increment(ref arrived) == count)
                {
                    ready.TrySetResult(true);
                }
                await ready.Task.WaitAsync(token);
                return JsonResponse(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = new MattermostClient("https://mattermost.example/chat/", "test-token", http);

            await Task.WhenAll(Enumerable.Range(0, count).Select(_ =>
                client.SendTypingAsync("channel-id", "root-id", deadline.Token)));

            Assert.That(arrived, Is.EqualTo(count));
            Assert.That(client.IsConnected, Is.False);
        }

        [Test]
        public void SendTyping_DisposedClient_IsRejected()
        {
            using MattermostClient client = new MattermostClient();
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.SendTypingAsync("channel-id"));
        }

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> publish)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization?.Parameter, Is.EqualTo("test-token"));
                if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"id\":\"current-user\"}"));
                }
                return publish(request, token);
            }));
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json)
        {
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
