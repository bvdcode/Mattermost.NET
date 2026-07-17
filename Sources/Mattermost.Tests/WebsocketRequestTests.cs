using System;
using System.Reflection;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class WebsocketRequestTests
    {
        [Test]
        public void Serialize_ActionRequest_UsesMattermostPropertyNames()
        {
            Type requestType = typeof(MattermostClient).Assembly.GetType(
                "Mattermost.Models.Requests.Websocket.ActionRequest",
                true)!;
            object request = Activator.CreateInstance(requestType, true)!;
            requestType.GetProperty("Seq")!.SetValue(request, 7);
            requestType.GetProperty("Action")!.SetValue(request, "user_typing");
            requestType.GetProperty("Data")!.SetValue(request, new object());

            using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(request, requestType));
            JsonElement root = document.RootElement;

            Assert.That(root.GetProperty("seq").GetInt32(), Is.EqualTo(7));
            Assert.That(root.GetProperty("action").GetString(), Is.EqualTo("user_typing"));
            Assert.That(root.GetProperty("data").ValueKind, Is.EqualTo(JsonValueKind.Object));
            Assert.That(root.TryGetProperty("Seq", out _), Is.False);
            Assert.That(root.TryGetProperty("Action", out _), Is.False);
            Assert.That(root.TryGetProperty("Data", out _), Is.False);
        }
    }
}
