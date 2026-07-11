using Mattermost.Models.Dialogs;
using System.Collections.Generic;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class InteractiveDialogTests
    {
        [Test]
        public void OpenInteractiveDialogRequest_SerializesCurrentDialogFields()
        {
            OpenInteractiveDialogRequest request = new OpenInteractiveDialogRequest
            {
                TriggerId = "trigger-id",
                Url = "https://example.com/dialog/submit",
                Dialog = new InteractiveDialog
                {
                    CallbackId = "create-ticket",
                    Title = "Create Ticket",
                    IntroductionText = "Fill the ticket details.",
                    IconUrl = "https://example.com/icon.png",
                    SubmitLabel = "Create",
                    NotifyOnCancel = true,
                    State = "state-1",
                    SourceUrl = "/plugins/example/dialog/refresh",
                    Elements = new List<InteractiveDialogElement>
                    {
                        new InteractiveDialogElement
                        {
                            DisplayName = "Email",
                            Name = "email",
                            Type = InteractiveDialogElementType.Text,
                            Subtype = InteractiveDialogTextSubtype.Email,
                            Placeholder = "user@example.com",
                            HelpText = "Work email",
                            Optional = true,
                            MinLength = 3,
                            MaxLength = 150
                        },
                        new InteractiveDialogElement
                        {
                            DisplayName = "Assignees",
                            Name = "assignees",
                            Type = InteractiveDialogElementType.Select,
                            DataSource = InteractiveDialogDataSource.Dynamic,
                            DataSourceUrl = "/plugins/example/dialog/lookup",
                            Multiselect = true,
                            Refresh = true,
                            DefaultValue = "user-1,user-2"
                        },
                        new InteractiveDialogElement
                        {
                            DisplayName = "Priority",
                            Name = "priority",
                            Type = InteractiveDialogElementType.Radio,
                            Options = new List<InteractiveDialogOption>
                            {
                                new InteractiveDialogOption("High", "high"),
                                new InteractiveDialogOption("Normal", "normal")
                            },
                            DefaultValue = "normal"
                        },
                        new InteractiveDialogElement
                        {
                            DisplayName = "Due",
                            Name = "due",
                            Type = InteractiveDialogElementType.DateTime,
                            DateTimeConfig = new InteractiveDialogDateTimeConfig
                            {
                                MinDate = "today",
                                MaxDate = "+30d",
                                TimeInterval = 30,
                                LocationTimezone = "America/Denver",
                                ManualTimeEntry = true
                            }
                        },
                        new InteractiveDialogElement
                        {
                            DisplayName = "Attachment",
                            Name = "attachment",
                            Type = InteractiveDialogElementType.File,
                            AllowMultiple = true,
                            Optional = true
                        },
                        new InteractiveDialogElement
                        {
                            DisplayName = "Open Child",
                            Name = "open_child",
                            Type = InteractiveDialogElementType.ActionButton,
                            ActionButton = new InteractiveDialogActionButton
                            {
                                Url = "/plugins/example/dialog/action",
                                Context = new Dictionary<string, object>
                                {
                                    ["action"] = "open_child"
                                }
                            }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(request);
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            JsonElement dialog = root.GetProperty("dialog");
            JsonElement elements = dialog.GetProperty("elements");

            Assert.That(root.GetProperty("trigger_id").GetString(), Is.EqualTo("trigger-id"));
            Assert.That(root.GetProperty("url").GetString(), Is.EqualTo("https://example.com/dialog/submit"));
            Assert.That(dialog.GetProperty("callback_id").GetString(), Is.EqualTo("create-ticket"));
            Assert.That(dialog.GetProperty("source_url").GetString(), Is.EqualTo("/plugins/example/dialog/refresh"));
            Assert.That(elements[0].GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(elements[0].GetProperty("subtype").GetString(), Is.EqualTo("email"));
            Assert.That(elements[1].GetProperty("data_source").GetString(), Is.EqualTo("dynamic"));
            Assert.That(elements[1].GetProperty("data_source_url").GetString(), Is.EqualTo("/plugins/example/dialog/lookup"));
            Assert.That(elements[1].GetProperty("multiselect").GetBoolean(), Is.True);
            Assert.That(elements[1].GetProperty("refresh").GetBoolean(), Is.True);
            Assert.That(elements[2].GetProperty("type").GetString(), Is.EqualTo("radio"));
            Assert.That(elements[2].GetProperty("options")[0].GetProperty("text").GetString(), Is.EqualTo("High"));
            Assert.That(elements[3].GetProperty("type").GetString(), Is.EqualTo("datetime"));
            Assert.That(elements[3].GetProperty("datetime_config").GetProperty("time_interval").GetInt32(), Is.EqualTo(30));
            Assert.That(elements[3].GetProperty("datetime_config").GetProperty("manual_time_entry").GetBoolean(), Is.True);
            Assert.That(elements[4].GetProperty("type").GetString(), Is.EqualTo("file"));
            Assert.That(elements[4].GetProperty("allow_multiple").GetBoolean(), Is.True);
            Assert.That(elements[5].GetProperty("type").GetString(), Is.EqualTo("action_button"));
            Assert.That(elements[5].GetProperty("action_button").GetProperty("context").GetProperty("action").GetString(), Is.EqualTo("open_child"));
        }

        [Test]
        public void InteractiveDialogSubmissionRequest_DeserializesSubmissionPayload()
        {
            const string json = "{" +
                "\"type\":\"dialog_submission\"," +
                "\"callback_id\":\"create-ticket\"," +
                "\"state\":\"state-1\"," +
                "\"user_id\":\"user-id\"," +
                "\"channel_id\":\"channel-id\"," +
                "\"team_id\":\"team-id\"," +
                "\"submission\":{\"title\":\"Bug\",\"estimate\":3,\"urgent\":true}," +
                "\"file_ids\":[\"file-1\",\"file-2\"]," +
                "\"cancelled\":false" +
                "}";

            InteractiveDialogSubmissionRequest request = JsonSerializer.Deserialize<InteractiveDialogSubmissionRequest>(json)!;

            Assert.That(request.Type, Is.EqualTo("dialog_submission"));
            Assert.That(request.CallbackId, Is.EqualTo("create-ticket"));
            Assert.That(request.State, Is.EqualTo("state-1"));
            Assert.That(request.UserId, Is.EqualTo("user-id"));
            Assert.That(request.ChannelId, Is.EqualTo("channel-id"));
            Assert.That(request.TeamId, Is.EqualTo("team-id"));
            Assert.That(request.Submission["title"].GetString(), Is.EqualTo("Bug"));
            Assert.That(request.Submission["estimate"].GetInt32(), Is.EqualTo(3));
            Assert.That(request.Submission["urgent"].GetBoolean(), Is.True);
            Assert.That(request.FileIds, Is.EqualTo(new[] { "file-1", "file-2" }));
            Assert.That(request.Cancelled, Is.False);
        }

        [Test]
        public void InteractiveDialogResponse_SerializesValidationAndFormResponses()
        {
            InteractiveDialogResponse validationResponse = new InteractiveDialogResponse
            {
                Error = "Fix the form.",
                Errors = new Dictionary<string, string>
                {
                    ["title"] = "Title is required."
                }
            };
            InteractiveDialogLookupResponse lookupResponse = new InteractiveDialogLookupResponse
            {
                Items = new List<InteractiveDialogOption>
                {
                    new InteractiveDialogOption("Option 1", "option-1")
                }
            };
            InteractiveDialogResponse formResponse = new InteractiveDialogResponse
            {
                Type = "form",
                Form = new InteractiveDialog
                {
                    CallbackId = "step-2",
                    Title = "Step 2",
                    Elements = new List<InteractiveDialogElement>
                    {
                        new InteractiveDialogElement
                        {
                            DisplayName = "Summary",
                            Name = "summary",
                            Type = InteractiveDialogElementType.Text
                        }
                    }
                }
            };

            using JsonDocument validationDocument = JsonDocument.Parse(JsonSerializer.Serialize(validationResponse));
            using JsonDocument lookupDocument = JsonDocument.Parse(JsonSerializer.Serialize(lookupResponse));
            using JsonDocument formDocument = JsonDocument.Parse(JsonSerializer.Serialize(formResponse));

            Assert.That(validationDocument.RootElement.GetProperty("error").GetString(), Is.EqualTo("Fix the form."));
            Assert.That(validationDocument.RootElement.GetProperty("errors").GetProperty("title").GetString(), Is.EqualTo("Title is required."));
            Assert.That(lookupDocument.RootElement.GetProperty("items")[0].GetProperty("value").GetString(), Is.EqualTo("option-1"));
            Assert.That(formDocument.RootElement.GetProperty("type").GetString(), Is.EqualTo("form"));
            Assert.That(formDocument.RootElement.GetProperty("form").GetProperty("title").GetString(), Is.EqualTo("Step 2"));
        }
    }
}
