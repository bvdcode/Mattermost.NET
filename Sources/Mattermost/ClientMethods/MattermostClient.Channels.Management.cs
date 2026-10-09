using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Channels;
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
        public Task<Channel> UpdateChannelAsync(Channel channel, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            if (channel is null) { throw new ArgumentNullException(nameof(channel)); }
            string id = EscapeReadIdentifier(channel.Id, nameof(channel));
            cancellationToken.ThrowIfCancellationRequested();
            var body = new
            {
                id = channel.Id.Trim(), name = channel.Name, display_name = channel.DisplayName,
                header = channel.Header, purpose = channel.Purpose
            };
            return SendRequestAsync<Channel>(HttpMethod.Put, Routes.Channels + "/" + id, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Channel> PatchChannelAsync(string channelId, string? name = null, string? displayName = null,
            string? header = null, string? purpose = null, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = new Dictionary<string, object>();
            AddPatchValue(body, "name", name);
            AddPatchValue(body, "display_name", displayName);
            AddPatchValue(body, "header", header);
            AddPatchValue(body, "purpose", purpose);
            return SendRequestAsync<Channel>(HttpMethod.Put, Routes.Channels + "/" + id + "/patch", body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Channel> UpdateChannelPrivacyAsync(string channelId, ChannelType privacy,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            string type;
            switch (privacy)
            {
                case ChannelType.Public: type = "O"; break;
                case ChannelType.Private: type = "P"; break;
                case ChannelType.Direct:
                case ChannelType.Group:
                default: throw new ArgumentOutOfRangeException(nameof(privacy), "Only public and private channels can be converted.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Channel>(HttpMethod.Put, Routes.Channels + "/" + id + "/privacy",
                new { privacy = type }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Channel> RestoreChannelAsync(string channelId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(channelId, nameof(channelId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<Channel>(HttpMethod.Post, Routes.Channels + "/" + id + "/restore", cancellationToken: cancellationToken);
        }

        private static void AddPatchValue(IDictionary<string, object> body, string name, object? value)
        {
            if (value is object suppliedValue) { body[name] = suppliedValue; }
        }
    }
}
