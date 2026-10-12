using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class AdminNotificationIntentTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task NotifyAdmin_UsesCurrentUserRouteAndExactNativeBody(bool trial)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/users/notify-admin"));
                Assert.That(request.RequestUri.Query, Is.Empty);
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EquivalentTo(new[] { "required_plan", "required_feature", "trial_notification" }));
                Assert.That(body.RootElement.GetProperty("required_plan").GetString(), Is.EqualTo("professional"));
                Assert.That(body.RootElement.GetProperty("required_feature").GetString(), Is.EqualTo("mattermost.feature.guest_accounts"));
                Assert.That(body.RootElement.GetProperty("trial_notification").GetBoolean(), Is.EqualTo(trial));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.NotifyAdminAsync(" professional ", " mattermost.feature.guest_accounts ", trial);
        }

        [Test]
        public async Task NotifyAdmin_PreservesEmptyPlanForPluginSpecificRequests()
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.GetProperty("required_plan").GetString(), Is.Empty);
                Assert.That(body.RootElement.GetProperty("required_feature").GetString(), Is.EqualTo("mattermost.feature.plugin.example"));
                Assert.That(body.RootElement.GetProperty("trial_notification").GetBoolean(), Is.False);
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.NotifyAdminAsync("", "mattermost.feature.plugin.example");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void NotifyAdmin_BlankFeatureIsRejected(string? feature)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.NotifyAdminAsync("professional", feature!))!.ParamName, Is.EqualTo("requiredFeature"));
        }

        [Test]
        public void NotifyAdmin_NullPlanIsRejected()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentNullException>(() => client.NotifyAdminAsync(null!, "mattermost.feature.guest_accounts"))!.ParamName, Is.EqualTo("requiredPlan"));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task NotifyAdmin_PropagatesNativeFailureWithoutRetry(HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot save intent\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.NotifyAdminAsync("professional", "mattermost.feature.guest_accounts")))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot save intent"));
            Assert.That(error.RequestMethod, Is.EqualTo("POST"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [Test]
        public void NotifyAdmin_CancelledOrDisposedCallSendsNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.NotifyAdminAsync("professional", "mattermost.feature.guest_accounts", cancellationToken: cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.NotifyAdminAsync("professional", "mattermost.feature.guest_accounts"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task NotifyAdmin_CancellationStopsAuthenticationOrRequest(bool authentication)
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
            Task pending = client.NotifyAdminAsync("professional", "mattermost.feature.guest_accounts", cancellationToken: cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending);
        }
    }
}
