using Mattermost.Exceptions;
using Mattermost.Models.OAuth;
using System.Net;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class OAuthMetadataTests
    {
        private const string MetadataJson = "{\"issuer\":\"https://mattermost.example/chat\",\"authorization_endpoint\":\"https://mattermost.example/chat/oauth/authorize\",\"token_endpoint\":\"https://mattermost.example/chat/oauth/access_token\",\"registration_endpoint\":\"https://mattermost.example/chat/api/v4/oauth/apps/register\",\"response_types_supported\":[\"code\",\"future_response\"],\"scopes_supported\":[\"user\"],\"grant_types_supported\":[\"authorization_code\",\"refresh_token\"],\"token_endpoint_auth_methods_supported\":[\"none\",\"client_secret_post\"],\"code_challenge_methods_supported\":[\"S256\",\"future_challenge\"]}";

        [TestCase("", "/.well-known/oauth-authorization-server")]
        [TestCase("/chat", "/chat/.well-known/oauth-authorization-server")]
        [TestCase("/chat/", "/chat/.well-known/oauth-authorization-server")]
        [TestCase("/chat%20root", "/chat%20root/.well-known/oauth-authorization-server")]
        public async Task Metadata_UsesAnonymousGetAndPreservesServerSubpath(string subpath, string path)
        {
            int count = 0;
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                count++;
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(request.RequestUri!.PathAndQuery, Is.EqualTo(path));
                Assert.That(request.Headers.Authorization, Is.Null);
                Assert.That(request.Content, Is.Null);
                return Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, MetadataJson));
            }));
            using MattermostClient implementation = new MattermostClient("https://mattermost.example" + subpath, "unused-token", http);
            IMattermostIntegrationsClient client = implementation;
            OAuthServerMetadata metadata = await client.GetOAuthServerMetadataAsync();
            Assert.That(count, Is.EqualTo(1));
            Assert.That(metadata.Issuer, Is.EqualTo("https://mattermost.example/chat"));
            Assert.That(metadata.AuthorizationEndpoint, Is.EqualTo("https://mattermost.example/chat/oauth/authorize"));
            Assert.That(metadata.TokenEndpoint, Is.EqualTo("https://mattermost.example/chat/oauth/access_token"));
            Assert.That(metadata.RegistrationEndpoint, Is.EqualTo("https://mattermost.example/chat/api/v4/oauth/apps/register"));
            Assert.That(metadata.ResponseTypesSupported, Is.EqualTo(new[] { "code", "future_response" }));
            Assert.That(metadata.ScopesSupported, Is.EqualTo(new[] { "user" }));
            Assert.That(metadata.GrantTypesSupported, Is.EqualTo(new[] { "authorization_code", "refresh_token" }));
            Assert.That(metadata.TokenEndpointAuthMethodsSupported, Is.EqualTo(new[] { "none", "client_secret_post" }));
            Assert.That(metadata.CodeChallengeMethodsSupported, Is.EqualTo(new[] { "S256", "future_challenge" }));
            using JsonDocument original = JsonDocument.Parse(MetadataJson);
            using JsonDocument serialized = JsonDocument.Parse(JsonSerializer.Serialize(metadata));
            Assert.That(serialized.RootElement.EnumerateObject().Count(), Is.EqualTo(9));
            foreach (JsonProperty field in original.RootElement.EnumerateObject())
            {
                Assert.That(serialized.RootElement.GetProperty(field.Name).GetRawText(), Is.EqualTo(field.Value.GetRawText()));
            }
        }

        [Test]
        public async Task Metadata_PreservesMissingOptionalEndpointsAndCollections()
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((_, _) => Task.FromResult(
                ApiTestHttp.Json(HttpStatusCode.OK, "{\"issuer\":\"https://example.org\",\"response_types_supported\":[\"code\"]}"))));
            using MattermostClient client = ApiTestHttp.Client(http);
            OAuthServerMetadata metadata = await client.GetOAuthServerMetadataAsync();
            Assert.That(metadata.AuthorizationEndpoint, Is.Empty);
            Assert.That(metadata.TokenEndpoint, Is.Empty);
            Assert.That(metadata.RegistrationEndpoint, Is.Empty);
            Assert.That(metadata.ScopesSupported, Is.Empty);
            Assert.That(metadata.GrantTypesSupported, Is.Empty);
            Assert.That(metadata.TokenEndpointAuthMethodsSupported, Is.Empty);
            Assert.That(metadata.CodeChallengeMethodsSupported, Is.Empty);
        }

        [TestCase(HttpStatusCode.NotImplemented)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.NotFound)]
        public async Task Metadata_PreservesNativeErrors(HttpStatusCode status)
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((_, _) => Task.FromResult(ApiTestHttp.Json(status,
                "{\"id\":\"oauth_metadata_unavailable\",\"message\":\"Unavailable\",\"status_code\":" + (int)status + "}"))));
            using MattermostClient client = ApiTestHttp.Client(http);
            MattermostClientException? error = await Assert.ThrowsAsync<MattermostClientException>(async () => await client.GetOAuthServerMetadataAsync());
            Assert.That(error!.StatusCode, Is.EqualTo(status));
            Assert.That(error.RequestMethod, Is.EqualTo("GET"));
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{invalid")]
        [TestCase("{\"issuer\":17}")]
        [TestCase("{\"response_types_supported\":\"code\"}")]
        [TestCase("{\"code_challenge_methods_supported\":[17]}")]
        public async Task Metadata_RejectsMissingOrMalformedObjectResponse(string response)
        {
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler((_, _) => Task.FromResult(ApiTestHttp.Json(HttpStatusCode.OK, response))));
            using MattermostClient client = ApiTestHttp.Client(http);
            await Assert.ThrowsAsync<JsonException>(async () => await client.GetOAuthServerMetadataAsync());
        }

        [Test]
        public async Task Metadata_RejectsDisposedClient()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await client.GetOAuthServerMetadataAsync());
        }

        [Test]
        public async Task Metadata_CancelsBeforeRequest()
        {
            using HttpClient http = ApiTestHttp.RejectRequests();
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await client.GetOAuthServerMetadataAsync(cancellation.Token));
        }

        [Test]
        public async Task Metadata_CancelsInFlightRequest()
        {
            TaskCompletionSource<bool> entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using HttpClient http = new HttpClient(new DelegateHttpMessageHandler(async (_, token) =>
            {
                entered.SetResult(true);
                await Task.Delay(Timeout.Infinite, token);
                throw new AssertionException("Expected cancellation.");
            }));
            using MattermostClient client = ApiTestHttp.Client(http);
            using CancellationTokenSource cancellation = new CancellationTokenSource();
            Task request = client.GetOAuthServerMetadataAsync(cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await cancellation.CancelAsync();
            await Assert.CatchAsync<OperationCanceledException>(async () => await request);
        }
    }
}
