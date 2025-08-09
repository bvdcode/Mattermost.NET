using System.Text.Json;
using Mattermost.Enums;
using Mattermost.Models;
using Mattermost.Constants;
using Mattermost.Exceptions;
using Mattermost.Models.Users;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses.Websocket.Posts;
using System.Threading.Tasks;

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
        public void UserInfo_GetUserInfo_ThrowsExceptionIfNotLoggedIn()
        {
            MattermostClient mmClient = new("https://community.mattermost.com");
            Assert.Throws<AuthorizationException>(() => _ = mmClient.CurrentUserInfo, "CurrentUserInfo should throw an exception if not logged in.");
        }

        [Test]
        [NonParallelizable]
        public async Task AutologinTest_ValidToken_Works()
        {
            Assert.Multiple(() =>
            {
                Assert.That(token, Is.Not.Empty);
            });

            MattermostClient mmClient = new("https://mm.tmk-group.digital", token);
            var user = await mmClient.GetMeAsync();
            Assert.That(mmClient.CurrentUserInfo, Is.Not.Null, "User should not be null after autologin.");
            Assert.Multiple(() =>
            {
                Assert.That(user.Username, Is.Not.Null, "Username should not be null.");
                Assert.That(user.Email, Is.EqualTo(username), "Email should match the autologin username.");
                Assert.That(user.Id, Is.Not.Empty, "User ID should not be empty.");
                Assert.That(user.Username, Is.Not.Empty, "Username should not be empty.");
                Assert.That(user.Locale, Is.Not.Empty, "Locale should not be empty.");
                Assert.That(user.IsBot, Is.False, "User should not be a bot.");
                Assert.That(user.Timezone, Is.Not.Null, "Timezone should not be null.");
                Assert.That(user.CreatedAt, Is.Not.EqualTo(default(DateTime)), "CreatedAt should not be default value.");
                Assert.That(user.UpdatedAt, Is.Not.EqualTo(default(DateTime)), "UpdatedAt should not be default value.");
            });
        }

        [Test]
        [NonParallelizable]
        public void AutologinTest_InvalidToken_ThrowsException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(token, Is.Not.Empty);
            });
            MattermostClient mmClient = new("https://community.mattermost.com", "invalid_token");
            Assert.ThrowsAsync<AuthorizationException>(mmClient.GetMeAsync);
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_ValidToken_LoginThrowsException()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            MattermostClient mmClient = new("https://community.mattermost.com", token);
            Assert.ThrowsAsync<AuthorizationException>(async () => await mmClient.LoginAsync(username, password));
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_ValidCredentials_ReturnsToken()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
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
        public void SendMessage_BigText_ThrowsException()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            string message = "A".PadRight(MattermostApiLimits.MaxPostMessageLength + 1, 'A');
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.CreatePostAsync(channelId, message));
        }

        [Test]
        [NonParallelizable]
        public void EditMessage_BigText_ThrowsException()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            string message = "A".PadRight(MattermostApiLimits.MaxPostMessageLength + 1, 'A');
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await client.UpdatePostAsync(channelId, message));
        }

        [Test]
        [NonParallelizable]
        public async Task GetChannelPosts_ReceivedPosts()
        {
            const string channelId = "k71ypb7hxpb7jx7ygs9b4rf6gy"; // https://community.mattermost.com/core/channels/off-topic-pub
            var result = await client.GetChannelPostsAsync(channelId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Posts, Is.Not.Empty);
        }

        [Test]
        [NonParallelizable]
        public async Task GetThreadPosts_ReceivedPosts()
        {
            const string postId = "z6adks4emffu7cspkh6asjorkw"; // https://community.mattermost.com/core/messages/@feedbackbot
            var result = await client.GetThreadPostsAsync(postId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Posts, Is.Not.Empty);
            Assert.That(result.Posts, Has.Count.GreaterThan(1));
        }

        [Test]
        [NonParallelizable]
        public async Task GetChannelPosts_UseDateTime_ReceivedPosts()
        {
            const string channelId = "k71ypb7hxpb7jx7ygs9b4rf6gy"; // https://community.mattermost.com/core/channels/off-topic-pub
            var result = await client.GetChannelPostsAsync(channelId, since: DateTime.UtcNow.AddDays(-15));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Posts, Is.Not.Empty);
        }

        [Test]
        [NonParallelizable]
        public async Task GetChannelInfo_ReceivedChannelInfo()
        {
            const string channelId = "k71ypb7hxpb7jx7ygs9b4rf6gy"; // https://community.mattermost.com/core/channels/off-topic-pub
            var result = await client.GetChannelAsync(channelId);
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(channelId));
                Assert.That(result.Name, Is.EqualTo("off-topic-pub"));
            }
        }

        [Test]
        public async Task GetUserByEmail_ValidEmail_ReceivedUserInfo()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            try
            {
                var user = await client.GetUserByEmailAsync(username);
                Assert.That(user, Is.Not.Null);
                Assert.That(user.Email, Is.EqualTo(username));
            }
            catch (MattermostClientException ex)
            {
                if (ex.Message.Contains("Access to user information by email is forbidden"))
                {
                    Assert.Pass();
                    return;
                }
            }
        }

        [Test]
        public async Task CreateDirectChannel_ValidUserId_ReceivedChannelInfo()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            var channel = await client.CreateDirectChannelAsync("zsdnqzetgj83xrwxxrze3i188r");
            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.ChannelType, Is.EqualTo(ChannelType.Direct));
        }

        [Test]
        public async Task CreatePostWithProps_ValidProps_ReceivedPostInfo()
        {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
                Assert.That(token, Is.Not.Empty);
            });
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            PostProps props = new();
            props.Attachments.Add(new PostPropsAttachment()
            {
                Text = "Attachment text",
            });
            var post = await client.CreatePostAsync(channelId, "Test post with props", props: props);
            Assert.That(post, Is.Not.Null);
            Assert.That(post.RawProps, Is.Not.Null);
            Assert.That(post.RawProps, Is.Not.Empty, "Post properties should not be empty.");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(post.RawProps, Contains.Key("attachments"), "Post properties should contain 'attachments' key.");
                Assert.That(post.Props, Is.Not.Null, "Post properties should not be null.");
                Assert.That(post.Props.Attachments, Is.Not.Null, "Post properties attachments should not be null.");
                Assert.That(post.Props.Attachments, Is.Not.Empty, "Post properties attachments should not be empty.");
            }
        }

        [Test]
        public void DisposeClient_SendRequest_ThrowsException()
        {
            var client = new MattermostClient();
            client.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { try { client.GetMeAsync().Wait(); } catch (AggregateException ex) { throw ex.InnerException!; } });
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
            Assert.ThrowsAsync<AuthorizationException>(client.GetMeAsync);
        }
    }
}