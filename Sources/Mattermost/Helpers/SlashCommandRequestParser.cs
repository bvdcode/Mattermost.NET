using Mattermost.Models.SlashCommands;
using System;
using System.Collections.Generic;
using System.Net;

namespace Mattermost.Helpers
{
    /// <summary>
    /// Parses URL-encoded slash command request parameters.
    /// </summary>
    public static class SlashCommandRequestParser
    {
        /// <summary>
        /// Parses a URL-encoded POST body or GET query string into a slash command request.
        /// </summary>
        /// <param name="encodedParameters">The URL-encoded request parameters.</param>
        /// <returns>The parsed slash command request.</returns>
        /// <exception cref="ArgumentNullException">Thrown when encodedParameters is null.</exception>
        /// <exception cref="FormatException">Thrown when a parameter name is empty or repeated.</exception>
        public static SlashCommandRequest Parse(string encodedParameters)
        {
            if (encodedParameters is null)
            {
                throw new ArgumentNullException(nameof(encodedParameters));
            }

            string parameters = encodedParameters;
            if (parameters.StartsWith("?", StringComparison.Ordinal))
            {
                parameters = parameters.Substring(1);
            }

            IReadOnlyDictionary<string, IList<string>> values = ParseValues(parameters);
            return new SlashCommandRequest
            {
                ChannelId = GetValue(values, "channel_id"),
                ChannelName = GetValue(values, "channel_name"),
                ChannelMentions = GetMentions(values, "channel_mentions", "channel_mentions_ids"),
                Command = GetValue(values, "command"),
                ResponseUrl = GetValue(values, "response_url"),
                RootId = GetValue(values, "root_id"),
                TeamDomain = GetValue(values, "team_domain"),
                TeamId = GetValue(values, "team_id"),
                Text = GetValue(values, "text"),
                Token = GetValue(values, "token"),
                TriggerId = GetValue(values, "trigger_id"),
                UserId = GetValue(values, "user_id"),
                UserName = GetValue(values, "user_name"),
                UserMentions = GetMentions(values, "user_mentions", "user_mentions_ids")
            };
        }

        private static IReadOnlyDictionary<string, IList<string>> ParseValues(string encodedParameters)
        {
            Dictionary<string, IList<string>> values =
                new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            string[] pairs = encodedParameters.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                int separatorIndex = pair.IndexOf('=');
                string encodedName;
                string encodedValue;
                if (separatorIndex < 0)
                {
                    encodedName = pair;
                    encodedValue = string.Empty;
                }
                else
                {
                    encodedName = pair.Substring(0, separatorIndex);
                    encodedValue = pair.Substring(separatorIndex + 1);
                }

                string name = WebUtility.UrlDecode(encodedName) ?? string.Empty;
                if (name.Length == 0)
                {
                    throw new FormatException("A slash command parameter name cannot be empty.");
                }

                string value = WebUtility.UrlDecode(encodedValue) ?? string.Empty;
                if (!values.TryGetValue(name, out IList<string>? parameterValues))
                {
                    parameterValues = new List<string>();
                    values.Add(name, parameterValues);
                }

                parameterValues.Add(value);
            }

            return values;
        }

        private static string GetValue(IReadOnlyDictionary<string, IList<string>> values, string name)
        {
            if (!values.TryGetValue(name, out IList<string>? parameterValues))
            {
                return string.Empty;
            }

            if (parameterValues.Count != 1)
            {
                throw new FormatException($"Slash command parameter '{name}' must be specified once.");
            }

            return parameterValues[0];
        }

        private static IReadOnlyDictionary<string, string> GetMentions(
            IReadOnlyDictionary<string, IList<string>> values,
            string mentionsName,
            string identifiersName)
        {
            bool hasMentions = values.TryGetValue(mentionsName, out IList<string>? mentions);
            bool hasIdentifiers = values.TryGetValue(identifiersName, out IList<string>? identifiers);
            if (!hasMentions && !hasIdentifiers)
            {
                return new Dictionary<string, string>();
            }

            if (!hasMentions || !hasIdentifiers || mentions!.Count != identifiers!.Count)
            {
                throw new FormatException(
                    $"Slash command parameters '{mentionsName}' and '{identifiersName}' must contain the same number of values.");
            }

            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < mentions.Count; i++)
            {
                string mention = mentions[i];
                string identifier = identifiers[i];
                if (result.TryGetValue(mention, out string? existingIdentifier) &&
                    !string.Equals(existingIdentifier, identifier, StringComparison.Ordinal))
                {
                    throw new FormatException(
                        $"Slash command mention '{mention}' is associated with multiple identifiers.");
                }

                result[mention] = identifier;
            }

            return result;
        }
    }
}
