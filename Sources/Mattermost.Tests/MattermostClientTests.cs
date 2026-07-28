using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Events;
using Mattermost.Exceptions;
using Mattermost.Models;
using Mattermost.Models.Channels;
using Mattermost.Models.Dialogs;
using Mattermost.Models.Posts;
using Mattermost.Models.Responses.Websocket.Posts;
using Mattermost.Models.Teams;
using Mattermost.Models.Users;
using System.Text;
using System.Text.Json;

namespace Mattermost.Tests
{
    [SingleThreaded]
    public class MattermostClientTests
    {
        private string email = string.Empty;
        private string password = string.Empty;
        private string token = string.Empty;
        private string customInstance = string.Empty;
        IMattermostClient client;

        [SetUp]
        [OneTimeSetUp]
        public async Task Setup()
        {
            string json = File.ReadAllText("secrets.json");
            var secrets = JsonSerializer.Deserialize<Secrets>(json)!;
            email = secrets.Username;
            password = secrets.Password;
            token = secrets.Token;
            customInstance = secrets.CustomInstance;
            var mmClient = (IMattermostClient)new MattermostClient();
            client = mmClient;
            await client.LoginAsync(email, password);
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
            if (string.IsNullOrWhiteSpace(customInstance))
            {
                Assert.Ignore("Custom instance is not set, skipping autologin test.");
            }
            if (string.IsNullOrEmpty(token))
            {
                Assert.Ignore("Token is empty, skipping autologin test.");
            }

            MattermostClient mmClient = new(customInstance, token);
            var user = await mmClient.GetMeAsync();
            Assert.That(mmClient.CurrentUserInfo, Is.Not.Null, "User should not be null after autologin.");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Username, Is.Not.Null, "Username should not be null.");
                Assert.That(user.Email, Is.Not.Empty, "Email should not be empty.");
                Assert.That(user.Id, Is.Not.Empty, "User ID should not be empty.");
                Assert.That(user.Username, Is.Not.Empty, "Username should not be empty.");
                Assert.That(user.Locale, Is.Not.Empty, "Locale should not be empty.");
                Assert.That(user.IsBot, Is.True, "User should be a bot."); // Testing mm chatgpt bot, which is a bot user.
                Assert.That(user.Timezone, Is.Not.Null, "Timezone should not be null.");
                Assert.That(user.CreatedAt, Is.Not.Default, "CreatedAt should not be default value.");
                Assert.That(user.UpdatedAt, Is.Not.Default, "UpdatedAt should not be default value.");
            }
        }

        [Test]
        [NonParallelizable]
        public void AutologinTest_InvalidToken_ThrowsException()
        {
            MattermostClient mmClient = new("https://community.mattermost.com", "invalid_token");
            Assert.ThrowsAsync<ApiKeyException>(mmClient.GetMeAsync);
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_ProvidedToken_LoginThrowsException()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(email, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
            }
            MattermostClient mmClient = new("https://community.mattermost.com", "abcabcabc");
            Assert.ThrowsAsync<AuthorizationException>(async () => await mmClient.LoginAsync(email, password));
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_ValidCredentials_ReturnsToken()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(email, Is.Not.Empty);
                Assert.That(password, Is.Not.Empty);
            }
            User result = client.CurrentUserInfo;
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Username, Is.Not.Null);
                Assert.That(result.Email, Is.EqualTo(email));
                Assert.That(result.Id, Is.Not.Empty);
                Assert.That(result.Username, Is.Not.Empty);
                Assert.That(result.Locale, Is.Not.Empty);
                Assert.That(result.IsBot, Is.False);
                Assert.That(result.Timezone, Is.Not.Null);
                Assert.That(result.CreatedAt, Is.Not.Default);
                Assert.That(result.UpdatedAt, Is.Not.Default);
            }
        }

        [Test]
        [NonParallelizable]
        public void LoginTest_InvalidCredentials_ThrowsException()
        {
            Assert.That(email, Is.Not.Empty);
            Assert.ThrowsAsync<AuthorizationException>(async () => await client.LoginAsync(email, "invalid"));
        }

        [Test]
        [NonParallelizable]
        public async Task GetUserByUsername_ValidUsername_ReceivedUserInfo()
        {
            const string rawUsername = "bvdcode"; // This is a valid username in the Mattermost community server.
            var user = await client.GetUserByUsernameAsync(rawUsername);
            Assert.That(user, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Username, Is.EqualTo(rawUsername));
                Assert.That(user.Email, Is.EqualTo(email));
                Assert.That(user.Id, Is.Not.Empty);
                Assert.That(user.Locale, Is.Not.Empty);
                Assert.That(user.IsBot, Is.False);
                Assert.That(user.Timezone, Is.Not.Null);
                Assert.That(user.CreatedAt, Is.Not.Default);
                Assert.That(user.UpdatedAt, Is.Not.Default);
            }
        }

        [Test]
        [NonParallelizable]
        public async Task GetUserById_ValidId_ReceivedUserInfo()
        {
            const string userId = "nm7fy5aztjgx9qqyj6qieca47c"; //
            // This is a valid user ID in the Mattermost community server.
            var user = await client.GetUserAsync(userId);
            Assert.That(user, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.Id, Is.EqualTo(userId));
                Assert.That(user.Username, Is.Not.Empty);
                Assert.That(user.Email, Is.EqualTo(email));
                Assert.That(user.Locale, Is.Not.Empty);
                Assert.That(user.IsBot, Is.False);
                Assert.That(user.Timezone, Is.Not.Null);
                Assert.That(user.CreatedAt, Is.Not.Default);
                Assert.That(user.UpdatedAt, Is.Not.Default);
            }
        }

        [Test]
        [NonParallelizable]
        public async Task ConnectWebSocket_ServerConnected()
        {
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
            const string message = "dick"; // I use Moderation Bot for testing, so I can send a message to it and it will respond with a message.
            const string botId = "ca6ni33mjpfafjw7uiy7tafznr";
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
            Assert.That(receivedMessages[0].Post.Text, Is.EqualTo("_A post with potentially offensive content was flagged and removed._"));
        }

        [Test]
        [NonParallelizable]
        public async Task SendOwnMessage_IgnoreOwnMessagesDisabled_ReceivedFromEvent()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            string message = $"self-message-test-{Guid.NewGuid():N}";
            TaskCompletionSource<bool> ownMessageReceived = new();
            var configurableClient = client as MattermostClient ?? throw new InvalidOperationException("Client should be MattermostClient.");

            configurableClient.Options.IgnoreOwnMessages = false;
            void handler(object? sender, MessageEventArgs e)
            {
                if (string.Equals(e.Message.Post.Text, message, StringComparison.Ordinal) && e.IsCurrentUser)
                {
                    ownMessageReceived.TrySetResult(true);
                }
            }
            client.OnMessageReceived += handler;

            try
            {
                await client.StartReceivingAsync();
                for (int i = 0; i < 20 && !client.IsConnected; i++)
                {
                    await Task.Delay(250);
                }
                Assert.That(client.IsConnected, Is.True, "WebSocket should be connected before sending a message.");
                await client.CreatePostAsync(channelId, message);

                var completedTask = await Task.WhenAny(ownMessageReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
                Assert.That(completedTask, Is.EqualTo(ownMessageReceived.Task), "Own message event should be dispatched when IgnoreOwnMessages is disabled.");
            }
            finally
            {
                client.OnMessageReceived -= handler;
                configurableClient.Options.IgnoreOwnMessages = true;
                configurableClient.Options.IncomingMessageFilter = null;
                try
                {
                    await client.StopReceivingAsync();
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        [Test]
        [NonParallelizable]
        public async Task SendOwnMessage_IncomingMessageFilterReturnsFalse_EventNotReceived()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            string message = $"self-message-filter-false-{Guid.NewGuid():N}";
            TaskCompletionSource<bool> ownMessageReceived = new();
            var configurableClient = client as MattermostClient ?? throw new InvalidOperationException("Client should be MattermostClient.");

            configurableClient.Options.IgnoreOwnMessages = false;
            configurableClient.Options.IncomingMessageFilter = e => !e.IsCurrentUser;
            void handler(object? sender, MessageEventArgs e)
            {
                if (string.Equals(e.Message.Post.Text, message, StringComparison.Ordinal) && e.IsCurrentUser)
                {
                    ownMessageReceived.TrySetResult(true);
                }
            }
            client.OnMessageReceived += handler;

            try
            {
                await client.StartReceivingAsync();
                for (int i = 0; i < 20 && !client.IsConnected; i++)
                {
                    await Task.Delay(250);
                }
                Assert.That(client.IsConnected, Is.True, "WebSocket should be connected before sending a message.");
                await client.CreatePostAsync(channelId, message);

                var completedTask = await Task.WhenAny(ownMessageReceived.Task, Task.Delay(TimeSpan.FromSeconds(5)));
                Assert.That(completedTask, Is.Not.EqualTo(ownMessageReceived.Task), "Own message event should be filtered out when IncomingMessageFilter returns false.");
            }
            finally
            {
                client.OnMessageReceived -= handler;
                configurableClient.Options.IgnoreOwnMessages = true;
                configurableClient.Options.IncomingMessageFilter = null;
                try
                {
                    await client.StopReceivingAsync();
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        [Test]
        [NonParallelizable]
        public async Task SendOwnMessage_IncomingMessageFilterReturnsTrue_EventReceived()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            string message = $"self-message-filter-true-{Guid.NewGuid():N}";
            TaskCompletionSource<bool> ownMessageReceived = new();
            var configurableClient = client as MattermostClient ?? throw new InvalidOperationException("Client should be MattermostClient.");

            configurableClient.Options.IgnoreOwnMessages = false;
            configurableClient.Options.IncomingMessageFilter = e => e.IsCurrentUser;
            void handler(object? sender, MessageEventArgs e)
            {
                if (string.Equals(e.Message.Post.Text, message, StringComparison.Ordinal) && e.IsCurrentUser)
                {
                    ownMessageReceived.TrySetResult(true);
                }
            }
            client.OnMessageReceived += handler;

            try
            {
                await client.StartReceivingAsync();
                for (int i = 0; i < 20 && !client.IsConnected; i++)
                {
                    await Task.Delay(250);
                }
                Assert.That(client.IsConnected, Is.True, "WebSocket should be connected before sending a message.");
                await client.CreatePostAsync(channelId, message);

                var completedTask = await Task.WhenAny(ownMessageReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
                Assert.That(completedTask, Is.EqualTo(ownMessageReceived.Task), "Own message event should be dispatched when IncomingMessageFilter returns true.");
            }
            finally
            {
                client.OnMessageReceived -= handler;
                configurableClient.Options.IgnoreOwnMessages = true;
                configurableClient.Options.IncomingMessageFilter = null;
                try
                {
                    await client.StopReceivingAsync();
                }
                catch (OperationCanceledException)
                {
                }
            }
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
        [NonParallelizable]
        public async Task DiscoveryApis_FindUsersTeamsAndChannels()
        {
            Team coreTeam = await client.GetTeamByNameAsync("core");
            IList<Team> teams = await client.GetTeamsAsync(perPage: 100);
            IList<Channel> channels = await client.GetTeamChannelsAsync(coreTeam.Id, perPage: 100);
            IList<User> teamUsers = await client.GetUsersAsync(perPage: 10, inTeamId: coreTeam.Id, active: true);
            IList<User> searchResults = await client.SearchUsersAsync(client.CurrentUserInfo.Username, teamId: coreTeam.Id, limit: 10);

            Assert.That(coreTeam.Id, Is.Not.Empty);
            Assert.That(coreTeam.Name, Is.EqualTo("core"));
            Assert.That(teams.Any(item => item.Id == coreTeam.Id), Is.True);
            Assert.That(channels, Is.Not.Empty);
            Assert.That(channels.All(item => item.TeamId == coreTeam.Id), Is.True);
            Assert.That(teamUsers, Is.Not.Empty);
            Assert.That(searchResults.Any(item => item.Id == client.CurrentUserInfo.Id), Is.True);
        }

        [Test]
        public async Task GetUserByEmail_ValidEmail_ReceivedUserInfo()
        {
            Assert.That(email, Is.Not.Empty);
            try
            {
                var user = await client.GetUserByEmailAsync(email);
                Assert.That(user, Is.Not.Null);
                Assert.That(user.Email, Is.EqualTo(email));
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
            var channel = await client.CreateDirectChannelAsync("zsdnqzetgj83xrwxxrze3i188r");
            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.ChannelType, Is.EqualTo(ChannelType.Direct));
        }

        [Test]
        public async Task CreatePostWithProps_ValidProps_ReceivedPostInfo()
        {
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
        [NonParallelizable]
        public async Task CreatePostWithPersistentNotifications_LiveInstanceAcceptsPriorityMetadata()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            Post? createdPost = null;

            try
            {
                string message = "@"
                    + client.CurrentUserInfo.Username
                    + " Persistent notification smoke "
                    + Guid.NewGuid().ToString("N");
                createdPost = await client.CreatePostAsync(
                    channelId,
                    message,
                    priority: MessagePriority.Urgent,
                    requestedAck: true,
                    persistentNotifications: true);

                Assert.That(createdPost.ChannelId, Is.EqualTo(channelId));
                await client.AddReactionAsync(createdPost.Id, "white_check_mark");
            }
            finally
            {
                if (createdPost is not null)
                {
                    await client.DeletePostAsync(createdPost.Id);
                }
            }
        }

        [Test]
        [NonParallelizable]
        public async Task PostInteractions_CreatePost_ReactionAndPinRoundTrip()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            const string emojiName = "white_check_mark";
            Post? createdPost = null;

            try
            {
                createdPost = await client.CreatePostAsync(channelId, "Post interaction test " + Guid.NewGuid().ToString("N"));
                Reaction reaction = await client.AddReactionAsync(createdPost.Id, emojiName);
                IList<Reaction> reactions = await client.GetReactionsAsync(createdPost.Id);

                Assert.That(reaction.PostId, Is.EqualTo(createdPost.Id));
                Assert.That(reaction.UserId, Is.EqualTo(client.CurrentUserInfo.Id));
                Assert.That(reaction.EmojiName, Is.EqualTo(emojiName));
                Assert.That(
                    reactions.Any(item =>
                        item.PostId == createdPost.Id
                        && item.UserId == client.CurrentUserInfo.Id
                        && item.EmojiName == emojiName),
                    Is.True);

                await client.RemoveReactionAsync(createdPost.Id, emojiName);
                IList<Reaction> reactionsAfterDelete = await client.GetReactionsAsync(createdPost.Id);
                Assert.That(
                    reactionsAfterDelete.Any(item =>
                        item.UserId == client.CurrentUserInfo.Id
                        && item.EmojiName == emojiName),
                    Is.False);

                await client.PinPostAsync(createdPost.Id);
                Post pinnedPost = await client.GetPostAsync(createdPost.Id);
                Assert.That(pinnedPost.IsPinned, Is.True);

                await client.UnpinPostAsync(createdPost.Id);
                Post unpinnedPost = await client.GetPostAsync(createdPost.Id);
                Assert.That(unpinnedPost.IsPinned, Is.False);
            }
            finally
            {
                if (createdPost is not null)
                {
                    await client.DeletePostAsync(createdPost.Id);
                }
            }
        }

        [Test]
        [NonParallelizable]
        public async Task CreatePostWithActionStyle_GetPost_StyleIsPersisted()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";
            const ActionStyle expectedStyle = ActionStyle.Danger;

            PostProps props = new();
            props.Attachments.Add(new PostPropsAttachment
            {
                Text = "Attachment with action style",
                Actions =
                {
                    new PostPropsAction
                    {
                        Id = "action-style-test",
                        Name = "Danger action",
                        Style = expectedStyle,
                        Integration = new Integration
                        {
                            Url = "https://example.com/action-style-test"
                        }
                    }
                }
            });

            var createdPost = await client.CreatePostAsync(channelId, "Test post with action style", props: props);
            var loadedPost = await client.GetPostAsync(createdPost.Id);

            Assert.That(loadedPost.Props.Attachments, Is.Not.Empty, "Post attachments should not be empty.");
            Assert.That(loadedPost.Props.Attachments[0].Actions, Is.Not.Empty, "Post attachment actions should not be empty.");
            Assert.That(loadedPost.Props.Attachments[0].Actions[0].Style, Is.EqualTo(expectedStyle), "Action style should be preserved after loading post.");
        }

        [Test]
        [NonParallelizable]
        public async Task CreatePostWithSelectActions_GetPost_ActionFieldsArePersisted()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";

            PostProps props = new();
            props.Attachments.Add(new PostPropsAttachment
            {
                Text = "Attachment with select actions",
                Actions =
                {
                    new PostPropsSelectAction
                    {
                        Id = "actionoptions",
                        Name = "Select an option...",
                        DefaultOption = "opt2",
                        Integration = new Integration
                        {
                            Url = "https://example.com/actionoptions",
                            Context =
                            {
                                ["action"] = "select_static_option"
                            }
                        },
                        Options = new List<PostActionOption>
                        {
                            new PostActionOption("Option1", "opt1"),
                            new PostActionOption("Option2", "opt2"),
                            new PostActionOption("Option3", "opt3")
                        }
                    },
                    new PostPropsSelectAction
                    {
                        Id = "actionusers",
                        Name = "Select a user...",
                        DataSource = PostActionDataSource.Users,
                        Integration = new Integration
                        {
                            Url = "https://example.com/actionusers",
                            Context =
                            {
                                ["action"] = "select_user"
                            }
                        }
                    }
                }
            });

            var createdPost = await client.CreatePostAsync(channelId, "Test post with select actions", props: props);
            var loadedPost = await client.GetPostAsync(createdPost.Id);

            Assert.That(loadedPost.Props.Attachments, Is.Not.Empty, "Post attachments should not be empty.");
            Assert.That(loadedPost.Props.Attachments[0].Actions, Has.Count.EqualTo(2), "Post actions should be preserved.");

            PostPropsAction staticSelect = loadedPost.Props.Attachments[0].Actions.Single(action => action.Id == "actionoptions");
            Assert.That(staticSelect.Type, Is.EqualTo(PostActionType.Select));
            Assert.That(staticSelect.DefaultOption, Is.EqualTo("opt2"));
            Assert.That(staticSelect.Options, Has.Count.EqualTo(3));
            Assert.That(staticSelect.Options![1].Text, Is.EqualTo("Option2"));
            Assert.That(staticSelect.Options[1].Value, Is.EqualTo("opt2"));

            PostPropsAction usersSelect = loadedPost.Props.Attachments[0].Actions.Single(action => action.Id == "actionusers");
            Assert.That(usersSelect.Type, Is.EqualTo(PostActionType.Select));
            Assert.That(usersSelect.DataSource, Is.EqualTo(PostActionDataSource.Users));
        }

        [Test]
        [NonParallelizable]
        public async Task OpenInteractiveDialog_PostActionTrigger_LiveInstanceOpensDialog()
        {
            const string channelId = "w5e788utqbfgickdfgsabp8wya";

            using HttpClient httpClient = new HttpClient();
            string webhookToken = await CreateWebhookTokenAsync(httpClient);
            string actionUrl = "https://webhook.site/" + webhookToken;
            string actionId = "dialog-smoke-" + Guid.NewGuid().ToString("N");
            Post? createdPost = null;

            try
            {
                string authToken = await LoginForApiTokenAsync(httpClient);
                PostProps props = CreateDialogSmokePostProps(actionId, actionUrl);
                createdPost = await client.CreatePostAsync(
                    channelId,
                    "Dialog live smoke " + Guid.NewGuid().ToString("N"),
                    props: props);

                await PerformPostActionAsync(httpClient, authToken, createdPost.Id, actionId);
                PostActionIntegrationRequest actionRequest = await WaitForActionCallbackAsync(httpClient, webhookToken);

                using MattermostClient unauthenticatedClient = new MattermostClient();
                await unauthenticatedClient.OpenInteractiveDialogAsync(
                    actionRequest.TriggerId,
                    actionUrl,
                    CreateLiveSmokeDialog());

                Assert.That(actionRequest.UserId, Is.EqualTo(client.CurrentUserInfo.Id));
                Assert.That(actionRequest.PostId, Is.EqualTo(createdPost.Id));
                Assert.That(actionRequest.TriggerId, Is.Not.Empty);
                Assert.That(actionRequest.Context["action"].GetString(), Is.EqualTo("open_dialog_smoke"));
            }
            finally
            {
                if (createdPost is not null)
                {
                    await client.DeletePostAsync(createdPost.Id);
                }

                await DeleteWebhookTokenAsync(httpClient, webhookToken);
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
            await client.LogoutAsync();
            Assert.ThrowsAsync<AuthorizationException>(client.GetMeAsync);
        }

        private async Task<string> LoginForApiTokenAsync(HttpClient httpClient)
        {
            using StringContent content = CreateJsonContent(new
            {
                login_id = email,
                password
            });
            using HttpResponseMessage response = await httpClient.PostAsync(BuildApiUri("users/login"), content);
            response.EnsureSuccessStatusCode();

            if (!response.Headers.TryGetValues("Token", out IEnumerable<string>? tokenValues))
            {
                throw new AssertionException("Mattermost login response did not include a token header.");
            }

            string token = tokenValues.FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AssertionException("Mattermost login token header was empty.");
            }

            return token;
        }

        private async Task<string> CreateWebhookTokenAsync(HttpClient httpClient)
        {
            using StringContent content = CreateJsonContent(new
            {
                default_status = 200,
                default_content = "{}",
                default_content_type = "application/json",
                expiry = 3600
            });
            using HttpResponseMessage response = await httpClient.PostAsync("https://webhook.site/token", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(json);

            return document.RootElement.GetProperty("uuid").GetString()
                ?? throw new AssertionException("Webhook token response did not include uuid.");
        }

        private static async Task DeleteWebhookTokenAsync(HttpClient httpClient, string webhookToken)
        {
            if (string.IsNullOrWhiteSpace(webhookToken))
            {
                return;
            }

            using HttpResponseMessage response = await httpClient.DeleteAsync("https://webhook.site/token/" + webhookToken);
        }

        private async Task PerformPostActionAsync(HttpClient httpClient, string authToken, string postId, string actionId)
        {
            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                BuildApiUri("posts/" + postId + "/actions/" + actionId));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private static async Task<PostActionIntegrationRequest> WaitForActionCallbackAsync(
            HttpClient httpClient,
            string webhookToken)
        {
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                using HttpResponseMessage response = await httpClient.GetAsync(
                    "https://webhook.site/token/" + webhookToken + "/requests?sorting=newest");
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement data = document.RootElement.GetProperty("data");
                if (data.GetArrayLength() == 0)
                {
                    continue;
                }

                string content = data[0].GetProperty("content").GetString() ?? string.Empty;
                PostActionIntegrationRequest? request = JsonSerializer.Deserialize<PostActionIntegrationRequest>(content);
                if (request is not null && !string.IsNullOrWhiteSpace(request.TriggerId))
                {
                    return request;
                }
            }

            throw new AssertionException("Mattermost did not send a post action callback with trigger_id.");
        }

        private Uri BuildApiUri(string route)
        {
            return new Uri(client.ServerAddress, "/api/v4/" + route);
        }

        private static StringContent CreateJsonContent(object payload)
        {
            return new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        }

        private static PostProps CreateDialogSmokePostProps(string actionId, string actionUrl)
        {
            PostProps props = new PostProps();
            props.Attachments.Add(new PostPropsAttachment
            {
                Text = "Open dialog smoke",
                Actions =
                {
                    new PostPropsButtonAction
                    {
                        Id = actionId,
                        Name = "Open",
                        Integration = new Integration
                        {
                            Url = actionUrl,
                            Context =
                            {
                                ["action"] = "open_dialog_smoke"
                            }
                        }
                    }
                }
            });

            return props;
        }

        private static InteractiveDialog CreateLiveSmokeDialog()
        {
            return new InteractiveDialog
            {
                Title = "Live dialog smoke",
                Elements = new List<InteractiveDialogElement>
                {
                    new InteractiveDialogElement
                    {
                        DisplayName = "Summary",
                        Name = "summary",
                        Type = InteractiveDialogElementType.Text
                    }
                }
            };
        }
    }
}
