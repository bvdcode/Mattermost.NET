using Mattermost.Models.Groups;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    /// <summary>Directory groups, custom groups, membership and team/channel synchronization links.</summary>
    /// <remarks>Group endpoints require an applicable license. Custom groups also require EnableCustomGroups; LDAP links require LDAPGroups. Native feature, permission and visibility checks remain enforced.</remarks>
    public interface IMattermostGroupsClient
    {
        /// <summary>Get a page of visible groups with optional search filters.</summary>
        /// <remarks>Results exclude groups the caller cannot see. Counts and identifiers are optional; no automatic paging or totals request is performed.</remarks>
        Task<IList<Group>> GetGroupsAsync(int page = 0, int perPage = 60, GroupSearchOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>Get a group, optionally including member counts or identifiers.</summary>
        Task<Group> GetGroupAsync(string groupId, bool includeMemberCount = false, bool includeMemberIds = false, CancellationToken cancellationToken = default);

        /// <summary>Get visible groups by their mention names. Empty input returns an empty native list.</summary>
        Task<IList<Group>> GetGroupsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

        /// <summary>Create a mentionable custom group with optional initial users.</summary>
        /// <remarks>Requires create_custom_group, a supporting license and EnableCustomGroups. Source is custom and allow_reference is true; remote identity and server timestamps are not supplied. Initial users are copied before authentication.</remarks>
        Task<Group> CreateGroupAsync(string name, string displayName, IEnumerable<string>? userIds = null, string description = "", CancellationToken cancellationToken = default);

        /// <summary>Change supplied group metadata. Null omits a field; empty optional text clears it.</summary>
        /// <remarks>Requires edit_custom_group for custom groups, or sysconsole_write_user_management_groups for directory groups. Custom groups must remain mentionable. Source and remote identity are unchanged.</remarks>
        Task<Group> PatchGroupAsync(string groupId, string? name = null, string? displayName = null, string? description = null, bool? allowReference = null, CancellationToken cancellationToken = default);

        /// <summary>Soft-delete a custom group. Requires delete_custom_group.</summary>
        Task<Group> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default);

        /// <summary>Restore a soft-deleted custom group. Requires restore_custom_group.</summary>
        Task<Group> RestoreGroupAsync(string groupId, CancellationToken cancellationToken = default);

        /// <summary>Get a page of visible user profiles belonging to a group with total_member_count.</summary>
        Task<GroupMemberList> GetGroupMembersAsync(string groupId, int page = 0, int perPage = 60, CancellationToken cancellationToken = default);

        /// <summary>Add users to a custom group. Requires manage_custom_group_members.</summary>
        /// <remarks>Copies identifiers before authentication. Returns native membership rows; empty input is allowed. Does not add users to teams or channels.</remarks>
        Task<IList<GroupMember>> AddGroupMembersAsync(string groupId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);

        /// <summary>Remove users from a custom group. Requires manage_custom_group_members.</summary>
        /// <remarks>Copies identifiers before authentication. Returns native membership rows; empty input is allowed. Cancellation does not undo already applied changes.</remarks>
        Task<IList<GroupMember>> RemoveGroupMembersAsync(string groupId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);

        /// <summary>Get a group's native member count. Requires LDAPGroups and sysconsole_read_user_management_groups.</summary>
        Task<GroupStats> GetGroupStatsAsync(string groupId, CancellationToken cancellationToken = default);

        /// <summary>Get groups containing a user. Requires LDAPGroups; another user's lookup requires manage_system.</summary>
        Task<IList<Group>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>Get channel group associations and total matching count. Requires LDAPGroups and channel group-read permission.</summary>
        /// <remarks>paginate=false requests all matching groups. Query is trimmed; native visibility filtering can restrict results.</remarks>
        Task<GroupAssociationList> GetChannelGroupsAsync(string channelId, int page = 0, int perPage = 60, string? query = null,
            bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true, CancellationToken cancellationToken = default);

        /// <summary>Get team group associations and total matching count. Requires LDAPGroups and list_team_channels.</summary>
        /// <remarks>paginate=false requests all matching groups. Query is trimmed; native visibility filtering can restrict results.</remarks>
        Task<GroupAssociationList> GetTeamGroupsAsync(string teamId, int page = 0, int perPage = 60, string? query = null,
            bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true, CancellationToken cancellationToken = default);

        /// <summary>Get group associations indexed by channels of a team. Requires LDAPGroups and list_team_channels.</summary>
        /// <remarks>paginate=false disables native paging. This response has a groups map, not total_group_count.</remarks>
        Task<TeamChannelGroups> GetTeamChannelGroupsAsync(string teamId, int page = 0, int perPage = 60, string? query = null,
            bool includeMemberCount = false, bool filterAllowReference = false, bool paginate = true, CancellationToken cancellationToken = default);

        /// <summary>Link a directory group to a team.</summary>
        /// <remarks>Requires LDAPGroups and native linking permissions. Omitted fields follow the server's linking behavior; supply both flags when explicit values are required. Changing schemeAdmin additionally requires permission to manage roles. Membership synchronization is asynchronous; completion does not confirm all memberships are synchronized.</remarks>
        Task<GroupSyncable> LinkGroupToTeamAsync(string groupId, string teamId, bool? autoAdd = null, bool? schemeAdmin = null, CancellationToken cancellationToken = default);

        /// <summary>Link a directory group to a channel.</summary>
        /// <remarks>Requires LDAPGroups and native linking permissions. Omitted fields follow the server's linking behavior; supply both flags when explicit values are required. Changing schemeAdmin additionally requires permission to manage roles. For a team without group constraints, the server can also create or update the group's parent-team link, affecting its auto_add and scheme_admin flags. Membership synchronization is asynchronous.</remarks>
        Task<GroupSyncable> LinkGroupToChannelAsync(string groupId, string channelId, bool? autoAdd = null, bool? schemeAdmin = null, CancellationToken cancellationToken = default);

        /// <summary>Unlink a directory group from a team. Membership removal runs asynchronously.</summary>
        /// <remarks>Requires LDAPGroups and native linking permissions. May remove memberships controlled by the link; cancellation does not undo accepted changes.</remarks>
        Task UnlinkGroupFromTeamAsync(string groupId, string teamId, CancellationToken cancellationToken = default);

        /// <summary>Unlink a directory group from a channel. Membership removal runs asynchronously.</summary>
        /// <remarks>Requires LDAPGroups and native linking permissions. May remove memberships controlled by the link; cancellation does not undo accepted changes.</remarks>
        Task UnlinkGroupFromChannelAsync(string groupId, string channelId, CancellationToken cancellationToken = default);

        /// <summary>Get a group-to-team link. Requires LDAPGroups and manage_system.</summary>
        Task<GroupSyncable> GetGroupTeamAsync(string groupId, string teamId, CancellationToken cancellationToken = default);

        /// <summary>Get a group-to-channel link. Requires LDAPGroups and manage_system.</summary>
        Task<GroupSyncable> GetGroupChannelAsync(string groupId, string channelId, CancellationToken cancellationToken = default);

        /// <summary>Get all team links of a group. Requires LDAPGroups and sysconsole_read_user_management_groups.</summary>
        Task<IList<GroupSyncable>> GetGroupTeamsAsync(string groupId, CancellationToken cancellationToken = default);

        /// <summary>Get all channel links of a group. Requires LDAPGroups and sysconsole_read_user_management_groups.</summary>
        Task<IList<GroupSyncable>> GetGroupChannelsAsync(string groupId, CancellationToken cancellationToken = default);

        /// <summary>Change selected fields of a group-to-team link. Null preserves a field; false explicitly clears it.</summary>
        /// <remarks>Requires LDAPGroups and native linking/role-management permissions. Membership and role synchronization runs asynchronously.</remarks>
        Task<GroupSyncable> PatchGroupTeamAsync(string groupId, string teamId, bool? autoAdd = null, bool? schemeAdmin = null, CancellationToken cancellationToken = default);

        /// <summary>Change selected fields of a group-to-channel link. Null preserves a field; false explicitly clears it.</summary>
        /// <remarks>Requires LDAPGroups and native linking/role-management permissions. Membership and role synchronization runs asynchronously.</remarks>
        Task<GroupSyncable> PatchGroupChannelAsync(string groupId, string channelId, bool? autoAdd = null, bool? schemeAdmin = null, CancellationToken cancellationToken = default);
    }
}
