using System.Net;
using System.Text;

namespace Mattermost.Tests
{
    internal static class ApiTestHttp
    {
        internal static MattermostClient Client(HttpClient http) => new MattermostClient("https://mattermost.example/chat", "test-token", http);

        internal static HttpClient RejectRequests() => new HttpClient(new DelegateHttpMessageHandler((_, _) => throw new AssertionException("Unexpected HTTP request.")));

        internal static HttpClient Create(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            return new HttpClient(new DelegateHttpMessageHandler((request, token) =>
            {
                Assert.That(request.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
                Assert.That(request.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
                if (request.RequestUri!.AbsolutePath == "/chat/api/v4/users/me")
                {
                    return Task.FromResult(Json(HttpStatusCode.OK, "{\"id\":\"user-1\"}"));
                }
                return respond(request, token);
            }));
        }

        internal static HttpResponseMessage Json(HttpStatusCode status, string body) => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
