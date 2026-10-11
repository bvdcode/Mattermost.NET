using Mattermost.Constants;
using Mattermost.Models.SlashCommands;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <inheritdoc />
        public Task<SlashCommandResponse> ExecuteCommandAsync(string channelId, string command, string? teamId = null,
            string? rootId = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            ValidateCommandIdentifier(channelId, nameof(channelId));
            if (string.IsNullOrWhiteSpace(command) || command.Length <= 1 || !command.StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("A slash-prefixed command is required.", nameof(command));
            }
            if (teamId is object) { ValidateCommandIdentifier(teamId, nameof(teamId)); }
            if (rootId is object) { ValidateCommandIdentifier(rootId, nameof(rootId)); }
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["channel_id"] = channelId.Trim(), ["command"] = command
            };
            if (teamId is object) { body.Add("team_id", teamId.Trim()); }
            if (rootId is object) { body.Add("root_id", rootId.Trim()); }
            return SendRequestAsync<SlashCommandResponse>(HttpMethod.Post, Routes.Commands + "/execute", body, cancellationToken);
        }
    }
}
