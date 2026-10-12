using Mattermost.Models.Dialogs;
using Mattermost.Models.OAuth;
using Mattermost.Models.SlashCommands;
using Mattermost.Models.Webhooks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Slash commands, webhooks and interactive dialogs.
    /// </summary>
    public interface IMattermostIntegrationsClient
    {
        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="triggerId"> Trigger identifier from a slash command or interactive action payload. </param>
        /// <param name="url"> URL where Mattermost sends the submitted dialog payload. </param>
        /// <param name="dialog"> Dialog definition. </param>
        Task OpenInteractiveDialogAsync(string triggerId, string url, InteractiveDialog dialog);

        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="request"> Open dialog request. </param>
        Task OpenInteractiveDialogAsync(OpenInteractiveDialogRequest request);

        /// <summary>Get incoming webhooks visible to the authenticated user.</summary>
        /// <param name="teamId">Optional team identifier. Null lists accessible webhooks across teams.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_own_incoming_webhooks. Managing others' webhooks requires manage_others_incoming_webhooks. Identifiers are secrets.</remarks>
        Task<IList<IncomingWebhook>> GetIncomingWebhooksAsync(string? teamId = null, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default);

        /// <summary>Get an incoming webhook. Requires incoming-webhook management permission.</summary>
        /// <param name="webhookId">Webhook identifier. Keep it secret.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IncomingWebhook> GetIncomingWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

        /// <summary>Create an incoming webhook. Requires incoming webhooks enabled and permission to manage webhooks and read the destination channel.</summary>
        /// <param name="webhook">Configuration with ChannelId. Empty UserId assigns the authenticated user; assigning another owner requires additional permissions. Server-owned IDs, team and timestamps are ignored.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IncomingWebhook> CreateIncomingWebhookAsync(IncomingWebhook webhook, CancellationToken cancellationToken = default);

        /// <summary>Replace an incoming webhook's settings. Requires incoming-webhook management permission.</summary>
        /// <param name="webhook">Complete configuration with Id and ChannelId. Read it first to preserve settings. Ownership and team cannot be changed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IncomingWebhook> UpdateIncomingWebhookAsync(IncomingWebhook webhook, CancellationToken cancellationToken = default);

        /// <summary>Delete an incoming webhook, invalidating its posting URL. Does not delete existing posts.</summary>
        /// <param name="webhookId">Webhook identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteIncomingWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

        /// <summary>Get outgoing webhooks visible to the authenticated user.</summary>
        /// <param name="teamId">Optional team identifier.</param>
        /// <param name="channelId">Optional channel identifier. Takes precedence over teamId on the server.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_own_outgoing_webhooks; managing others' webhooks requires manage_others_outgoing_webhooks. Returned tokens are secrets.</remarks>
        Task<IList<OutgoingWebhook>> GetOutgoingWebhooksAsync(string? teamId = null, string? channelId = null,
            int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get an outgoing webhook, including its secret token. Requires outgoing-webhook management permission.</summary>
        /// <param name="webhookId">Webhook identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OutgoingWebhook> GetOutgoingWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

        /// <summary>Create an outgoing webhook. Requires outgoing webhooks enabled and outgoing-webhook management permission.</summary>
        /// <param name="webhook">Configuration with TeamId and HTTP(S) CallbackUrls. ChannelId must identify a public channel, or be empty with TriggerWords to match the team. Empty CreatorId assigns the authenticated user. Assigning another owner requires additional permissions. Id, Token and timestamps are not sent.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OutgoingWebhook> CreateOutgoingWebhookAsync(OutgoingWebhook webhook, CancellationToken cancellationToken = default);

        /// <summary>Replace an outgoing webhook's settings. Requires outgoing-webhook management permission.</summary>
        /// <param name="webhook">Complete configuration with Id, TeamId and Token. Read it first: the server overwrites the token and other writable fields, rather than preserving omitted values. Ownership and team cannot be changed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OutgoingWebhook> UpdateOutgoingWebhookAsync(OutgoingWebhook webhook, CancellationToken cancellationToken = default);

        /// <summary>Regenerate an outgoing webhook's secret token. Update the callback receiver to accept the new token.</summary>
        /// <param name="webhookId">Webhook identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Updated webhook, including its new token.</returns>
        Task<OutgoingWebhook> RegenerateOutgoingWebhookTokenAsync(string webhookId, CancellationToken cancellationToken = default);

        /// <summary>Delete an outgoing webhook. Does not delete existing posts.</summary>
        /// <param name="webhookId">Webhook identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteOutgoingWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

        /// <summary>List visible built-in, plugin and manageable custom commands for a team.</summary>
        /// <param name="teamId">Team identifier. Requires view_team permission.</param>
        /// <param name="customOnly">Return only manageable custom commands. Requires manage_own_slash_commands.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<SlashCommand>> GetCommandsAsync(string teamId, bool customOnly = false, CancellationToken cancellationToken = default);

        /// <summary>Execute a slash command as the current user. The server processes its response and may create posts or invoke an external integration.</summary>
        /// <param name="channelId">Channel context. Requires membership and create_post permission.</param>
        /// <param name="command">Slash-prefixed command including arguments, such as "/echo hello". Argument whitespace is preserved.</param>
        /// <param name="teamId">Optional team context, particularly for direct/group channels. Ordinary channels determine their own team.</param>
        /// <param name="rootId">Optional thread root for response posts.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; the server may already have executed the command.</param>
        /// <returns>The command response, including any client trigger identifier. Empty server visibility is represented by a null ResponseType.</returns>
        Task<SlashCommandResponse> ExecuteCommandAsync(string channelId, string command, string? teamId = null,
            string? rootId = null, CancellationToken cancellationToken = default);

        /// <summary>Get a manageable custom command, including its secret. Requires Mattermost 5.22 or later.</summary>
        /// <param name="commandId">Command identifier. Requires view_team and manage_own_slash_commands; other creators' commands also require manage_others_slash_commands.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<SlashCommand> GetCommandAsync(string commandId, CancellationToken cancellationToken = default);

        /// <summary>Register a custom slash command. Requires enabled commands and manage_own_slash_commands.</summary>
        /// <param name="command">Team, trigger, request method, URL and display/autocomplete settings. CreatorId is optional; another creator requires manage_others_slash_commands. Identity, token and timestamps are server-generated. Plugin metadata is not persisted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<SlashCommand> CreateCommandAsync(SlashCommand command, CancellationToken cancellationToken = default);

        /// <summary>Replace a custom command's request and display/autocomplete settings.</summary>
        /// <param name="command">Full command settings, including Id and the current TeamId. The server preserves token, creator and timestamps; use MoveCommandAsync to change teams. Plugin metadata is not persisted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_own_slash_commands; other creators' commands also require manage_others_slash_commands.</remarks>
        Task<SlashCommand> UpdateCommandAsync(SlashCommand command, CancellationToken cancellationToken = default);

        /// <summary>Delete a manageable custom command.</summary>
        /// <param name="commandId">Command identifier. Requires manage_own_slash_commands; other creators' commands also require manage_others_slash_commands.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteCommandAsync(string commandId, CancellationToken cancellationToken = default);

        /// <summary>Move a custom command to another team. Requires Mattermost 5.22 or later.</summary>
        /// <param name="commandId">Command identifier. Requires permission to manage it on its current team.</param>
        /// <param name="teamId">Destination team. Both the caller and command creator need manage_own_slash_commands there.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task MoveCommandAsync(string commandId, string teamId, CancellationToken cancellationToken = default);

        /// <summary>Regenerate a custom command's secret and return its new value. The previous token immediately becomes invalid.</summary>
        /// <param name="commandId">Command identifier. Requires permission to manage the command.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<string> RegenerateCommandTokenAsync(string commandId, CancellationToken cancellationToken = default);

        /// <summary>List autocomplete-enabled commands for a team, including structured plugin metadata.</summary>
        /// <param name="teamId">Team identifier. Requires view_team permission.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<SlashCommand>> GetAutocompleteCommandsAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Get command and argument suggestions for the current input. Requires Mattermost 5.24 or later.</summary>
        /// <param name="teamId">Team identifier. Requires view_team permission.</param>
        /// <param name="userInput">Nonempty input, optionally prefixed by a slash. Use "/" for initial suggestions. Whitespace is preserved for argument completion.</param>
        /// <param name="channelId">Optional channel context for suggestions.</param>
        /// <param name="rootId">Optional thread root context for suggestions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<CommandAutocompleteSuggestion>> GetCommandAutocompleteSuggestionsAsync(string teamId, string userInput,
            string? channelId = null, string? rootId = null, CancellationToken cancellationToken = default);

        /// <summary>Read public OAuth authorization-server discovery metadata. Requires enabled OAuth service provider.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <remarks>Does not authenticate or send saved credentials. RegistrationEndpoint is empty when dynamic registration is disabled.</remarks>
        Task<OAuthServerMetadata> GetOAuthServerMetadataAsync(CancellationToken cancellationToken = default);

        /// <summary>List registered OAuth applications, including client secrets. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Without manage_system_wide_oauth, only applications created by the current account are returned.</remarks>
        Task<IList<OAuthApp>> GetOAuthAppsAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get an OAuth application, including its secret. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="appId">Application client identifier. Another creator's app also requires manage_system_wide_oauth.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OAuthApp> GetOAuthAppAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>Get OAuth application information without its client secret. Requires authentication and enabled OAuth service provider.</summary>
        /// <param name="appId">Application client identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<OAuthApp> GetOAuthAppInfoAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>Register a confidential OAuth application. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="application">Name, description, icon, HTTP(S) callback URLs, homepage and trust flag. Trust requires manage_system; identity, timestamps and client secret are server-generated.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; registration may already have completed.</param>
        /// <returns>The registered application, including its new client secret.</returns>
        Task<OAuthApp> CreateOAuthAppAsync(OAuthApp application, CancellationToken cancellationToken = default);

        /// <summary>Replace an OAuth application's settings. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="application">Full application settings including Id and MattermostAppId. Another creator's app also requires manage_system_wide_oauth. Trust changes require manage_system.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; settings may already have changed.</param>
        /// <remarks>Preserves creator, creation time, client secret and dynamic-registration flag. Unset editable fields are not preserved; this is not a patch.</remarks>
        Task<OAuthApp> UpdateOAuthAppAsync(OAuthApp application, CancellationToken cancellationToken = default);

        /// <summary>Delete a registered OAuth application. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="appId">Application client identifier. Another creator's app also requires manage_system_wide_oauth.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; deletion may already have completed.</param>
        Task DeleteOAuthAppAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>Replace a confidential OAuth application's secret and return the updated application. Requires enabled OAuth service provider and manage_oauth.</summary>
        /// <param name="appId">Application client identifier. Another creator's app also requires manage_system_wide_oauth. Public clients do not have a secret to regenerate.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; the secret may already have changed.</param>
        Task<OAuthApp> RegenerateOAuthAppSecretAsync(string appId, CancellationToken cancellationToken = default);

        /// <summary>List OAuth applications authorized by a user, without client secrets. Requires enabled OAuth service provider.</summary>
        /// <param name="userId">User identifier or me. Another account requires edit_other_users.</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<OAuthApp>> GetAuthorizedOAuthAppsAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default);
    }
}
