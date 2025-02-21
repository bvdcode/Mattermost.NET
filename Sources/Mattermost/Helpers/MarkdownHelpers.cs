using System;
using System.Linq;
using Mattermost.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace Mattermost.Helpers
{
    /// <summary>
    /// This class provides helper methods for formatting text in Markdown.
    /// </summary>
    public class MarkdownHelpers
    {
        private const string reservedChars = "\\`*_{}[]()#+-.!";

        /// <summary>
        /// Makes the given text bold.
        /// </summary>
        /// <param name="text">The text to be made bold.</param>
        /// <returns>The formatted text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string Bold(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"**{text}**";
        }

        /// <summary>
        /// Makes the given text italic.
        /// </summary>
        /// <param name="text">The text to be made italic.</param>
        /// <returns>The formatted text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string Italic(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"_{text}_";
        }

        /// <summary>
        /// Makes the given text bold and italic.
        /// </summary>
        /// <param name="text">The text to be made bold and italic.</param>
        /// <returns>The formatted text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string BoldItalic(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"***{text}***";
        }

        /// <summary>
        /// Strikes through the given text.
        /// </summary>
        /// <param name="text">The text to be struck through.</param>
        /// <returns>The formatted text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string StrikeThrough(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"~~{text}~~";
        }

        /// <summary>
        /// Formats the given text as inline code.
        /// </summary>
        /// <param name="text">The text to be formatted as inline code.</param>
        /// <returns>The formatted text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string InlineCode(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"`{text}`";
        }

        /// <summary>
        /// Formats the given text as a code block with optional language syntax highlighting.
        /// </summary>
        /// <param name="text">The text to be formatted as a code block.</param>
        /// <param name="language">The programming language for syntax highlighting. Default is plain text.</param>
        /// <returns>The formatted code block.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string CodeBlock(string text, CodeLanguage language = CodeLanguage.Text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"```{language}\n{text}\n```";
        }

        /// <summary>
        /// Quotes the given text.
        /// </summary>
        /// <param name="text">The text to be quoted.</param>
        /// <returns>The formatted quote.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string Quote(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            string result = string.Empty;
            foreach (string line in text.Split('\n'))
            {
                result += $"> {line}\n";
            }
            return result;
        }

        /// <summary>
        /// Creates a link with the given text and URL.
        /// </summary>
        /// <param name="text">The text to be displayed as a link.</param>
        /// <param name="url">The URL to link to.</param>
        /// <returns>The formatted link.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string Link(string text, string url)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            return $"[{Escape(text)}]({url})";
        }

        /// <summary>
        /// Creates an image with the given alt text and URL.
        /// </summary>
        /// <param name="altText">The alt text for the image.</param>
        /// <param name="url">The URL of the image.</param>
        /// <returns>The formatted image.</returns>
        /// <exception cref="ArgumentException">Thrown when the alt text is null or empty.</exception>
        public static string Image(string altText, string url)
        {
            ExceptionHelpers.ThrowIfEmpty(altText, nameof(altText));
            return $"![{Escape(altText)}]({url})";
        }

        /// <summary>
        /// Creates a mention for a user with the given username.
        /// </summary>
        /// <param name="username">The username of the user to mention.</param>
        /// <returns>The formatted mention.</returns>
        /// <exception cref="ArgumentException">Thrown when the username is null or empty.</exception>
        public static string Mention(string username)
        {
            ExceptionHelpers.ThrowIfEmpty(username, nameof(username));
            return $"@{username}";
        }

        /// <summary>
        /// Creates a mention for a channel with the given channel name.
        /// </summary>
        /// <param name="channelName">The name of the channel to mention.</param>
        /// <returns>The formatted channel mention.</returns>
        /// <exception cref="ArgumentException">Thrown when the channel name is null or empty.</exception>
        public static string ChannelMention(string channelName)
        {
            ExceptionHelpers.ThrowIfEmpty(channelName, nameof(channelName));
            return $"~{channelName}";
        }

        /// <summary>
        /// Formats the given text as a header.
        /// </summary>
        /// <param name="text">The text to be formatted as a header.</param>
        /// <param name="level">The level of the header (1-6). Default is 1.</param>
        /// <returns>The formatted header text.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty, or when the level is not between 1 and 6.</exception>
        public static string Header(string text, int level = 1)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            if (level < 1 || level > 6)
            {
                throw new ArgumentException("Header level must be between 1 and 6.", nameof(level));
            }
            return new string('#', level) + $" {text}";
        }

        /// <summary>
        /// Formats the given items as an ordered list.
        /// </summary>
        /// <param name="items">The items to be formatted as an ordered list.</param>
        /// <returns>The formatted ordered list.</returns>
        /// <exception cref="ArgumentException">Thrown when any item is null or empty.</exception>
        public static string OrderedList(params string[] items)
        {
            string result = string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                ExceptionHelpers.ThrowIfEmpty(items[i], $"{nameof(items)}[{i}]");
                result += $"{i + 1}. {items[i]}\n";
            }
            return result;
        }

        /// <summary>
        /// Formats the given items as an unordered list.
        /// </summary>
        /// <param name="items">The items to be formatted as an unordered list.</param>
        /// <returns>The formatted unordered list.</returns>
        /// <exception cref="ArgumentException">Thrown when any item is null or empty.</exception>
        public static string UnorderedList(params string[] items)
        {
            return UnorderedList(items.Select(x => (x, 0)).ToArray());
        }

        /// <summary>
        /// Formats the given items as an unordered list with specified indentation levels.
        /// </summary>
        /// <param name="values">The items and their indentation levels to be formatted as an unordered list.</param>
        /// <returns>The formatted unordered list.</returns>
        /// <exception cref="ArgumentException">Thrown when any item is null or empty, or when any indentation level is less than 0.</exception>
        public static string UnorderedList(params (string, int)[] values)
        {
            string result = string.Empty;
            foreach (var item in values)
            {
                if (item.Item2 < 0)
                {
                    throw new ArgumentException("Indentation must be greater or equal to 0.", nameof(item.Item2));
                }
                ExceptionHelpers.ThrowIfEmpty(item.Item1, nameof(item.Item1));
                result += $"{new string(' ', item.Item2 * 2)}- {item.Item1}\n";
            }
            return result;
        }

        /// <summary>
        /// Formats the given items as a task list.
        /// </summary>
        /// <param name="items">The items to be formatted as a task list.</param>
        /// <returns>The formatted task list.</returns>
        /// <exception cref="ArgumentException">Thrown when any item is null or empty.</exception>
        public static string TaskList(params string[] items)
        {
            return TaskList(items.Select(x => (x, false)).ToArray());
        }

        /// <summary>
        /// Formats the given items as a task list with specified completion status.
        /// </summary>
        /// <param name="items">The items and their completion status to be formatted as a task list.</param>
        /// <returns>The formatted task list.</returns>
        /// <exception cref="ArgumentException">Thrown when any item is null or empty.</exception>
        public static string TaskList(params (string, bool)[] items)
        {
            string result = string.Empty;
            foreach (var item in items)
            {
                ExceptionHelpers.ThrowIfEmpty(item.Item1, nameof(item.Item1));
                result += $"- [{(item.Item2 ? "x" : " ")}] {item.Item1}\n";
            }
            return result;
        }

        /// <summary>
        /// Escapes reserved Markdown characters in the given text.
        /// </summary>
        /// <param name="text">The text in which to escape reserved characters.</param>
        /// <returns>The text with escaped characters.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        public static string Escape(string text)
        {
            ExceptionHelpers.ThrowIfEmpty(text, nameof(text));
            foreach (char c in reservedChars)
            {
                text = text.Replace(c.ToString(), $"\\{c}");
            }
            return text;
        }
    }
}
