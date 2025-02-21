using System.Text.Json;
using Mattermost.Models;
using Mattermost.Exceptions;
using Mattermost.Models.Users;
using Mattermost.Models.Responses.Websocket.Posts;

namespace Mattermost.Tests
{
    [SingleThreaded]
    public class MattermostClientTests
    {
        private string username = string.Empty;
        private string password = string.Empty;
        private string token = string.Empty;
        IMattermostClient client;

        [SetUp]
        [OneTimeSetUp]
        public async Task Setup()
        {
            string json = File.ReadAllText("secrets.json");
            var secrets = JsonSerializer.Deserialize<Secrets>(json)!;
            username = secrets.Username;
            password = secrets.Password;
            token = secrets.Token;
            var mmClient = (IMattermostClient)new MattermostClient();
            client = mmClient;
            await client.LoginAsync(username, password);
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_ValidCredentials_ReturnsToken()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            User result = client.CurrentUserInfo;
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Username, Is.Not.Null);
                Assert.That(result.Email, Is.EqualTo(username));
                Assert.That(result.Id, Is.Not.Empty);
                Assert.That(result.Username, Is.Not.Empty);
                Assert.That(result.Locale, Is.Not.Empty);
                Assert.That(result.IsBot, Is.False);
                Assert.That(result.Timezone, Is.Not.Null);
                Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
                Assert.That(result.UpdatedAt, Is.Not.EqualTo(default(DateTime)));
            });
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_InvalidCredentials_ThrowsException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            Assert.ThrowsAsync<AuthorizationException>(async () => await client.LoginAsync(username, "invalid"));
        }

        [Test]
        [NonParallelizable]
        public async Task ConnectWebSocket_ServerConnected()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            await client.StartReceivingAsync();
            await Task.Delay(1000);
            Assert.That(client.IsConnected, Is.True);
            await client.StopReceivingAsync();
            await Task.Delay(1000);
            Assert.That(client.IsConnected, Is.False);
        }

        [Test]
        [NonParallelizable]
        public async Task SendMessageToBot_ReceivedFromEvent()
        {
            const string message = "/ping";
            const string botId = "w5e788utqbfgickdfgsabp8wya";
            await client.StartReceivingAsync();
            await Task.Delay(1000);
            List<PostInfo> receivedMessages = [];
            client.OnMessageReceived += (sender, e) =>
            {
                receivedMessages.Add(e.Message);
            };
            await client.CreatePostAsync(botId, message);
            await Task.Delay(1000);
            Assert.That(receivedMessages, Is.Not.Empty);
            Assert.That(receivedMessages[0].Post.Text, Is.EqualTo(":tada: Thanks for helping us make Mattermost better!"));
        }

        [Test]
        [NonParallelizable]
        public async Task GetChannelPosts_ReceivedPosts()
        {
            const string channelId = "k71ypb7hxpb7jx7ygs9b4rf6gy"; // https://community.mattermost.com/core/channels/off-topic-pub
            var result = await client.GetChannelPostsAsync(channelId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Posts, Is.Not.Empty);
            Assert.That(result.Posts, Is.Not.Null);
        }

        [Test]
        [NonParallelizable]
        public async Task GetChannelPosts_UseDateTime_ReceivedPosts()
        {
            const string channelId = "k71ypb7hxpb7jx7ygs9b4rf6gy"; // https://community.mattermost.com/core/channels/off-topic-pub
            var result = await client.GetChannelPostsAsync(channelId, since: DateTime.UtcNow.AddDays(-15));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Posts, Is.Not.Empty);
            Assert.That(result.Posts, Is.Not.Null);
        }

        [Test]
        public void DisposeClient_SendRequest_ThrowsException()
        {
            var client = new MattermostClient();
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { try { client.LoginAsync(string.Empty).Wait(); } catch (AggregateException ex) { throw ex.InnerException!; } });
        }

        /// <summary>
        /// Don't rename this test, it should be started with 'Z' to be the last one.
        /// </summary>
        /// <returns></returns>
        [Test]
        [NonParallelizable]
        public async Task Z_Logout_Successful()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            await client.LogoutAsync();
            Assert.ThrowsAsync<ApiKeyException>(client.GetMeAsync);
        }
    }
}