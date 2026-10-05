using Mattermost.Models;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    [NonParallelizable]
    internal class TypingTests
    {
        [TestCase(false, 1, TestName = "SendTyping_Channel_ObserverReceivesPulse")]
        [TestCase(true, 1, TestName = "SendTyping_Thread_ObserverReceivesPulse")]
        [TestCase(true, 8, TestName = "SendTyping_ConcurrentCalls_ObserverReceivesEveryPulse")]
        public async Task SendTyping_ObserverReceivesPulses(bool inThread, int pulseCount)
        {
            const string secretsFileName = "secrets.json";
            if (!File.Exists(secretsFileName))
            {
                Assert.Ignore($"{secretsFileName} not found. Provide credentials for two accounts to run this test.");
                return;
            }

            var secrets = JsonSerializer.Deserialize<Secrets>(await File.ReadAllTextAsync(secretsFileName));
            Assert.That(secrets, Is.Not.Null);
            if (string.IsNullOrWhiteSpace(secrets.CustomInstance) || string.IsNullOrWhiteSpace(secrets.Token)
                || string.IsNullOrWhiteSpace(secrets.Username) || string.IsNullOrWhiteSpace(secrets.Password))
            {
                Assert.Ignore("Provide customInstance, token, username, and password in secrets.json to run this test.");
                return;
            }

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var ct = timeout.Token;
            using var sender = new MattermostClient(secrets.CustomInstance, secrets.Token);
            var senderUser = await sender.GetMeAsync();

            // The raw observer can assert the channel in the broadcast envelope, which the SDK event does not expose.
            using var http = new HttpClient { BaseAddress = new Uri(secrets.CustomInstance.TrimEnd('/') + "/") };
            using var login = await http.PostAsJsonAsync("api/v4/users/login",
                new { login_id = secrets.Username, password = secrets.Password }, ct);
            login.EnsureSuccessStatusCode();
            using var observerUser = JsonDocument.Parse(await login.Content.ReadAsStringAsync(ct));
            var observerId = observerUser.RootElement.GetProperty("id").GetString()!;
            Assert.That(senderUser.Id, Is.Not.EqualTo(observerId), "The sender and observer must be different accounts.");
            var observerToken = login.Headers.GetValues("Token").Single();
            var channel = await sender.CreateDirectChannelAsync(observerId);

            using var observer = new ClientWebSocket();
            observer.Options.SetRequestHeader("Authorization", $"Bearer {observerToken}");
            var socketAddress = new UriBuilder(http.BaseAddress)
            {
                Scheme = http.BaseAddress.Scheme == "https" ? "wss" : "ws",
                Path = http.BaseAddress.AbsolutePath.TrimEnd('/') + "/api/v4/websocket"
            }.Uri;
            await observer.ConnectAsync(socketAddress, ct);
            using (var hello = await ReadEventAsync(observer, ct))
                Assert.That(hello.RootElement.GetProperty("event").GetString(), Is.EqualTo("hello"));

            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            sender.OnConnected += (_, _) => connected.TrySetResult(true);
            await sender.StartReceivingAsync(ct);
            await connected.Task.WaitAsync(ct);

            string? rootPostId = null;
            try
            {
                if (inThread)
                    rootPostId = (await sender.CreatePostAsync(channel.Id, $"typing-test-{Guid.NewGuid():N}")).Id;

                IMattermostClient client = sender;
                await Task.WhenAll(Enumerable.Range(0, pulseCount).Select(_ =>
                    Task.Run(() => client.SendTypingAsync(channel.Id, rootPostId ?? "", ct), ct)));

                for (var i = 0; i < pulseCount; i++)
                {
                    JsonDocument pulse;
                    do
                    {
                        pulse = await ReadEventAsync(observer, ct);
                        if (pulse.RootElement.TryGetProperty("event", out var eventName) && eventName.GetString() == "typing")
                            break;
                        pulse.Dispose();
                    } while (true);

                    using (pulse)
                    using (Assert.EnterMultipleScope())
                    {
                        var data = pulse.RootElement.GetProperty("data");
                        Assert.That(data.GetProperty("user_id").GetString(), Is.EqualTo(senderUser.Id));
                        Assert.That(data.GetProperty("parent_id").GetString(), Is.EqualTo(rootPostId ?? ""));
                        Assert.That(pulse.RootElement.GetProperty("broadcast").GetProperty("channel_id").GetString(),
                            Is.EqualTo(channel.Id));
                    }
                }
            }
            finally
            {
                await sender.StopReceivingAsync();
                if (rootPostId != null)
                    await sender.DeletePostAsync(rootPostId);
            }
        }

        private static async Task<JsonDocument> ReadEventAsync(ClientWebSocket socket, CancellationToken ct)
        {
            using var payload = new MemoryStream();
            var buffer = new byte[8192];
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                Assert.That(result.MessageType, Is.EqualTo(WebSocketMessageType.Text));
                payload.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);
            return JsonDocument.Parse(payload.ToArray());
        }
    }
}
