using System;

namespace Mattermost.Helpers
{
    internal class MarkdownHelpers
    {
        internal static string Bold(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"**{text}**";
        }

        internal static string Italic(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"*{text}*";
        }

        internal static string StrikeThrough(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"~~{text}~~";
        }

        internal static string Code(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"`{text}`";
        }

        internal static string CodeBlock(string text, string language = "")
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"```{language}\n{text}\n```";
        }

        internal static string Quote(string text)
        {
            string result = string.Empty;
            foreach (string line in text.Split('\n'))
            {
                result += $"> {line}\n";
            }
            return result;
        }

        internal static string Link(string text, string url)
        {
            return $"[{text}]({url})";
        }

        internal static string Mention(string username)
        {
            return $"@{username}";
        }

        internal static string ChannelMention(string channelName)
        {
            return $"~{channelName}";
        }

        internal static string Escape(string text)
        {
            string chars = "\\`*_{}[]()#+-.!";
            foreach (char c in chars)
            {
                text = text.Replace(c.ToString(), $"\\{c}");
            }
            return text;
        }
    }
}
