using Mattermost.Models.Posts;
using System.Text.Json;

namespace Mattermost.Tests
{
    internal class PostPropsAttachmentTests
    {
        [Test]
        public void Serialize_FooterAndThumbnail_UsesMattermostPropertyNames()
        {
            PostPropsAttachment attachment = new PostPropsAttachment
            {
                ThumbUrl = "https://example.com/thumbnail.png",
                Footer = "Deployment completed",
                FooterIcon = "https://example.com/footer.png"
            };

            using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(attachment));
            JsonElement root = document.RootElement;

            Assert.That(root.GetProperty("thumb_url").GetString(), Is.EqualTo(attachment.ThumbUrl));
            Assert.That(root.GetProperty("footer").GetString(), Is.EqualTo(attachment.Footer));
            Assert.That(root.GetProperty("footer_icon").GetString(), Is.EqualTo(attachment.FooterIcon));
        }

        [Test]
        public void Serialize_UnsetFooterAndThumbnail_OmitsNewProperties()
        {
            PostPropsAttachment attachment = new PostPropsAttachment();

            using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(attachment));
            JsonElement root = document.RootElement;

            Assert.That(root.TryGetProperty("thumb_url", out _), Is.False);
            Assert.That(root.TryGetProperty("footer", out _), Is.False);
            Assert.That(root.TryGetProperty("footer_icon", out _), Is.False);
        }
    }
}
