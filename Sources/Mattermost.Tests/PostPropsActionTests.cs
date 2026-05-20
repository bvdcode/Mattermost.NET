using Mattermost.Models.Posts;
using System.Collections.Generic;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class PostPropsActionTests
    {
        [Test]
        public void SelectAction_SerializesInteractiveMenuFields()
        {
            PostProps props = new();
            props.Attachments.Add(new PostPropsAttachment
            {
                Text = "Attachment text",
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
                                ["action"] = "do_something"
                            }
                        },
                        Options = new List<PostActionOption>
                        {
                            new PostActionOption("Option1", "opt1"),
                            new PostActionOption("Option2", "opt2")
                        }
                    }
                }
            });

            string json = JsonSerializer.Serialize(props);
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement action = document.RootElement
                .GetProperty("attachments")[0]
                .GetProperty("actions")[0];

            Assert.That(action.GetProperty("id").GetString(), Is.EqualTo("actionoptions"));
            Assert.That(action.GetProperty("type").GetString(), Is.EqualTo("select"));
            Assert.That(action.GetProperty("name").GetString(), Is.EqualTo("Select an option..."));
            Assert.That(action.GetProperty("default_option").GetString(), Is.EqualTo("opt2"));
            Assert.That(action.GetProperty("options").GetArrayLength(), Is.EqualTo(2));
            Assert.That(action.GetProperty("options")[0].GetProperty("text").GetString(), Is.EqualTo("Option1"));
            Assert.That(action.GetProperty("options")[0].GetProperty("value").GetString(), Is.EqualTo("opt1"));
            Assert.That(action.GetProperty("integration").GetProperty("context").GetProperty("action").GetString(), Is.EqualTo("do_something"));
            Assert.That(action.TryGetProperty("style", out _), Is.False);
        }

        [Test]
        public void DataSourceSelectAction_SerializesWithoutOptions()
        {
            PostProps props = new();
            props.Attachments.Add(new PostPropsAttachment
            {
                Text = "Attachment text",
                Actions =
                {
                    new PostPropsSelectAction
                    {
                        Id = "actionusers",
                        Name = "Select a user...",
                        DataSource = PostActionDataSource.Users,
                        Integration = new Integration
                        {
                            Url = "https://example.com/actionusers"
                        }
                    }
                }
            });

            string json = JsonSerializer.Serialize(props);
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement action = document.RootElement
                .GetProperty("attachments")[0]
                .GetProperty("actions")[0];

            Assert.That(action.GetProperty("type").GetString(), Is.EqualTo("select"));
            Assert.That(action.GetProperty("data_source").GetString(), Is.EqualTo("users"));
            Assert.That(action.TryGetProperty("options", out _), Is.False);
        }

        [Test]
        public void ButtonAction_SerializesButtonTypeAndStyle()
        {
            var action = new PostPropsButtonAction
            {
                Id = "approve",
                Name = "Approve",
                Style = ActionStyle.Primary,
                Integration = new Integration
                {
                    Url = "https://example.com/approve"
                }
            };

            string json = JsonSerializer.Serialize(action);
            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty("type").GetString(), Is.EqualTo("button"));
            Assert.That(document.RootElement.GetProperty("style").GetString(), Is.EqualTo("primary"));
        }

        [Test]
        public void PostRawProps_DeserializesInteractiveMenuFields()
        {
            const string json = "{" +
                "\"id\":\"postid\"," +
                "\"props\":{" +
                "\"attachments\":[{" +
                "\"text\":\"Attachment text\"," +
                "\"actions\":[{" +
                "\"id\":\"actionchannels\"," +
                "\"type\":\"select\"," +
                "\"name\":\"Select a channel...\"," +
                "\"data_source\":\"channels\"," +
                "\"default_option\":\"town-square\"," +
                "\"style\":\"danger\"," +
                "\"options\":[{\"text\":\"Town Square\",\"value\":\"town-square\"}]," +
                "\"integration\":{\"url\":\"https://example.com/actionchannels\",\"context\":{\"action\":\"pick_channel\"}}" +
                "}]" +
                "}]" +
                "}" +
                "}";

            Post post = JsonSerializer.Deserialize<Post>(json)!;
            PostPropsAction action = post.Props.Attachments[0].Actions[0];

            Assert.That(action.Id, Is.EqualTo("actionchannels"));
            Assert.That(action.Type, Is.EqualTo(PostActionType.Select));
            Assert.That(action.DataSource, Is.EqualTo(PostActionDataSource.Channels));
            Assert.That(action.DefaultOption, Is.EqualTo("town-square"));
            Assert.That(action.Style, Is.EqualTo(ActionStyle.Danger));
            Assert.That(action.Options, Has.Count.EqualTo(1));
            Assert.That(action.Options![0].Text, Is.EqualTo("Town Square"));
            Assert.That(action.Options[0].Value, Is.EqualTo("town-square"));
            Assert.That(action.Integration.Context["action"].ToString(), Is.EqualTo("pick_channel"));
        }
    }
}
