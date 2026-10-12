using Mattermost.Exceptions;
using Mattermost.Models.Users;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class AuthenticationTransportTests
    {
        private const string UserJson = "{\"id\":\"transport-user\",\"username\":\"transport\"}";
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

        [Test]
        public async Task ApiToken_IgnoresResponseCookiesAndRetainsBearerAuthentication([Values(false, true)] bool uriConstructor)
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            string server = GetServerUrl(listener);
            using CancellationTokenSource timeout = new CancellationTokenSource(RequestTimeout);
            Task<IList<string>> received = ReceiveAsync(listener, timeout.Token,
                Response(UserJson, "Set-Cookie: MMAUTHTOKEN=cookie-session; Path=/\r\n"), Response(UserJson), Response("{}"));
            using MattermostClient client = uriConstructor
                ? new MattermostClient(new Uri(server), "api-token")
                : new MattermostClient(server, "api-token");

            User user = await client.GetMeAsync().WaitAsync(timeout.Token);
            Assert.That(user.Id, Is.EqualTo("transport-user"));
            await client.LogoutAsync().WaitAsync(timeout.Token);
            IList<string> requests = await received;

            Assert.That(requests[0], Does.StartWith("GET /api/v4/users/me HTTP/1.1\r\n"));
            Assert.That(requests[1], Does.StartWith("GET /api/v4/users/me HTTP/1.1\r\n"));
            Assert.That(requests[2], Does.StartWith("POST /api/v4/users/logout HTTP/1.1\r\n"));
            Assert.That(requests.All(request => request.Contains("\r\nAuthorization: Bearer api-token\r\n", StringComparison.Ordinal)), Is.True);
            Assert.That(requests.All(request => !request.Contains("\r\nCookie:", StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task PasswordLogin_UsesTokenHeaderWithoutCookiesAndLogoutClearsSession([Values(false, true)] bool uriConstructor)
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            string server = GetServerUrl(listener);
            using CancellationTokenSource timeout = new CancellationTokenSource(RequestTimeout);
            Task<IList<string>> received = ReceiveAsync(listener, timeout.Token,
                Response(UserJson, "Token: login-token\r\nSet-Cookie: MMAUTHTOKEN=cookie-session; Path=/\r\n"),
                Response(UserJson), Response("{}"));
            using MattermostClient client = uriConstructor ? new MattermostClient(new Uri(server)) : new MattermostClient(server);

            User loggedIn = await client.LoginAsync("transport", "test-password").WaitAsync(timeout.Token);
            Assert.That(loggedIn.Id, Is.EqualTo("transport-user"));
            Assert.That((await client.GetMeAsync().WaitAsync(timeout.Token)).Id, Is.EqualTo(loggedIn.Id));
            await client.LogoutAsync().WaitAsync(timeout.Token);
            await Assert.ThrowsAsync<AuthorizationException>(() => client.GetMeAsync());
            IList<string> requests = await received;

            Assert.That(requests[0], Does.StartWith("POST /api/v4/users/login HTTP/1.1\r\n"));
            Assert.That(requests[0], Does.Not.Contain("\r\nAuthorization:"));
            using JsonDocument login = JsonDocument.Parse(requests[0].Split("\r\n\r\n", 2)[1]);
            Assert.That(login.RootElement.GetProperty("login_id").GetString(), Is.EqualTo("transport"));
            Assert.That(login.RootElement.GetProperty("password").GetString(), Is.EqualTo("test-password"));
            Assert.That(requests.Skip(1).All(request => request.Contains("\r\nAuthorization: Bearer login-token\r\n", StringComparison.Ordinal)), Is.True);
            Assert.That(requests.All(request => !request.Contains("\r\nCookie:", StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task ExternalTransport_PreservesCookieSettingsHeadersAndOwnership([Values(false, true)] bool useCookies,
            [Values(false, true)] bool uriConstructor)
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            string server = GetServerUrl(listener);
            using CancellationTokenSource timeout = new CancellationTokenSource(RequestTimeout);
            Task<IList<string>> received = ReceiveAsync(listener, timeout.Token, Response(UserJson), Response(UserJson), Response("{}"));
            CookieContainer cookies = new CookieContainer();
            cookies.Add(new Uri(server), new Cookie("MMAUTHTOKEN", "external-session", "/"));
            using HttpClientHandler handler = new HttpClientHandler { UseCookies = useCookies, CookieContainer = cookies };
            using HttpClient http = new HttpClient(handler) { Timeout = RequestTimeout, BaseAddress = new Uri(server + "/external/") };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "ZXh0ZXJuYWw=");
            using MattermostClient client = uriConstructor
                ? new MattermostClient(new Uri(server), "api-token", http)
                : new MattermostClient(server, "api-token", http);

            await client.GetMeAsync().WaitAsync(timeout.Token);
            client.Dispose();
            using HttpResponseMessage response = await http.GetAsync(server + "/probe", timeout.Token);
            IList<string> requests = await received;

            Assert.That(handler.UseCookies, Is.EqualTo(useCookies));
            Assert.That(handler.CookieContainer, Is.SameAs(cookies));
            Assert.That(cookies.GetCookies(new Uri(server))["MMAUTHTOKEN"]!.Value, Is.EqualTo("external-session"));
            Assert.That(http.Timeout, Is.EqualTo(RequestTimeout));
            Assert.That(http.BaseAddress, Is.EqualTo(new Uri(server + "/external/")));
            Assert.That(http.DefaultRequestHeaders.Authorization!.Scheme, Is.EqualTo("Basic"));
            Assert.That(requests.Take(2).All(request => request.Contains("\r\nAuthorization: Bearer api-token\r\n", StringComparison.Ordinal)), Is.True);
            Assert.That(requests.All(request => request.Contains("\r\nCookie: MMAUTHTOKEN=external-session\r\n", StringComparison.Ordinal) == useCookies), Is.True);
            Assert.That(requests[2], Does.Contain("\r\nAuthorization: Basic ZXh0ZXJuYWw=\r\n"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task OwnedTransport_RetainsTimeoutAndIsDisposedWithClient()
        {
            using MattermostClient client = new MattermostClient("https://mattermost.example");
            HttpClient http = client.HttpClient;
            Assert.That(http.Timeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
            client.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => http.GetAsync("https://mattermost.example"));
        }

        private static string GetServerUrl(TcpListener listener)
        {
            IPEndPoint address = (IPEndPoint)listener.LocalEndpoint;
            return "http://127.0.0.1:" + address.Port.ToString(CultureInfo.InvariantCulture);
        }

        private static string Response(string body, string headers = "")
        {
            return "HTTP/1.1 200 OK\r\nConnection: close\r\nContent-Type: application/json\r\nContent-Length: "
                + Encoding.UTF8.GetByteCount(body).ToString(CultureInfo.InvariantCulture) + "\r\n" + headers + "\r\n" + body;
        }

        private static async Task<IList<string>> ReceiveAsync(TcpListener listener, CancellationToken token, params string[] responses)
        {
            List<string> requests = new List<string>();
            foreach (string response in responses)
            {
                using TcpClient connection = await listener.AcceptTcpClientAsync(token);
                using NetworkStream stream = connection.GetStream();
                StringBuilder headers = new StringBuilder();
                byte[] next = new byte[1];
                while (!headers.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
                {
                    Assert.That(headers.Length, Is.LessThan(32768));
                    await stream.ReadExactlyAsync(next, token);
                    headers.Append((char)next[0]);
                }
                string? contentLength = headers.ToString().Split("\r\n")
                    .SingleOrDefault(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
                int length = contentLength is null ? 0 : int.Parse(contentLength.Split(':', 2)[1], CultureInfo.InvariantCulture);
                byte[] body = new byte[length];
                await stream.ReadExactlyAsync(body, token);
                requests.Add(headers + Encoding.UTF8.GetString(body));
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response), token);
            }
            return requests;
        }
    }
}
