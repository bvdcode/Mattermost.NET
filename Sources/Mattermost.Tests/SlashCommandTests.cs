using Mattermost.Helpers;
using Mattermost.Models.Posts;
using Mattermost.Models.SlashCommands;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class SlashCommandTests
    {
        [Test]
        public void Parse_PostBody_MapsDocumentedParameters()
        {
            const string encodedParameters =
                "channel_id=channel-id&" +
                "channel_name=town-square&" +
                "command=%2Fweather&" +
                "response_url=https%3A%2F%2Fmattermost.example%2Fhooks%2Fcommands%2Fcommand-id&" +
                "root_id=root-post-id&" +
                "team_domain=team-awesome&" +
                "team_id=team-id&" +
                "text=toronto+week&" +
                "token=command-token&" +
                "trigger_id=trigger%3Did&" +
                "user_id=user-id&" +
                "user_name=alan&" +
                "user_mentions=alan&" +
                "user_mentions=bob&" +
                "user_mentions_ids=user-id&" +
                "user_mentions_ids=bob-id&" +
                "channel_mentions=town-square&" +
                "channel_mentions=developers&" +
                "channel_mentions_ids=channel-id&" +
                "channel_mentions_ids=developers-channel-id";

            SlashCommandRequest request = SlashCommandRequestParser.Parse(encodedParameters);

            Assert.That(request.ChannelId, Is.EqualTo("channel-id"));
            Assert.That(request.ChannelName, Is.EqualTo("town-square"));
            Assert.That(request.Command, Is.EqualTo("/weather"));
            Assert.That(request.ResponseUrl, Is.EqualTo("https://mattermost.example/hooks/commands/command-id"));
            Assert.That(request.RootId, Is.EqualTo("root-post-id"));
            Assert.That(request.TeamDomain, Is.EqualTo("team-awesome"));
            Assert.That(request.TeamId, Is.EqualTo("team-id"));
            Assert.That(request.Text, Is.EqualTo("toronto week"));
            Assert.That(request.Token, Is.EqualTo("command-token"));
            Assert.That(request.TriggerId, Is.EqualTo("trigger=id"));
            Assert.That(request.UserId, Is.EqualTo("user-id"));
            Assert.That(request.UserName, Is.EqualTo("alan"));
            Assert.That(request.UserMentions["alan"], Is.EqualTo("user-id"));
            Assert.That(request.UserMentions["bob"], Is.EqualTo("bob-id"));
            Assert.That(request.ChannelMentions["town-square"], Is.EqualTo("channel-id"));
            Assert.That(request.ChannelMentions["developers"], Is.EqualTo("developers-channel-id"));
        }

        [Test]
        public void Parse_GetQueryString_DecodesValuesAndIgnoresUnknownParameters()
        {
            const string encodedParameters =
                "?command=%2Fsearch&text=build%2Brelease+notes&user_name=alice&" +
                "future_field=first&future_field=second";

            SlashCommandRequest request = SlashCommandRequestParser.Parse(encodedParameters);

            Assert.That(request.Command, Is.EqualTo("/search"));
            Assert.That(request.Text, Is.EqualTo("build+release notes"));
            Assert.That(request.UserName, Is.EqualTo("alice"));
            Assert.That(request.ChannelId, Is.Empty);
        }

        [Test]
        public void Parse_DuplicateParameter_ThrowsFormatException()
        {
            const string encodedParameters = "user_id=first&user_id=second";

            FormatException? exception = Assert.Throws<FormatException>(
                () => SlashCommandRequestParser.Parse(encodedParameters));

            Assert.That(exception!.Message, Does.Contain("user_id"));
        }

        [Test]
        public void Parse_MismatchedMentionParameters_ThrowsFormatException()
        {
            const string encodedParameters =
                "user_mentions=alice&user_mentions=bob&user_mentions_ids=alice-id";

            FormatException? exception = Assert.Throws<FormatException>(
                () => SlashCommandRequestParser.Parse(encodedParameters));

            Assert.That(exception!.Message, Does.Contain("user_mentions"));
            Assert.That(exception.Message, Does.Contain("user_mentions_ids"));
        }

        [Test]
        public void SlashCommandResponse_SerializesDocumentedFields()
        {
            SlashCommandResponse response = new SlashCommandResponse
            {
                Text = "Weather: sunny",
                ResponseType = SlashCommandResponseType.InChannel,
                Username = "weather-bot",
                ChannelId = "channel-id",
                IconUrl = "https://example.com/weather.png",
                Type = "custom_weather",
                SkipSlackParsing = true,
                GotoLocation = "https://example.com/weather",
                Attachments = new List<PostPropsAttachment>
                {
                    new PostPropsAttachment
                    {
                        Fallback = "Weather details",
                        Text = "Sunny, 21 C"
                    }
                },
                Props = new Dictionary<string, object>
                {
                    ["forecast_days"] = 7,
                    ["cached"] = true
                },
                ExtraResponses = new List<SlashCommandResponseItem>
                {
                    new SlashCommandResponseItem
                    {
                        Text = "Forecast updated",
                        ResponseType = SlashCommandResponseType.Ephemeral
                    }
                }
            };

            using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(response));
            JsonElement root = document.RootElement;
            JsonElement attachment = root.GetProperty("attachments")[0];
            JsonElement props = root.GetProperty("props");
            JsonElement extraResponse = root.GetProperty("extra_responses")[0];

            Assert.That(root.GetProperty("text").GetString(), Is.EqualTo("Weather: sunny"));
            Assert.That(root.GetProperty("response_type").GetString(), Is.EqualTo("in_channel"));
            Assert.That(root.GetProperty("username").GetString(), Is.EqualTo("weather-bot"));
            Assert.That(root.GetProperty("channel_id").GetString(), Is.EqualTo("channel-id"));
            Assert.That(root.GetProperty("icon_url").GetString(), Is.EqualTo("https://example.com/weather.png"));
            Assert.That(root.GetProperty("type").GetString(), Is.EqualTo("custom_weather"));
            Assert.That(root.GetProperty("skip_slack_parsing").GetBoolean(), Is.True);
            Assert.That(root.GetProperty("goto_location").GetString(), Is.EqualTo("https://example.com/weather"));
            Assert.That(attachment.GetProperty("fallback").GetString(), Is.EqualTo("Weather details"));
            Assert.That(props.GetProperty("forecast_days").GetInt32(), Is.EqualTo(7));
            Assert.That(props.GetProperty("cached").GetBoolean(), Is.True);
            Assert.That(extraResponse.GetProperty("text").GetString(), Is.EqualTo("Forecast updated"));
            Assert.That(extraResponse.GetProperty("response_type").GetString(), Is.EqualTo("ephemeral"));
            Assert.That(extraResponse.TryGetProperty("goto_location", out _), Is.False);
            Assert.That(extraResponse.TryGetProperty("extra_responses", out _), Is.False);
        }

        [Test]
        public void SlashCommandResponse_OmitsUnsetOptionalFields()
        {
            SlashCommandResponse response = new SlashCommandResponse
            {
                Text = "Done"
            };

            using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(response));
            JsonElement root = document.RootElement;

            Assert.That(root.GetProperty("text").GetString(), Is.EqualTo("Done"));
            Assert.That(root.TryGetProperty("response_type", out _), Is.False);
            Assert.That(root.TryGetProperty("attachments", out _), Is.False);
            Assert.That(root.TryGetProperty("props", out _), Is.False);
            Assert.That(root.TryGetProperty("goto_location", out _), Is.False);
            Assert.That(root.TryGetProperty("extra_responses", out _), Is.False);
        }
    }
}
