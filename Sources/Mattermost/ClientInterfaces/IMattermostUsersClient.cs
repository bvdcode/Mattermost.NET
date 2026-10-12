using Mattermost.Models.Bots;
using Mattermost.Models.Channels;
using Mattermost.Models.Enums;
using Mattermost.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// User profiles, account management, presence, bot accounts, access tokens and preferences.
    /// </summary>
    public interface IMattermostUsersClient
    {
        /// <summary>Count users matching administrator filters. Requires Mattermost 5.26 or later.</summary>
        /// <param name="teamId">Optional team filter.</param>
        /// <param name="channelId">Optional channel filter.</param>
        /// <param name="includeDeleted">Include deactivated accounts.</param>
        /// <param name="includeBots">Include bot accounts.</param>
        /// <param name="roles">Optional nonempty collection of system role names; matches any role.</param>
        /// <param name="teamRoles">Optional nonempty collection of team role names; requires teamId.</param>
        /// <param name="channelRoles">Optional nonempty collection of channel role names; requires channelId.</param>
        /// <param name="includeRemoteUsers">Include remote accounts on supporting servers.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires sysconsole_read_user_management_users. Role names are trimmed. Native role filters are combined with OR; team and channel roles exclude system administrators.</remarks>
        Task<long> GetFilteredUserCountAsync(string? teamId = null, string? channelId = null, bool includeDeleted = false,
            bool includeBots = false, IEnumerable<string>? roles = null, IEnumerable<string>? teamRoles = null,
            IEnumerable<string>? channelRoles = null, bool includeRemoteUsers = false, CancellationToken cancellationToken = default);

        /// <summary>Download a user's current profile image.</summary>
        /// <param name="userId">User identifier or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete profile image as bytes.</returns>
        /// <remarks>Requires authentication and permission to view the user. The server returns its generated image when no custom image is set.</remarks>
        Task<byte[]> GetUserImageAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Download a user's generated default profile image, even when a custom image is set. Requires Mattermost 5.5 or later.</summary>
        /// <param name="userId">User identifier or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <returns>The complete default image as bytes.</returns>
        /// <remarks>Requires authentication and permission to view the user.</remarks>
        Task<byte[]> GetDefaultUserImageAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Replace a user's profile image.</summary>
        /// <param name="userId">User identifier or me. Requires permission to edit the user or bot.</param>
        /// <param name="image">Nonempty image bytes. The server validates the format, size and dimensions.</param>
        /// <param name="fileName">Filename sent with the multipart image.</param>
        /// <param name="cancellationToken">Cancels authentication and the upload.</param>
        /// <remarks>Requires configured file storage. The server rejects locked or externally synchronized profile images.</remarks>
        Task SetUserImageAsync(string userId, byte[] image, string fileName, CancellationToken cancellationToken = default);

        /// <summary>Remove a user's custom profile image and reset it to the generated default.</summary>
        /// <param name="userId">User identifier or me. Requires permission to edit the user or bot.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires configured file storage. The server rejects locked profile images.</remarks>
        Task DeleteUserImageAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Get a page of bot accounts visible to the current user. Requires Mattermost 5.10 or later.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size; the server may limit it.</param>
        /// <param name="includeDeleted">Include disabled bot accounts.</param>
        /// <param name="onlyOrphaned">Return only bots whose owner has been deactivated.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires read_bots; reading bots owned by others also requires read_others_bots.</remarks>
        Task<IList<Bot>> GetBotsAsync(int page = 0, int perPage = 60, bool includeDeleted = false, bool onlyOrphaned = false,
            CancellationToken cancellationToken = default);

        /// <summary>Get a bot account. Requires Mattermost 5.10 or later and permission to read the bot.</summary>
        /// <param name="botUserId">Bot's user identifier.</param>
        /// <param name="includeDeleted">Include a disabled bot account.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Bot> GetBotAsync(string botUserId, bool includeDeleted = false, CancellationToken cancellationToken = default);

        /// <summary>Create a bot account owned by the current user. Requires Mattermost 5.10 or later.</summary>
        /// <param name="username">Nonblank username. The server validates its format and uniqueness.</param>
        /// <param name="displayName">Display name; null omits the field.</param>
        /// <param name="description">Description; null omits the field.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires create_bot and enabled bot creation. Bot accounts cannot create other bots. Does not create an API token.</remarks>
        Task<Bot> CreateBotAsync(string username, string? displayName = null, string? description = null,
            CancellationToken cancellationToken = default);

        /// <summary>Change selected bot metadata without replacing omitted fields. Requires Mattermost 5.10 or later.</summary>
        /// <param name="botUserId">Bot's user identifier.</param>
        /// <param name="username">New nonblank username; null leaves it unchanged.</param>
        /// <param name="displayName">New display name; null leaves it unchanged, empty clears it.</param>
        /// <param name="description">New description; null leaves it unchanged, empty clears it.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires permission to manage the bot. At least one field must be supplied.</remarks>
        Task<Bot> PatchBotAsync(string botUserId, string? username = null, string? displayName = null, string? description = null,
            CancellationToken cancellationToken = default);

        /// <summary>Disable a bot account. Requires Mattermost 5.10 or later and permission to manage the bot.</summary>
        /// <param name="botUserId">Bot's user identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Protected system bots may not be disabled. Existing credentials cannot authenticate while the bot is disabled.</remarks>
        Task<Bot> DisableBotAsync(string botUserId, CancellationToken cancellationToken = default);

        /// <summary>Enable a disabled bot account. Requires Mattermost 5.10 or later and permission to manage the bot.</summary>
        /// <param name="botUserId">Bot's user identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Bot> EnableBotAsync(string botUserId, CancellationToken cancellationToken = default);

        /// <summary>Assign a bot to a user. Requires Mattermost 5.10 or later and permission to manage the bot.</summary>
        /// <param name="botUserId">Bot's user identifier.</param>
        /// <param name="userId">New owner's identifier. The owner must not be a bot.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Bot> AssignBotAsync(string botUserId, string userId, CancellationToken cancellationToken = default);

        /// <summary>Convert a normal account into a bot. Requires Mattermost 5.26 or later and manage_system permission.</summary>
        /// <param name="userId">Account identifier. The server clears external authentication credentials and revokes all account sessions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; conversion may already have completed.</param>
        /// <returns>Bot metadata for the same account identifier. The converted account becomes its own bot owner.</returns>
        Task<Bot> ConvertUserToBotAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Convert a bot into a normal account with a password. Requires Mattermost 5.26 or later and manage_system permission.</summary>
        /// <param name="botUserId">Bot account identifier, retained by the converted account.</param>
        /// <param name="password">New account password, preserved exactly and validated against the server password policy.</param>
        /// <param name="email">Optional replacement email. Null preserves the existing email.</param>
        /// <param name="setSystemAdmin">Ask the server to grant the system administrator role. False does not remove an existing administrator role.</param>
        /// <param name="cancellationToken">Cancels authentication and the request; conversion may already have completed.</param>
        /// <returns>The updated account profile. Use PatchUserAsync for other profile settings.</returns>
        Task<User> ConvertBotToUserAsync(string botUserId, string password, string? email = null,
            bool setSystemAdmin = false, CancellationToken cancellationToken = default);

        /// <summary>Create a personal access token. Requires Mattermost 4.1 or later.</summary>
        /// <param name="userId">User identifier or "me".</param>
        /// <param name="description">Nonblank description of the intended use.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Created token, including its secret value. Store it securely; later reads omit the secret.</returns>
        /// <remarks>Requires create_user_access_token and permission to edit the user or bot. Personal tokens must be enabled for non-bot users. Server token-lifetime policies apply; no expiry is requested.</remarks>
        Task<UserAccessToken> CreateUserAccessTokenAsync(string userId, string description, CancellationToken cancellationToken = default);

        /// <summary>Get a user's personal access token metadata. Requires Mattermost 4.1 or later.</summary>
        /// <param name="userId">User identifier or "me".</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires read_user_access_token and permission to edit the user or bot. Secret values are omitted.</remarks>
        Task<IList<UserAccessToken>> GetUserAccessTokensAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default);

        /// <summary>Get personal access token metadata across the server. Requires Mattermost 4.7 or later.</summary>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive page size.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Secret values are omitted.</remarks>
        Task<IList<UserAccessToken>> GetAllUserAccessTokensAsync(int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Get a personal access token's metadata. Requires Mattermost 4.1 or later.</summary>
        /// <param name="tokenId">Token identifier, not its secret value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires read_user_access_token and permission to edit the owning user or bot. The secret value is omitted.</remarks>
        Task<UserAccessToken> GetUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>Search token metadata by token ID, user ID or username. Requires Mattermost 4.7 or later.</summary>
        /// <param name="term">Nonblank search term.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Secret values are omitted.</remarks>
        Task<IList<UserAccessToken>> SearchUserAccessTokensAsync(string term, CancellationToken cancellationToken = default);

        /// <summary>Temporarily disable a personal access token. Requires Mattermost 4.4 or later.</summary>
        /// <param name="tokenId">Token identifier, not its secret value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires revoke_user_access_token and permission to edit the owning user or bot. Use another credential to re-enable it.</remarks>
        Task DisableUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>Re-enable a disabled personal access token. Requires Mattermost 4.4 or later.</summary>
        /// <param name="tokenId">Token identifier, not its secret value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires create_user_access_token and permission to edit the owning user or bot. This does not extend expiry.</remarks>
        Task EnableUserAccessTokenAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>Count active personal access tokens that violate the server's maximum-lifetime policy. Requires Mattermost 11.1 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Number of non-compliant tokens; zero when no maximum lifetime is configured.</returns>
        /// <remarks>Requires manage_system. Does not revoke or modify tokens.</remarks>
        Task<long> GetNonCompliantUserAccessTokenCountAsync(CancellationToken cancellationToken = default);

        /// <summary>Create an email/password account using the current authenticated session.</summary>
        /// <param name="username">New username.</param>
        /// <param name="email">Email address.</param>
        /// <param name="password">Initial password, passed unchanged. Must meet server password policy.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Server signup settings apply unless the session is a system administrator. Does not support invitation or external-authentication signup.</remarks>
        Task<User> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);

        /// <summary>Change only supplied user profile fields.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="patch">At least one profile field. Null omits a field; empty optional text clears it. Supplied maps replace previous maps.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires ownership or edit_other_users; provider-managed or locked profile fields may not be changed. Own-email changes require the current password in patch.Password.</remarks>
        Task<User> PatchUserAsync(string userId, UserPatch patch, CancellationToken cancellationToken = default);

        /// <summary>Deactivate an account and revoke its sessions. Does not permanently delete the account.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires ownership with self-deactivation enabled, or edit_other_users. Editing a system administrator requires manage_system.</remarks>
        Task DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Activate or deactivate a user account.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="active">True activates; false deactivates and revokes sessions.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Self-deactivation depends on server settings. Other changes require user-management write permission; system-administrator accounts additionally require manage_system. LDAP-managed status cannot be changed.</remarks>
        Task UpdateUserActiveAsync(string userId, bool active, CancellationToken cancellationToken = default);

        /// <summary>Replace a user's system-level roles.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="roles">Space-separated native system role names; replaces all previous system roles.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_roles; some role names additionally require a license.</remarks>
        Task UpdateUserRolesAsync(string userId, string roles, CancellationToken cancellationToken = default);

        /// <summary>Change a user's password subject to the server's password policy.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="newPassword">New password, passed unchanged.</param>
        /// <param name="currentPassword">Required for your own account; an authorized administrator changing another account may omit it.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Other-user changes require administrative user-management permission. The server may send an email notification.</remarks>
        Task UpdateUserPasswordAsync(string userId, string newPassword, string? currentPassword = null,
            CancellationToken cancellationToken = default);

        /// <summary>Reset the failed password-attempt counter for an email/password or LDAP account.</summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires sysconsole_write_user_management_users. System-administrator accounts additionally require manage_system.</remarks>
        Task ResetUserFailedAttemptsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Administratively verify an account's email without a verification token.</summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires manage_system. Returns the native user response; it does not send a verification email.</remarks>
        Task<User> VerifyUserEmailAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Revoke one session belonging to the specified user.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="sessionId">Session identifier, not the session token.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires ownership or edit_other_users. Revoking a session does not delete a personal access token.</remarks>
        Task RevokeUserSessionAsync(string userId, string sessionId, CancellationToken cancellationToken = default);

        /// <summary>Revoke all active sessions for one user. Requires Mattermost 4.4 or later.</summary>
        /// <param name="userId">User identifier, or me.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Requires ownership or edit_other_users. Does not delete personal access tokens; a valid access token can create a new session.</remarks>
        Task RevokeAllUserSessionsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current authorized user information.
        /// </summary>
        /// <returns> Authorized user information. </returns>
        Task<User> GetMeAsync();

        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserAsync(string userId);

        /// <summary>
        /// Get a page of users.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of users per page. </param>
        /// <param name="inTeamId"> Only users in this team. </param>
        /// <param name="notInTeamId"> Only users not in this team. </param>
        /// <param name="inChannelId"> Only users in this channel. </param>
        /// <param name="notInChannelId"> Only users not in this channel. </param>
        /// <param name="active"> Only active users. </param>
        /// <param name="inactive"> Only inactive users. </param>
        /// <returns> Users matching the query. </returns>
        Task<IList<User>> GetUsersAsync(
            int page = 0,
            int perPage = 60,
            string? inTeamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool? active = null,
            bool? inactive = null);

        /// <summary>
        /// Search users by term.
        /// </summary>
        /// <param name="term"> Search term matched against username, full name, nickname and email. </param>
        /// <param name="teamId"> Only search users on this team. </param>
        /// <param name="notInTeamId"> Only search users not on this team. </param>
        /// <param name="inChannelId"> Only search users in this channel. </param>
        /// <param name="notInChannelId"> Only search users not in this channel. Must specify teamId when using this option. </param>
        /// <param name="groupConstrained"> Only users allowed to join based on group constraints. </param>
        /// <param name="allowInactive"> Include deactivated users in the results. </param>
        /// <param name="withoutTeam"> Search users that are not on a team. </param>
        /// <param name="limit"> Maximum number of users to return. </param>
        /// <returns> Users matching the search term. </returns>
        Task<IList<User>> SearchUsersAsync(
            string term,
            string? teamId = null,
            string? notInTeamId = null,
            string? inChannelId = null,
            string? notInChannelId = null,
            bool groupConstrained = false,
            bool allowInactive = false,
            bool withoutTeam = false,
            int? limit = null);

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username"> Username. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Get user by email address.
        /// </summary>
        /// <param name="email"> Email address. </param>
        /// <returns> User information. </returns>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>
        /// Get multiple user profiles in one request.
        /// </summary>
        /// <param name="userIds">A nonempty collection of user identifiers. Surrounding whitespace is removed.</param>
        /// <param name="since">Only profiles updated after this Unix timestamp in milliseconds. Zero disables filtering. Requires Mattermost 5.14 or later when used.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible user profiles. Missing or inaccessible users may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank identifiers.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The timestamp is negative.</exception>
        Task<IList<User>> GetUsersByIdsAsync(IEnumerable<string> userIds, long since = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get multiple user profiles by username in one request.
        /// </summary>
        /// <param name="usernames">A nonempty collection of usernames. Surrounding whitespace and leading @ characters are removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible user profiles. Missing or inaccessible users may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank usernames.</exception>
        Task<IList<User>> GetUsersByUsernamesAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's presence status.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Presence status and activity metadata.</returns>
        /// <exception cref="ArgumentException">The user identifier is null, empty, or whitespace.</exception>
        Task<UserPresence> GetUserStatusAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get multiple users' presence statuses in one request.
        /// </summary>
        /// <param name="userIds">A nonempty collection of actual user identifiers, not "me". Surrounding whitespace is removed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Presence statuses returned by the server; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The collection is empty or contains blank identifiers.</exception>
        Task<IList<UserPresence>> GetUsersStatusesByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

        /// <summary>Get identifiers of users sharing channels with the current account, including direct and group channels. Requires Mattermost 5.23 or later.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<string>> GetKnownUserIdsAsync(CancellationToken cancellationToken = default);

        /// <summary>Autocomplete visible users by username, nickname or full name, subject to privacy settings.</summary>
        /// <param name="name">Search text. An empty string requests initial suggestions; email addresses are not searched.</param>
        /// <param name="teamId">Optional team filter.</param>
        /// <param name="channelId">Optional channel filter. Requires teamId and returns separate in-channel and out-of-channel suggestions.</param>
        /// <param name="limit">Maximum users per result group. The server may cap this value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<UserAutocomplete> AutocompleteUsersAsync(string name, string? teamId = null, string? channelId = null,
            int limit = 100, CancellationToken cancellationToken = default);

        /// <summary>Get the server-reported total user count, subject to user visibility restrictions.</summary>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<long> GetTotalUserCountAsync(CancellationToken cancellationToken = default);

        /// <summary>Get other members keyed by group-channel identifier, excluding the current user. Requires Mattermost 5.14 or later.</summary>
        /// <param name="channelIds">Between 1 and 50 group-channel identifiers. Channels the current user does not belong to are omitted.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IDictionary<string, IList<User>>> GetUsersByGroupChannelIdsAsync(IEnumerable<string> channelIds,
            CancellationToken cancellationToken = default);

        /// <summary>Get a page of channel memberships across teams, with team metadata. Requires Mattermost 6.2 or later.</summary>
        /// <param name="userId">User identifier, or "me". Reading another user's memberships requires edit_other_users.</param>
        /// <param name="page">Zero-based page. The request always uses the server's paginated JSON-array mode.</param>
        /// <param name="perPage">Positive number of memberships per page. The server may cap this value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<ChannelMemberWithTeamData>> GetUserChannelMembersAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default);

        /// <summary>Get user session metadata without usable session tokens.</summary>
        /// <param name="userId">User identifier, or "me". Reading another user's sessions requires edit_other_users.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<UserSession>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Get a page of user audit records.</summary>
        /// <param name="userId">User identifier, or "me". Reading another user's audit history requires edit_other_users.</param>
        /// <param name="page">Zero-based page.</param>
        /// <param name="perPage">Positive number of records per page. The server may cap this value.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<UserAudit>> GetUserAuditsAsync(string userId, int page = 0, int perPage = 60,
            CancellationToken cancellationToken = default);

        /// <summary>Set a user's presence status. Updating another user requires edit_other_users permission.</summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="status">Online, Offline, Away or DoNotDisturb. Unknown and OutOfOffice cannot be set through this endpoint.</param>
        /// <param name="dndEndTime">Do not disturb expiry as a Unix timestamp in seconds. Zero means no expiry; ignored for other statuses.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The user's updated presence metadata.</returns>
        Task<UserPresence> UpdateUserStatusAsync(string userId, UserStatus status, long dndEndTime = 0,
            CancellationToken cancellationToken = default);

        /// <summary>Set a user's custom status and add it to their recent status history. Requires enabled custom statuses.</summary>
        /// <param name="userId">User identifier, or "me" for the current user. Updating another user requires edit_other_users permission.</param>
        /// <param name="emoji">Emoji name, preserved exactly. May be empty when text is supplied.</param>
        /// <param name="text">Status text, preserved exactly. The server truncates it to 100 Unicode characters.</param>
        /// <param name="duration">Expiration label. Never has no expiry; every other label requires expiresAt.</param>
        /// <param name="expiresAt">Explicit expiration instant. Duration labels do not calculate this value; supply a future instant accepted by the server.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateUserCustomStatusAsync(string userId, string emoji, string text,
            CustomStatusDuration duration = CustomStatusDuration.Never, DateTimeOffset? expiresAt = null,
            CancellationToken cancellationToken = default);

        /// <summary>Clear a user's current custom status without removing its recent history. Requires enabled custom statuses.</summary>
        /// <param name="userId">User identifier, or "me" for the current user. Updating another user requires edit_other_users permission.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task ClearUserCustomStatusAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Remove one custom status from recent history using the POST endpoint. Requires enabled custom statuses.</summary>
        /// <param name="userId">User identifier, or "me" for the current user. Updating another user requires edit_other_users permission.</param>
        /// <param name="emoji">Exact stored emoji name.</param>
        /// <param name="text">Exact stored text, including any truncation performed by the server.</param>
        /// <param name="duration">Exact stored duration label.</param>
        /// <param name="expiresAt">Exact stored expiry instant, or null or MinValue for a non-expiring status. An expired instant may be supplied.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>All four fields must match the history entry. Removing a history entry does not clear the current custom status.</remarks>
        Task DeleteRecentUserCustomStatusAsync(string userId, string emoji, string text,
            CustomStatusDuration duration = CustomStatusDuration.Never, DateTimeOffset? expiresAt = null,
            CancellationToken cancellationToken = default);

        /// <summary>Get a user's stored preferences. Requires access to that user.</summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Preference>> GetPreferencesAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Get preferences in one category. A missing category produces an API error.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="category">Exact category key, without trimming. Server route restrictions apply.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<IList<Preference>> GetPreferencesByCategoryAsync(string userId, string category, CancellationToken cancellationToken = default);

        /// <summary>Get a preference by category and name. A missing preference produces an API error.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="category">Exact category key, without trimming. Server route restrictions apply.</param>
        /// <param name="name">Exact nonempty preference key, without trimming. Use category lookup for preferences with empty names.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Preference> GetPreferenceAsync(string userId, string category, string name, CancellationToken cancellationToken = default);

        /// <summary>Save 1–100 preferences. Unspecified preferences are not changed.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="preferences">Preferences with the target user's actual UserId. Names and values may be empty and are not trimmed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdatePreferencesAsync(string userId, IEnumerable<Preference> preferences, CancellationToken cancellationToken = default);

        /// <summary>Delete 1–100 preferences by user, category, and name.</summary>
        /// <param name="userId">User identifier, or "me".</param>
        /// <param name="preferences">Preference keys with the target user's actual UserId. Value is ignored by the server.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeletePreferencesAsync(string userId, IEnumerable<Preference> preferences, CancellationToken cancellationToken = default);
    }
}
