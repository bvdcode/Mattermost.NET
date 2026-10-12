using Mattermost.Exceptions;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class RedirectLocationTests
    {
        [TestCase("https://example.com/next")]
        [TestCase("/relative?one=1&two=2")]
        [TestCase("")]
        [TestCase("  /next  ")]
        public async Task Get_EncodesUrlAndPreservesLocation(string location)
        {
            const string url = "https://example.com/путь?q=a%20b&next=/two#fragment";
            using HttpClient http = ApiTestHttp.Create((request, _) =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/chat/api/v4/redirect_location"));
                Assert.That(request.RequestUri.Query, Is.EqualTo("?url=" + Uri.EscapeDataString(url)));
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, JsonSerializer.Serialize(new { location })));
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetRedirectLocationAsync("  " + url + "  "), Is.EqualTo(location));
        }

        [TestCase("http://example.com/")]
        [TestCase("https://example.com/")]
        public async Task Get_AcceptsHttpAndHttps(string url)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, "{\"location\":\"\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            Assert.That(await client.GetRedirectLocationAsync(url), Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" \t ")]
        [TestCase("/relative")]
        [TestCase("not a url")]
        [TestCase("file:///tmp/file")]
        [TestCase("ftp://example.com/file")]
        public async Task Get_RejectsInvalidUrlBeforeAuthentication(string? url)
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetRedirectLocationAsync(url!));
        }

        [TestCase("null")]
        [TestCase("invalid JSON")]
        [TestCase("{}")]
        [TestCase("{\"location\":null}")]
        [TestCase("{\"location\":42}")]
        [TestCase("{\"Location\":\"/next\"}")]
        public async Task Get_RejectsMalformedResponse(string body)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, body)));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(() => client.GetRedirectLocationAsync("https://example.com/"));
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Get_PreservesNativeErrors(HttpStatusCode status)
        {
            using HttpClient http = ApiTestHttp.Create((_, _) => Task.FromResult(ApiTestHttp.Json(status, "{\"message\":\"redirect request denied\"}")));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(() => client.GetRedirectLocationAsync("https://example.com/"));
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.Message, Is.EqualTo("redirect request denied"));
        }

        [Test]
        public async Task Get_RejectsCancellationBeforeAuthentication()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.GetRedirectLocationAsync("https://example.com/", cancellation.Token));
        }

        [Test]
        public async Task Get_RejectsDisposedClient()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            MattermostClient client = ApiTestHttp.Client(http);
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetRedirectLocationAsync("https://example.com/"));
        }

        [Test]
        public async Task Get_PassesRequestCancellation()
        {
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            using HttpClient http = ApiTestHttp.Create(async (_, token) =>
            {
                Assert.That(token.CanBeCanceled, Is.True);
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
                throw new AssertionException("Expected canceled request.");
            });
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThatAsync(() => client.GetRedirectLocationAsync("https://example.com/", cancellation.Token), Throws.InstanceOf<OperationCanceledException>());
        }
    }
}
