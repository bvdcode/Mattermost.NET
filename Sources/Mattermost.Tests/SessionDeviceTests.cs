using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class SessionDeviceTests
    {
        [Test]
        public async Task Update_SendsOnlySuppliedNativeStringProperties([Values(false, true)] bool disabled)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo("/chat/api/v4/users/sessions/device"));
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name),
                    Is.EquivalentTo(new[] { "device_id", "voip_device_id", "device_notification_disabled", "mobile_version" }));
                Assert.That(body.RootElement.EnumerateObject().All(field => field.Value.ValueKind == JsonValueKind.String), Is.True);
                Assert.That(body.RootElement.GetProperty("device_id").GetString(), Is.EqualTo("android_rn:device+1"));
                Assert.That(body.RootElement.GetProperty("voip_device_id").GetString(), Is.EqualTo("apple_rn:voice/1"));
                Assert.That(body.RootElement.GetProperty("mobile_version").GetString(), Is.EqualTo("2.12.0-beta.1+build.7"));
                Assert.That(body.RootElement.GetProperty("device_notification_disabled").GetString(), Is.EqualTo(disabled.ToString().ToLowerInvariant()));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await client.UpdateSessionDeviceAsync("android_rn:device+1", "apple_rn:voice/1", disabled, "2.12.0-beta.1+build.7");
        }

        [TestCase("device_id", " android_rn:exact ")]
        [TestCase("voip_device_id", " apple_rn:exact ")]
        [TestCase("mobile_version", " 2.12.0 ")]
        [TestCase("device_notification_disabled", "false")]
        public async Task Update_OmitsOtherPropertiesAndPreservesExactText(string key, string value)
        {
            using HttpClient http = ApiTestHttp.Create(async (request, token) =>
            {
                using JsonDocument body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.That(body.RootElement.EnumerateObject().Select(field => field.Name), Is.EqualTo(new[] { key }));
                Assert.That(body.RootElement.GetProperty(key).GetString(), Is.EqualTo(value));
                return ApiTestHttp.Json(HttpStatusCode.OK, "{\"status\":\"OK\"}");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await UpdatePropertyAsync(client, key, value);
        }

        [Test]
        public async Task Update_RejectsMissingPropertiesBeforeAuthentication()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateSessionDeviceAsync());
        }

        [Test]
        public async Task Update_RejectsBlankTextBeforeAuthentication([Values("device_id", "voip_device_id", "mobile_version")] string key,
            [Values("", " ", "\t")] string value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => UpdatePropertyAsync(client, key, value));
        }

        [Test]
        public async Task Update_PreservesNativeErrors([Values(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)] HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"invalid session property\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.UpdateSessionDeviceAsync(mobileVersion: "invalid-version"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("invalid session property"));
        }

        [Test]
        public async Task Update_RejectsMalformedResponse([Values("null", "invalid JSON")] string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.UpdateSessionDeviceAsync(notificationDisabled: true));
        }

        [Test]
        public async Task Update_RejectsCancellationAndDisposalBeforeAuthentication()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource source = new CancellationTokenSource();
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => client.UpdateSessionDeviceAsync(notificationDisabled: false, cancellationToken: source.Token));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.UpdateSessionDeviceAsync(notificationDisabled: false));
        }

        [Test]
        public async Task Update_PropagatesRequestCancellation()
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
            Task pending = client.UpdateSessionDeviceAsync(notificationDisabled: true, cancellationToken: source.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await source.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5)));
        }

        private static Task UpdatePropertyAsync(MattermostClient client, string key, string value)
        {
            switch (key)
            {
                case "device_id": return client.UpdateSessionDeviceAsync(deviceId: value);
                case "voip_device_id": return client.UpdateSessionDeviceAsync(voipDeviceId: value);
                case "mobile_version": return client.UpdateSessionDeviceAsync(mobileVersion: value);
                case "device_notification_disabled": return client.UpdateSessionDeviceAsync(notificationDisabled: false);
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }
    }
}
