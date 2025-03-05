using Mattermost.Helpers;

namespace Mattermost.Tests
{
    internal class HelperTests
    {
        [Test]
        public void QueryHelpers_BuildQuery_ValidResult()
        {
            const string expected = "page=1&per_page=10&include_deleted=True&since=1690182000&before=postId1&after=postId2";
            var actual = QueryHelpers.BuildChannelPostsQuery(1, 10, beforePostId: "postId1", afterPostId: "postId2", includeDeleted: true, since: new DateTime(2023, 7, 24, 0, 0, 0));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExceptionHelpers_ThrowIfEmpty_ValidResult()
        {
            Assert.Throws<ArgumentException>(() => ExceptionHelpers.ThrowIfEmpty("", "paramName"));
            Assert.Throws<ArgumentException>(() => ExceptionHelpers.ThrowIfEmpty("text", ""));
            Assert.DoesNotThrow(() => ExceptionHelpers.ThrowIfEmpty("text", "paramName"));
        }

        [Test]
        public void MarkdownHelpers_ConvertToMarkdown_ValidResult()
        {
            const string expected = "This is a **bold** text and this is an _italic_ text.";
            var actual = $"This is a {MarkdownHelpers.Bold("bold")} text and this is an {MarkdownHelpers.Italic("italic")} text.";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
