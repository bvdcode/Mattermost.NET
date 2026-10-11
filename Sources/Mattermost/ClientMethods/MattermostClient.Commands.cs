using Mattermost.Constants;
using Mattermost.Models.SlashCommands;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<IList<SlashCommand>> GetCommandsAsync(string teamId, bool customOnly = false,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateCommandIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            string query = "team_id=" + Uri.EscapeDataString(teamId.Trim()) + "&custom_only=" + customOnly.ToString().ToLowerInvariant();
            return SendRequestAsync<IList<SlashCommand>>(HttpMethod.Get, Routes.Commands + "?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<SlashCommand>());
        }

        /// <inheritdoc />
        public Task<SlashCommand> GetCommandAsync(string commandId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = BuildCommandUrl(commandId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<SlashCommand>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<SlashCommand> CreateCommandAsync(SlashCommand command, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildCommandPayload(command);
            if (!string.IsNullOrEmpty(command.CreatorId))
            {
                ValidateCommandIdentifier(command.CreatorId, nameof(command));
                body.Add("creator_id", command.CreatorId.Trim());
            }
            return SendRequestAsync<SlashCommand>(HttpMethod.Post, Routes.Commands, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<SlashCommand> UpdateCommandAsync(SlashCommand command, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildCommandPayload(command);
            string url = BuildCommandUrl(command.Id);
            body.Add("id", command.Id.Trim());
            return SendRequestAsync<SlashCommand>(HttpMethod.Put, url, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteCommandAsync(string commandId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = BuildCommandUrl(commandId);
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task MoveCommandAsync(string commandId, string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = BuildCommandUrl(commandId);
            ValidateCommandIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            var body = new { team_id = teamId.Trim() };
            return SendRequestAsync(HttpMethod.Put, url + "/move", body, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> RegenerateCommandTokenAsync(string commandId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string url = BuildCommandUrl(commandId);
            cancellationToken.ThrowIfCancellationRequested();
            IDictionary<string, string> result = await SendRequestAsync<IDictionary<string, string>>(HttpMethod.Put,
                url + "/regen_token", cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!result.TryGetValue("token", out string? token) || string.IsNullOrWhiteSpace(token))
            {
                throw new JsonException("The regenerated command token is missing or invalid.");
            }
            return token;
        }

        /// <inheritdoc />
        public Task<IList<SlashCommand>> GetAutocompleteCommandsAsync(string teamId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateCommandIdentifier(teamId, nameof(teamId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<SlashCommand>>(HttpMethod.Get,
                Routes.Teams + "/" + Uri.EscapeDataString(teamId.Trim()) + "/commands/autocomplete",
                cancellationToken: cancellationToken, nullResultFactory: () => new List<SlashCommand>());
        }

        /// <inheritdoc />
        public Task<IList<CommandAutocompleteSuggestion>> GetCommandAutocompleteSuggestionsAsync(string teamId, string userInput,
            string? channelId = null, string? rootId = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateCommandIdentifier(teamId, nameof(teamId));
            if (string.IsNullOrWhiteSpace(userInput)) { throw new ArgumentException("Command input cannot be null or empty.", nameof(userInput)); }
            if (channelId is object) { ValidateCommandIdentifier(channelId, nameof(channelId)); }
            if (rootId is object) { ValidateCommandIdentifier(rootId, nameof(rootId)); }
            cancellationToken.ThrowIfCancellationRequested();
            string query = "user_input=" + Uri.EscapeDataString(userInput);
            if (channelId is object) { query += "&channel_id=" + Uri.EscapeDataString(channelId.Trim()); }
            if (rootId is object) { query += "&root_id=" + Uri.EscapeDataString(rootId.Trim()); }
            return SendRequestAsync<IList<CommandAutocompleteSuggestion>>(HttpMethod.Get,
                Routes.Teams + "/" + Uri.EscapeDataString(teamId.Trim()) + "/commands/autocomplete_suggestions?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<CommandAutocompleteSuggestion>());
        }

        private static string BuildCommandUrl(string commandId)
        {
            ValidateCommandIdentifier(commandId, nameof(commandId));
            return Routes.Commands + "/" + Uri.EscapeDataString(commandId.Trim());
        }

        private static void ValidateCommandIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentException("The identifier cannot be null or empty.", parameterName); }
        }

        private static Dictionary<string, object> BuildCommandPayload(SlashCommand command)
        {
            if (command is null) { throw new ArgumentNullException(nameof(command)); }
            ValidateCommandIdentifier(command.TeamId, nameof(command));
            if (string.IsNullOrWhiteSpace(command.Trigger) || command.Trigger.Trim().StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("A command trigger without a leading slash is required.", nameof(command));
            }
            if (!Uri.TryCreate(command.Url?.Trim(), UriKind.Absolute, out Uri? url)
                || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("The command URL must be an absolute HTTP or HTTPS URL.", nameof(command));
            }
            switch (command.Method)
            {
                case "P": case "G": break;
                default: throw new ArgumentException("The command method must be P (POST) or G (GET).", nameof(command));
            }
            return new Dictionary<string, object>
            {
                ["team_id"] = command.TeamId.Trim(), ["trigger"] = command.Trigger.Trim(), ["method"] = command.Method,
                ["url"] = command.Url!.Trim(), ["username"] = command.Username, ["icon_url"] = command.IconUrl,
                ["auto_complete"] = command.AutoComplete, ["auto_complete_desc"] = command.AutoCompleteDescription,
                ["auto_complete_hint"] = command.AutoCompleteHint, ["display_name"] = command.DisplayName,
                ["description"] = command.Description
            };
        }
    }
}
