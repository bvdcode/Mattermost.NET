using System;
using System.Linq;
using Mattermost.Models.Enums;

namespace Mattermost.Helpers
{
    internal class MarkdownHelpers
    {
        private const string reservedChars = "\\`*_{}[]()#+-.!";

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
            return $"_{text}_";
        }

        internal static string BoldItalic(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"***{text}***";
        }

        internal static string StrikeThrough(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"~~{text}~~";
        }

        internal static string InlineCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            return $"`{text}`";
        }

        internal static string CodeBlock(string text, CodeLanguage language = CodeLanguage.Text)
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
            return $"[{Escape(text)}]({url})";
        }

        internal static string Image(string altText, string url)
        {
            return $"![{Escape(altText)}]({url})";
        }

        internal static string Mention(string username)
        {
            return $"@{username}";
        }

        internal static string ChannelMention(string channelName)
        {
            return $"~{channelName}";
        }

        internal static string Header(string text, int level = 1)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }
            if (level < 1 || level > 6)
            {
                throw new ArgumentException("Header level must be between 1 and 6.", nameof(level));
            }
            return new string('#', level) + $" {text}";
        }

        internal static string OrderedList(params string[] items)
        {
            string result = string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                result += $"{i + 1}. {items[i]}\n";
            }
            return result;
        }

        internal static string UnorderedList(params string[] items)
        {
            return UnorderedList(items.Select(x => (x, 0)).ToArray());
        }

        internal static string UnorderedList(params (string, int)[] values)
        {
            string result = string.Empty;
            foreach (var item in values)
            {
                if (item.Item2 < 0)
                {
                    throw new ArgumentException("Indentation must be greater or equal to 0.", nameof(item.Item2));
                }
                result += $"{new string(' ', item.Item2 * 2)}- {item.Item1}\n";
            }
            return result;
        }

        internal static string TaskList(params string[] items)
        {
            return TaskList(items.Select(x => (x, false)).ToArray());
        }

        internal static string TaskList(params (string, bool)[] items)
        {
            string result = string.Empty;
            foreach (var item in items)
            {
                result += $"- [{(item.Item2 ? "x" : " ")}] {item.Item1}\n";
            }
            return result;
        }

        internal static string Escape(string text)
        {
            foreach (char c in reservedChars)
            {
                text = text.Replace(c.ToString(), $"\\{c}");
            }
            return text;
        }
    }
}
