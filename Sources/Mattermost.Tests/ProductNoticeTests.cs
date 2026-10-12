using Mattermost.Enums;
using Mattermost.Exceptions;
using Mattermost.Models.Notices;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ProductNoticeTests
    {
        [TestCase(ProductNoticeClientType.Web, "web")]
        [TestCase(ProductNoticeClientType.Desktop, "desktop")]
        [TestCase(ProductNoticeClientType.MobileIos, "mobile-ios")]
        [TestCase(ProductNoticeClientType.MobileAndroid, "mobile-android")]
        public async Task GetProductNotices_UsesNativeRouteQueryAndProjection(ProductNoticeClientType platform, string nativePlatform)
        {
            const string response = "[{\"id\":\"notice-1\",\"title\":\"{{Mattermost}} news\",\"description\":\"**Text**\",\"action\":\"url\",\"actionParam\":\"/console/site?x=1\",\"actionText\":\"Open\",\"image\":\"/image.png\",\"sysAdminOnly\":true,\"teamAdminOnly\":false}]";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/system/notices/team%2F1%3Fx%23y"));
                Assert.That(request.RequestUri.Query, Is.EqualTo("?client=" + nativePlatform + "&clientVersion=12.0.0%2Bbuild%26x%3D1&locale=en%2FUS%3Fx%26y%23z"));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            IList<ProductNotice> result = await client.GetProductNoticesAsync(" team/1?x#y ", platform, " 12.0.0+build&x=1 ", " en/US?x&y#z ");
            Assert.That(result, Has.Count.EqualTo(1));
            ProductNotice notice = result[0];
            Assert.Multiple(() =>
            {
                Assert.That(notice.Id, Is.EqualTo("notice-1"));
                Assert.That(notice.Title, Is.EqualTo("{{Mattermost}} news"));
                Assert.That(notice.Description, Is.EqualTo("**Text**"));
                Assert.That(notice.Action, Is.EqualTo(ProductNoticeAction.Url));
                Assert.That(notice.ActionParam, Is.EqualTo("/console/site?x=1"));
                Assert.That(notice.ActionText, Is.EqualTo("Open"));
                Assert.That(notice.Image, Is.EqualTo("/image.png"));
                Assert.That(notice.SysAdminOnly, Is.True);
                Assert.That(notice.TeamAdminOnly, Is.False);
            });
            using JsonDocument native = JsonDocument.Parse(response);
            Assert.That(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(result), native.RootElement), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public async Task GetProductNotices_BlankLocaleIsOmittedAndEmptyListIsAccepted(string? locale)
        {
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.RequestUri!.Query, Is.EqualTo("?client=web&clientVersion=12.0.0"));
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "[]"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "12.0.0", locale), Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task GetProductNotices_OptionalFieldsPreserveAbsenceAndExplicitEmptyValues(bool explicitEmpty)
        {
            string optional = explicitEmpty ? ",\"action\":null,\"actionParam\":\"\",\"actionText\":\"\",\"image\":\"\"" : "";
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK,
                "[{\"id\":\"notice-2\",\"title\":\"Title\",\"description\":\"Description\",\"sysAdminOnly\":false,\"teamAdminOnly\":true" + optional + "}]")));
            using MattermostClient client = ApiTestHttp.Client(http);
            ProductNotice result = (await client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Desktop, "1.0.0"))[0];
            Assert.That(result.Action, Is.Null);
            Assert.That(result.SysAdminOnly, Is.False);
            Assert.That(result.TeamAdminOnly, Is.True);
            if (explicitEmpty)
            {
                Assert.That(result.ActionParam, Is.Empty);
                Assert.That(result.ActionText, Is.Empty);
                Assert.That(result.Image, Is.Empty);
            }
            else
            {
                Assert.That(result.ActionParam, Is.Null);
                Assert.That(result.ActionText, Is.Null);
                Assert.That(result.Image, Is.Null);
            }
            JsonElement serialized = JsonSerializer.SerializeToElement(result);
            Assert.That(serialized.TryGetProperty("action", out _), Is.False);
            Assert.That(serialized.TryGetProperty("actionParam", out _), Is.EqualTo(explicitEmpty));
            Assert.That(serialized.TryGetProperty("actionText", out _), Is.EqualTo(explicitEmpty));
            Assert.That(serialized.TryGetProperty("image", out _), Is.EqualTo(explicitEmpty));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void GetProductNotices_BlankTeamIsRejected(string? teamId)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetProductNoticesAsync(teamId!, ProductNoticeClientType.Web, "1.0.0"))!.ParamName,
                Is.EqualTo("teamId"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t")]
        public void GetProductNotices_BlankVersionIsRejected(string? version)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentException>(() => client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, version!))!.ParamName,
                Is.EqualTo("clientVersion"));
        }

        [TestCase(-1)]
        [TestCase(4)]
        [TestCase(int.MaxValue)]
        public void GetProductNotices_UnknownClientIsRejected(int value)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => client.GetProductNoticesAsync("team-1", (ProductNoticeClientType)value, "1.0.0"))!.ParamName,
                Is.EqualTo("client"));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task GetProductNotices_NativeErrorsPropagateWithoutRetry(HttpStatusCode status)
        {
            int requests = 0;
            using HttpClient http = ApiTestHttp.Create((_, _) =>
            {
                requests++;
                return Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"Cannot get notices\"}"));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException error = (await Assert.ThrowsAsync<MattermostClientException>(() => client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "1.0.0")))!;
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("Cannot get notices"));
            Assert.That(error.RequestMethod, Is.EqualTo("GET"));
            Assert.That(requests, Is.EqualTo(1));
        }

        [TestCase("null")]
        [TestCase("{}")]
        [TestCase("{broken")]
        [TestCase("[{\"action\":\"unsupported\"}]")]
        public async Task GetProductNotices_InvalidResponseIsRejected(string response)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.CatchAsync<JsonException>(() => client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "1.0.0"));
        }

        [Test]
        public void GetProductNotices_CancelledOrDisposedCallSendsNothing()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "1.0.0", cancellationToken: cancellation.Token));
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "1.0.0"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task GetProductNotices_CancellationStopsAuthenticationOrRequest(bool authentication)
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
            Task<IList<ProductNotice>> pending = client.GetProductNoticesAsync("team-1", ProductNoticeClientType.Web, "1.0.0", cancellationToken: cancellation.Token);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await pending);
        }
    }
}
