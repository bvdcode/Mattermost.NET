using Mattermost.Models.Posts;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class PostActionIntegrationRequestTests
    {
        [Test]
        public void Deserialize_MapsInteractiveActionCallbackPayload()
        {
            const string json = "{" +
                "\"user_id\":\"user-id\"," +
                "\"user_name\":\"alice\"," +
                "\"channel_id\":\"channel-id\"," +
                "\"channel_name\":\"town-square\"," +
                "\"team_id\":\"team-id\"," +
                "\"team_domain\":\"example-team\"," +
                "\"post_id\":\"post-id\"," +
                "\"trigger_id\":\"trigger-id\"," +
                "\"type\":\"button\"," +
                "\"data_source\":\"\"," +
                "\"context\":{\"action\":\"approve\",\"pr\":1234}" +
                "}";

            PostActionIntegrationRequest request = JsonSerializer.Deserialize<PostActionIntegrationRequest>(json)!;

            Assert.That(request.UserId, Is.EqualTo("user-id"));
            Assert.That(request.UserName, Is.EqualTo("alice"));
            Assert.That(request.ChannelId, Is.EqualTo("channel-id"));
            Assert.That(request.ChannelName, Is.EqualTo("town-square"));
            Assert.That(request.TeamId, Is.EqualTo("team-id"));
            Assert.That(request.TeamName, Is.EqualTo("example-team"));
            Assert.That(request.PostId, Is.EqualTo("post-id"));
            Assert.That(request.TriggerId, Is.EqualTo("trigger-id"));
            Assert.That(request.Type, Is.EqualTo("button"));
            Assert.That(request.DataSource, Is.Empty);
            Assert.That(request.Context["action"].GetString(), Is.EqualTo("approve"));
            Assert.That(request.Context["pr"].GetInt32(), Is.EqualTo(1234));
        }

        [Test]
        public void Deserialize_KeepsSelectedOptionInContext()
        {
            const string json = "{" +
                "\"user_id\":\"user-id\"," +
                "\"channel_id\":\"channel-id\"," +
                "\"post_id\":\"post-id\"," +
                "\"trigger_id\":\"trigger-id\"," +
                "\"type\":\"select\"," +
                "\"data_source\":\"users\"," +
                "\"context\":{\"action\":\"assign\",\"selected_option\":\"selected-user-id\"}" +
                "}";

            PostActionIntegrationRequest request = JsonSerializer.Deserialize<PostActionIntegrationRequest>(json)!;

            Assert.That(request.Type, Is.EqualTo("select"));
            Assert.That(request.DataSource, Is.EqualTo("users"));
            Assert.That(request.Context["selected_option"].GetString(), Is.EqualTo("selected-user-id"));
        }
    }
}
