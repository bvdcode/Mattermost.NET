using Mattermost.Constants;
using Mattermost.Helpers;
using Mattermost.Models.Webhooks;
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
        public Task<IList<IncomingWebhook>> GetIncomingWebhooksAsync(string? teamId = null, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (teamId is string id) { query += "&team_id=" + EscapeReadIdentifier(id, nameof(teamId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<IncomingWebhook>>(HttpMethod.Get, Routes.IncomingWebhooks + "?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<IncomingWebhook>());
        }

        /// <inheritdoc />
        public Task<IncomingWebhook> GetIncomingWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(webhookId, nameof(webhookId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IncomingWebhook>(HttpMethod.Get, Routes.IncomingWebhooks + "/" + id, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<IncomingWebhook> CreateIncomingWebhookAsync(IncomingWebhook webhook, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildIncomingWebhookPayload(webhook);
            body["user_id"] = webhook.UserId;
            return SendRequestAsync<IncomingWebhook>(HttpMethod.Post, Routes.IncomingWebhooks, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IncomingWebhook> UpdateIncomingWebhookAsync(IncomingWebhook webhook, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildIncomingWebhookPayload(webhook);
            string id = EscapeReadIdentifier(webhook.Id, nameof(webhook));
            body["id"] = webhook.Id.Trim();
            return SendRequestAsync<IncomingWebhook>(HttpMethod.Put, Routes.IncomingWebhooks + "/" + id, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteIncomingWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(webhookId, nameof(webhookId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.IncomingWebhooks + "/" + id, cancellationToken: cancellationToken);
        }

        private static Dictionary<string, object> BuildIncomingWebhookPayload(IncomingWebhook webhook)
        {
            if (webhook is null) { throw new ArgumentNullException(nameof(webhook)); }
            if (string.IsNullOrWhiteSpace(webhook.ChannelId)) { throw new ArgumentException("A webhook must have a channel identifier.", nameof(webhook)); }
            return new Dictionary<string, object>
            {
                ["channel_id"] = webhook.ChannelId.Trim(), ["display_name"] = webhook.DisplayName,
                ["description"] = webhook.Description, ["username"] = webhook.Username,
                ["icon_url"] = webhook.IconUrl, ["channel_locked"] = webhook.ChannelLocked
            };
        }
    }
}
