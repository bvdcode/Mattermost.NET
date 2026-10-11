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
        public Task<IList<OutgoingWebhook>> GetOutgoingWebhooksAsync(string? teamId = null, string? channelId = null,
            int page = 0, int perPage = 60, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string query = QueryHelpers.BuildPagedQuery(page, perPage);
            if (teamId is string team) { query += "&team_id=" + EscapeReadIdentifier(team, nameof(teamId)); }
            if (channelId is string channel) { query += "&channel_id=" + EscapeReadIdentifier(channel, nameof(channelId)); }
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<IList<OutgoingWebhook>>(HttpMethod.Get, Routes.OutgoingWebhooks + "?" + query,
                cancellationToken: cancellationToken, nullResultFactory: () => new List<OutgoingWebhook>());
        }

        /// <inheritdoc />
        public Task<OutgoingWebhook> GetOutgoingWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(webhookId, nameof(webhookId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OutgoingWebhook>(HttpMethod.Get, Routes.OutgoingWebhooks + "/" + id, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<OutgoingWebhook> CreateOutgoingWebhookAsync(OutgoingWebhook webhook, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildOutgoingWebhookPayload(webhook);
            body["creator_id"] = webhook.CreatorId;
            return SendRequestAsync<OutgoingWebhook>(HttpMethod.Post, Routes.OutgoingWebhooks, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OutgoingWebhook> UpdateOutgoingWebhookAsync(OutgoingWebhook webhook, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, object> body = BuildOutgoingWebhookPayload(webhook);
            string id = EscapeReadIdentifier(webhook.Id, nameof(webhook));
            if (string.IsNullOrWhiteSpace(webhook.Token)) { throw new ArgumentException("Read the webhook first to preserve its token.", nameof(webhook)); }
            body["id"] = webhook.Id.Trim();
            body["token"] = webhook.Token;
            return SendRequestAsync<OutgoingWebhook>(HttpMethod.Put, Routes.OutgoingWebhooks + "/" + id, body, cancellationToken);
        }

        /// <inheritdoc />
        public Task<OutgoingWebhook> RegenerateOutgoingWebhookTokenAsync(string webhookId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(webhookId, nameof(webhookId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync<OutgoingWebhook>(HttpMethod.Post, Routes.OutgoingWebhooks + "/" + id + "/regen_token",
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteOutgoingWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            string id = EscapeReadIdentifier(webhookId, nameof(webhookId));
            cancellationToken.ThrowIfCancellationRequested();
            return SendRequestAsync(HttpMethod.Delete, Routes.OutgoingWebhooks + "/" + id, cancellationToken: cancellationToken);
        }

        private static Dictionary<string, object> BuildOutgoingWebhookPayload(OutgoingWebhook webhook)
        {
            if (webhook is null) { throw new ArgumentNullException(nameof(webhook)); }
            if (string.IsNullOrWhiteSpace(webhook.TeamId)) { throw new ArgumentException("A webhook must have a team identifier.", nameof(webhook)); }
            List<string> triggers = new List<string>();
            if (webhook.TriggerWords is IList<string> words)
            {
                foreach (string word in words)
                {
                    if (string.IsNullOrWhiteSpace(word)) { throw new ArgumentException("Trigger words cannot be blank.", nameof(webhook)); }
                    triggers.Add(word);
                }
            }
            string channelId = webhook.ChannelId?.Trim() ?? string.Empty;
            if (channelId.Length == 0 && triggers.Count == 0) { throw new ArgumentException("Team-wide webhooks require trigger words.", nameof(webhook)); }
            if (webhook.CallbackUrls is null || webhook.CallbackUrls.Count == 0) { throw new ArgumentException("A webhook must have callback URLs.", nameof(webhook)); }
            List<string> callbacks = new List<string>();
            foreach (string callback in webhook.CallbackUrls)
            {
                if (!Uri.TryCreate(callback?.Trim(), UriKind.Absolute, out Uri? uri)
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ArgumentException("Callback URLs must be absolute HTTP or HTTPS URLs.", nameof(webhook));
                }
                callbacks.Add(callback!.Trim());
            }
            int triggerWhen;
            switch (webhook.TriggerWhen)
            {
                case OutgoingWebhookTriggerWhen.FirstWordExactMatch: triggerWhen = 0; break;
                case OutgoingWebhookTriggerWhen.FirstWordStartsWith: triggerWhen = 1; break;
                default: throw new ArgumentOutOfRangeException(nameof(webhook));
            }
            return new Dictionary<string, object>
            {
                ["team_id"] = webhook.TeamId.Trim(), ["channel_id"] = channelId, ["trigger_words"] = triggers,
                ["trigger_when"] = triggerWhen, ["callback_urls"] = callbacks, ["display_name"] = webhook.DisplayName,
                ["description"] = webhook.Description, ["content_type"] = webhook.ContentType,
                ["username"] = webhook.Username, ["icon_url"] = webhook.IconUrl
            };
        }
    }
}
