using Mattermost.Constants;
using Mattermost.Enums;
using Mattermost.Models.Teams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>
    /// Teams, team membership, invitations and team images.
    /// </summary>
    public interface IMattermostTeamsClient
    {
        /// <summary>Create a team. Requires create_team permission.</summary>
        /// <param name="name">Team URL name. Surrounding whitespace is removed.</param>
        /// <param name="displayName">Team display name. Surrounding whitespace is removed.</param>
        /// <param name="type">Open or invite-only membership.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The created team.</returns>
        Task<Team> CreateTeamAsync(string name, string displayName, TeamType type,
            CancellationToken cancellationToken = default);

        /// <summary>Partially update team metadata. Requires manage_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="displayName">Replacement display name.</param>
        /// <param name="description">Replacement description. An empty string clears it.</param>
        /// <param name="companyName">Replacement company name. An empty string clears it.</param>
        /// <param name="allowedDomains">Replacement allowed email domains. An empty string removes the restriction.</param>
        /// <param name="allowOpenInvite">Whether open invitations are allowed.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The updated team.</returns>
        /// <remarks>Null parameters are omitted. Empty strings and false are sent unchanged. Server validation applies.</remarks>
        Task<Team> PatchTeamAsync(string teamId, string? displayName = null, string? description = null,
            string? companyName = null, string? allowedDomains = null, bool? allowOpenInvite = null,
            CancellationToken cancellationToken = default);

        /// <summary>Search teams visible to the current user, without pagination.</summary>
        /// <param name="term">Search term. An empty string matches all visible teams.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Matching teams. Visibility depends on team-listing permissions.</returns>
        Task<IList<Team>> SearchTeamsAsync(string term, CancellationToken cancellationToken = default);

        /// <summary>Add a user to a team.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">Actual user identifier; the me alias is not supported.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>The team membership.</returns>
        /// <remarks>Adding another user requires add_user_to_team permission. Joining as the current user requires an open team or appropriate membership permission.</remarks>
        Task<TeamMember> AddTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get team by specified identifier.
        /// </summary>
        /// <param name="teamId"> Team identifier. </param>
        /// <returns> Team information. </returns>
        Task<Team> GetTeamAsync(string teamId);

        /// <summary>Download a team's icon as PNG bytes. Requires Mattermost 4.9 or later.</summary>
        /// <param name="teamId">Team identifier. Requires view_team permission unless it is an open team allowing open invitations.</param>
        /// <param name="cancellationToken">Cancels authentication and the download.</param>
        /// <remarks>Requires authentication and configured file storage. Returns HTTP 404 when no stored image is available.</remarks>
        Task<byte[]> GetTeamImageAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Replace a team's icon. Requires manage_team permission and Mattermost 4.9 or later.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="image">Nonempty image bytes. The server validates the format, size and dimensions.</param>
        /// <param name="fileName">Filename sent with the multipart image.</param>
        /// <param name="cancellationToken">Cancels authentication and the upload.</param>
        Task SetTeamImageAsync(string teamId, byte[] image, string fileName, CancellationToken cancellationToken = default);

        /// <summary>Remove a team's custom icon. Requires manage_team permission and Mattermost 4.10 or later.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>Resets the team's icon metadata. The server may retain the stored image at its download URL.</remarks>
        Task DeleteTeamImageAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a page of teams.
        /// </summary>
        /// <param name="page"> The page to select. </param>
        /// <param name="perPage"> The number of teams per page. </param>
        /// <returns> Teams visible to the current user. </returns>
        Task<IList<Team>> GetTeamsAsync(int page = 0, int perPage = 60);

        /// <summary>
        /// Get teams for a specified user.
        /// </summary>
        /// <param name="userId"> User identifier. </param>
        /// <returns> Teams the user belongs to. </returns>
        Task<IReadOnlyList<Team>> GetUserTeamsAsync(string userId);

        /// <summary>
        /// Get team by name.
        /// </summary>
        /// <param name="teamName"> Team name. </param>
        /// <returns> Team information. </returns>
        Task<Team> GetTeamByNameAsync(string teamName);

        /// <summary>
        /// Get one page of team memberships. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="page">Zero-based page number.</param>
        /// <param name="perPage">Members per page, from 1 to <see cref="MattermostApiLimits.MaxTeamMembersPerPage"/>.</param>
        /// <param name="sortByUsername">Sort by username instead of user identifier.</param>
        /// <param name="excludeDeletedUsers">Exclude deactivated users.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Membership records, not user profiles. The server may hide other members' role data.</returns>
        /// <exception cref="ArgumentException">The team identifier is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The page is negative or the page size is outside the allowed range.</exception>
        Task<IList<TeamMember>> GetTeamMembersAsync(string teamId, int page = 0, int perPage = 60,
            bool sortByUsername = false, bool excludeDeletedUsers = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's team membership. Requires permission to view the team and user.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Membership record. A missing membership is reported as an API error.</returns>
        /// <exception cref="ArgumentException">An identifier is blank.</exception>
        Task<TeamMember> GetTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get team memberships by user identifiers in one request. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userIds">A nonempty collection of actual user identifiers, not "me".</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Visible membership records. Missing or inaccessible members may be omitted; input order is not preserved.</returns>
        /// <exception cref="ArgumentNullException">The collection is null.</exception>
        /// <exception cref="ArgumentException">The team identifier is blank, or the collection is empty or contains blank identifiers.</exception>
        Task<IList<TeamMember>> GetTeamMembersByIdsAsync(string teamId, IEnumerable<string> userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's memberships across teams, subject to server visibility permissions.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Team membership and role records. The server may hide role data.</returns>
        /// <exception cref="ArgumentException">The user identifier is blank.</exception>
        Task<IList<TeamMember>> GetUserTeamMembersAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get membership counts for a team. Requires permission to view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Total and active member counts, subject to visibility restrictions.</returns>
        /// <exception cref="ArgumentException">The team identifier is blank.</exception>
        Task<TeamStats> GetTeamStatsAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check whether a team exists and is visible to the current user.
        /// </summary>
        /// <param name="teamName">Team URL name, not its display name.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>True when a visible team exists; false otherwise. HTTP failures are propagated.</returns>
        /// <exception cref="ArgumentException">The team name is blank.</exception>
        Task<bool> TeamExistsAsync(string teamName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a user's unread counts for a team. Requires permission to access the user and view the team.
        /// </summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Unread message and mention counts. This does not mark messages as read.</returns>
        /// <exception cref="ArgumentException">An identifier is blank.</exception>
        Task<TeamUnread> GetTeamUnreadAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unread counts across a user's teams. Reading another user's counts requires system administrator permissions.
        /// </summary>
        /// <param name="userId">User identifier, or "me" for the current user.</param>
        /// <param name="excludeTeamId">Team to omit. Null, empty or whitespace means no exclusion.</param>
        /// <param name="includeCollapsedThreads">Include unread counts for followed collapsed threads.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Unread counts for the user's teams. This does not mark messages as read.</returns>
        /// <exception cref="ArgumentException">The user identifier is blank.</exception>
        Task<IList<TeamUnread>> GetUserTeamsUnreadAsync(string userId, string? excludeTeamId = null,
            bool includeCollapsedThreads = false, CancellationToken cancellationToken = default);

        /// <summary>Update complete team metadata. Requires manage_team permission.</summary>
        /// <param name="team">Team with an identifier and all writable metadata. Read it first to preserve fields. Invite settings additionally require invite_user permission.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Team> UpdateTeamAsync(Team team, CancellationToken cancellationToken = default);

        /// <summary>Delete a team. By default this is a reversible soft deletion. Requires manage_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="permanent">Irreversibly delete the team and its data. Requires the EnableAPITeamDeletion server setting.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task DeleteTeamAsync(string teamId, bool permanent = false, CancellationToken cancellationToken = default);

        /// <summary>Restore a soft-deleted team. Requires manage_team permission and Mattermost 5.24 or later.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Team> RestoreTeamAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Change team privacy. Requires manage_team and invite_user permissions.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="privacy">Open or InviteOnly. This also enables or disables open invitations, respectively.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task<Team> UpdateTeamPrivacyAsync(string teamId, TeamType privacy, CancellationToken cancellationToken = default);

        /// <summary>Regenerate a team's invitation identifier, invalidating its previous invite link. Requires manage_team and invite_user permissions.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <returns>Team metadata with the new InviteId.</returns>
        Task<Team> RegenerateTeamInviteIdAsync(string teamId, CancellationToken cancellationToken = default);

        /// <summary>Read public metadata for an open team from its invitation identifier without authentication. Requires Mattermost 4.0 or later.</summary>
        /// <param name="inviteId">Team invitation identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>Team with only Id, Name, DisplayName and Description populated.</returns>
        Task<Team> GetTeamInviteInfoAsync(string inviteId, CancellationToken cancellationToken = default);

        /// <summary>Add multiple users to a team. Requires add_user_to_team permission. Errors abort the request rather than returning per-user errors.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userIds">Nonempty collection of up to 256 actual user identifiers, not the me alias. Match results by UserId.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        /// <remarks>An error does not guarantee rollback of users already added by the server.</remarks>
        Task<IList<TeamMember>> AddTeamMembersAsync(string teamId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);

        /// <summary>Remove a team member, including their channel memberships. Removing another user requires remove_user_from_team permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or me to leave the team.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task RemoveTeamMemberAsync(string teamId, string userId, CancellationToken cancellationToken = default);

        /// <summary>Replace a member's team roles. Requires manage_team_roles permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="roles">Space-delimited roles, passed unchanged. The server validates role names.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateTeamMemberRolesAsync(string teamId, string userId, string roles, CancellationToken cancellationToken = default);

        /// <summary>Replace a member's scheme-derived team roles. Requires Mattermost 5.0 or later and manage_team_roles permission.</summary>
        /// <param name="teamId">Team identifier.</param>
        /// <param name="userId">User identifier, or me for the current user.</param>
        /// <param name="schemeUser">Whether the member has the scheme user role.</param>
        /// <param name="schemeAdmin">Whether the member has the scheme administrator role.</param>
        /// <param name="schemeGuest">Whether the member has the scheme guest role, mutually exclusive with schemeUser.</param>
        /// <param name="cancellationToken">Cancels authentication and the request.</param>
        Task UpdateTeamMemberSchemeRolesAsync(string teamId, string userId, bool schemeUser, bool schemeAdmin,
            bool schemeGuest = false, CancellationToken cancellationToken = default);
    }
}
