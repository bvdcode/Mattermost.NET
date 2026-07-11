# Mattermost.NET API Coverage

This document tracks the current Mattermost HTTP API surface from the official OpenAPI specification and maps each operation to the public Mattermost.NET SDK surface when a wrapper exists.

Routes implemented by the SDK but missing from the current Mattermost OpenAPI specification are kept in this document instead of being dropped from coverage.

- Mattermost API documentation: https://developers.mattermost.com/api-documentation/
- Mattermost OpenAPI specification: https://developers.mattermost.com/mattermost-openapi-v4.yaml
- Last reviewed: 2026-07-09
- Implemented operations: 27/631
- SDK-only implemented routes missing from the current OpenAPI specification: 2

## Legend

| Status | Meaning |
|---|---|
| ✅ Implemented | Public `IMattermostClient` method exists and maps to this Mattermost operation. |
| ❌ Not implemented | No public wrapper currently exists in `IMattermostClient`. |

## Coverage

# Access Control

## PUT /api/v4/access_control_policies - Create an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateAccessControlPolicy) | `—`
❌ Not implemented

## PUT /api/v4/access_control_policies/activate - Activate or deactivate access control policies
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateAccessControlPoliciesActive) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/cel/autocomplete/fields - Get autocomplete fields for access control policies
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAccessControlPolicyAutocompleteFields) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/check - Check an access control policy expression
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CheckAccessControlPolicyExpression) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/simulate_users - Simulate an access control policy decision for an explicit user list
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SimulateAccessControlPolicyForUsers) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/test - Test an access control policy expression
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestAccessControlPolicyExpression) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/validate_requester - Validate if the current user matches a CEL expression
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ValidateExpressionAgainstRequester) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/visual_ast - Get the visual AST for a CEL expression
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCELVisualAST) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/search - Search access control policies
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchAccessControlPolicies) | `—`
❌ Not implemented

## DELETE /api/v4/access_control_policies/{policy_id} - Delete an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteAccessControlPolicy) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id} - Get an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAccessControlPolicy) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id}/activate - Activate or deactivate an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateAccessControlPolicyActiveStatus) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/{policy_id}/assign - Assign an access control policy to channels or teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AssignAccessControlPolicyToChannels) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id}/resources/channels - Get channels for an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsForAccessControlPolicy) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/{policy_id}/resources/channels/search - Search channels for an access control policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchChannelsForAccessControlPolicy) | `—`
❌ Not implemented

## DELETE /api/v4/access_control_policies/{policy_id}/unassign - Unassign an access control policy from channels or teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnassignAccessControlPolicyFromChannels) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/access_control/attributes - Get access control attributes for a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelAccessControlAttributes) | `—`
❌ Not implemented

# Agents

## GET /api/v4/agents - Get available agents
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAgents) | `—`
❌ Not implemented

## GET /api/v4/agents/status - Get agents bridge status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAgentsStatus) | `—`
❌ Not implemented

## GET /api/v4/llmservices - Get available LLM services
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLLMServices) | `—`
❌ Not implemented

# Audit Logs

## DELETE /api/v4/audit_logs/certificate - Remove audit log certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveAuditLogCertificate) | `—`
❌ Not implemented

## POST /api/v4/audit_logs/certificate - Upload audit log certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddAuditLogCertificate) | `—`
❌ Not implemented

# Boards

## POST /api/v4/boards - Create a board channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateBoard) | `—`
❌ Not implemented

# Bookmarks

## GET /api/v4/channels/{channel_id}/bookmarks - Get channel bookmarks for Channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListChannelBookmarksForChannel) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/bookmarks - Create channel bookmark
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateChannelBookmark) | `—`
❌ Not implemented

## DELETE /api/v4/channels/{channel_id}/bookmarks/{bookmark_id} - Delete channel bookmark
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteChannelBookmark) | `—`
❌ Not implemented

## PATCH /api/v4/channels/{channel_id}/bookmarks/{bookmark_id} - Update channel bookmark
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelBookmark) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/bookmarks/{bookmark_id}/sort_order - Update channel bookmark's order
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelBookmarkSortOrder) | `—`
❌ Not implemented

# Bots

## GET /api/v4/bots - Get bots
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetBots) | `—`
❌ Not implemented

## POST /api/v4/bots - Create a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateBot) | `—`
❌ Not implemented

## GET /api/v4/bots/{bot_user_id} - Get a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetBot) | `—`
❌ Not implemented

## PUT /api/v4/bots/{bot_user_id} - Patch a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchBot) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/assign/{user_id} - Assign a bot to a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AssignBot) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/convert_to_user - Convert a bot into a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ConvertBotToUser) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/disable - Disable a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DisableBot) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/enable - Enable a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/EnableBot) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/convert_to_bot - Convert a user into a bot
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ConvertUserToBot) | `—`
❌ Not implemented

# Brand

## DELETE /api/v4/brand/image - Delete current brand image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteBrandImage) | `—`
❌ Not implemented

## GET /api/v4/brand/image - Get brand image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetBrandImage) | `—`
❌ Not implemented

## POST /api/v4/brand/image - Upload brand image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadBrandImage) | `—`
❌ Not implemented

# Calls Plugin

## POST /plugins/com.mattermost.calls/{channel_id} - Set channel call state
[Not listed in current Mattermost OpenAPI specification](https://developers.mattermost.com/mattermost-openapi-v4.yaml) | `IMattermostClient.SetChannelCallStateAsync`
✅ Implemented

# Channels

## GET /api/v4/channels - Get a list of all channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAllChannels) | `—`
❌ Not implemented

## POST /api/v4/channels - Create a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateChannel) | `IMattermostClient.CreateChannelAsync`
✅ Implemented

## POST /api/v4/channels/direct - Create a direct message channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateDirectChannel) | `IMattermostClient.CreateDirectChannelAsync`
✅ Implemented

## POST /api/v4/channels/group - Create a group message channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateGroupChannel) | `IMattermostClient.CreateGroupChannelAsync`
✅ Implemented

## POST /api/v4/channels/group/search - Search Group Channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchGroupChannels) | `—`
❌ Not implemented

## PUT /api/v4/channels/members/{user_id}/direct/read - Mark all direct and group messages as read
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkAllDirectMessagesRead) | `—`
❌ Not implemented

## POST /api/v4/channels/members/{user_id}/mark_read - Mark multiple channels as read
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkChannelsReadForUser) | `—`
❌ Not implemented

## POST /api/v4/channels/members/{user_id}/view - View channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ViewChannel) | `—`
❌ Not implemented

## POST /api/v4/channels/search - Search all private and open type channels across all teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchAllChannels) | `—`
❌ Not implemented

## POST /api/v4/channels/stats/member_count - Get member counts for multiple channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsMemberCount) | `—`
❌ Not implemented

## DELETE /api/v4/channels/{channel_id} - Delete a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteChannel) | `IMattermostClient.ArchiveChannelAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id} - Get a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannel) | `IMattermostClient.GetChannelAsync`
✅ Implemented

## PUT /api/v4/channels/{channel_id} - Update a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannel) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/common_teams - Get common teams for members of a Group Message.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupMessageMembersCommonTeams) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/convert_to_channel - Convert group message to private channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ConvertGroupMessageToChannel) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/member_counts_by_group - Channel members counts for each group that has atleast one member in the channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMemberCountsByGroup) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/members - Get channel members
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMembers) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/members - Add user(s) to channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddChannelMember) | `IMattermostClient.AddUserToChannelAsync`
✅ Implemented

## PUT /api/v4/channels/{channel_id}/members - Set channel members
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetChannelMembers) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/members/ids - Get channel members by ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMembersByIds) | `—`
❌ Not implemented

## DELETE /api/v4/channels/{channel_id}/members/{user_id} - Remove user from channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveUserFromChannel) | `IMattermostClient.DeleteUserFromChannelAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/members/{user_id} - Get channel member
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMember) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/autotranslation - Update channel member autotranslation setting
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelMemberAutotranslation) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/notify_props - Update channel notifications
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelNotifyProps) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/roles - Update channel roles
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelRoles) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/schemeRoles - Update the scheme-derived roles of a channel member.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelMemberSchemeRoles) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/members_minus_group_members - Channel members minus group members.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ChannelMembersMinusGroupMembers) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/moderations - Get information about channel's moderation.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelModerations) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/moderations/patch - Update a channel's moderation settings.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchChannelModerations) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/move - Move a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MoveChannel) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/patch - Patch a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchChannel) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/pinned - Get a channel's pinned posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPinnedPosts) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/privacy - Update channel's privacy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelPrivacy) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/restore - Restore a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RestoreChannel) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/scheme - Set a channel's scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelScheme) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/stats - Get channel statistics
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelStats) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/timezones - Get timezones in a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMembersTimezones) | `—`
❌ Not implemented

## GET /api/v4/teams/name/{team_name}/channels/name/{channel_name} - Get a channel by name and team name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelByNameForTeamName) | `IMattermostClient.FindChannelByNameAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/channels - Get public channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPublicChannelsForTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/autocomplete - Autocomplete channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AutocompleteChannelsForTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/deleted - Get deleted channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDeletedChannelsForTeam) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/channels/ids - Get a list of channels by ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPublicChannelsByIdsForTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/managed_categories - Get managed category mappings
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetManagedCategories) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/name/{channel_name} - Get a channel by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelByName) | `IMattermostClient.FindChannelByNameAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/channels/private - Get private channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPrivateChannelsForTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/recommended - Get recommended public channels for the current user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRecommendedChannelsForTeam) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/channels/search - Search channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchChannels) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/search_autocomplete - Autocomplete channels for search
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AutocompleteChannelsForTeamForSearch) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/channels - Get all channels from all teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/channels/{channel_id}/unread - Get unread messages
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelUnread) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels - Get channels for user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsForTeamForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Get user's sidebar categories
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSidebarCategoriesForTeamForUser) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Create user's sidebar category
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateSidebarCategoryForTeamForUser) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Update user's sidebar categories
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateSidebarCategoriesForTeamForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories/order - Get user's sidebar category order
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSidebarCategoryOrderForTeamForUser) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories/order - Update user's sidebar category order
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateSidebarCategoryOrderForTeamForUser) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Delete sidebar category
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveSidebarCategoryForTeamForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Get sidebar category
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSidebarCategoryForTeamForUser) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Update sidebar category
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateSidebarCategoryForTeamForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/members - Get channel memberships and roles for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMembersForUser) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/read - Mark all channels and threads in a team as read
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkAllTeamChannelsRead) | `—`
❌ Not implemented

# Cloud

## GET /api/v4/cloud/check-cws-connection - Check CWS connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CheckCWSConnection) | `—`
❌ Not implemented

## GET /api/v4/cloud/customer - Get cloud customer
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCloudCustomer) | `—`
❌ Not implemented

## PUT /api/v4/cloud/customer - Update cloud customer
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateCloudCustomer) | `—`
❌ Not implemented

## PUT /api/v4/cloud/customer/address - Update cloud customer address
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateCloudCustomerAddress) | `—`
❌ Not implemented

## GET /api/v4/cloud/installation - GET endpoint for Installation information
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEndpointForInstallationInformation) | `—`
❌ Not implemented

## GET /api/v4/cloud/limits - Get cloud workspace limits
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCloudLimits) | `—`
❌ Not implemented

## GET /api/v4/cloud/preview/modal_data - Get cloud preview modal data
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPreviewModalData) | `—`
❌ Not implemented

## GET /api/v4/cloud/products - Get cloud products
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCloudProducts) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription - Get cloud subscription
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSubscription) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription/invoices - Get cloud subscription invoices
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetInvoicesForSubscription) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription/invoices/{invoice_id}/pdf - Get cloud invoice PDF
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetInvoiceForSubscriptionAsPdf) | `—`
❌ Not implemented

## POST /api/v4/cloud/validate-business-email - Validate business email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ValidateBusinessEmail) | `—`
❌ Not implemented

## POST /api/v4/cloud/validate-workspace-business-email - Validate workspace business email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ValidateWorkspaceBusinessEmail) | `—`
❌ Not implemented

## POST /api/v4/cloud/webhook - POST endpoint for CWS Webhooks
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PostEndpointForCwsWebhooks) | `—`
❌ Not implemented

## GET /api/v4/hosted_customer/signup_available - Check hosted signup availability
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/HostedCustomerSignupAvailable) | `—`
❌ Not implemented

# Cluster

## GET /api/v4/cluster/status - Get cluster status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetClusterStatus) | `—`
❌ Not implemented

# Commands

## GET /api/v4/commands - List commands for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListCommands) | `—`
❌ Not implemented

## POST /api/v4/commands - Create a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateCommand) | `—`
❌ Not implemented

## POST /api/v4/commands/execute - Execute a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ExecuteCommand) | `—`
❌ Not implemented

## DELETE /api/v4/commands/{command_id} - Delete a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteCommand) | `—`
❌ Not implemented

## GET /api/v4/commands/{command_id} - Get a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCommandById) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id} - Update a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateCommand) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id}/move - Move a command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MoveCommand) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id}/regen_token - Generate a new token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegenCommandToken) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/commands/autocomplete - List autocomplete commands
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListAutocompleteCommands) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/commands/autocomplete_suggestions - List commands' autocomplete data
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListCommandAutocompleteSuggestions) | `—`
❌ Not implemented

# Compliance

## GET /api/v4/compliance/reports - Get reports
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetComplianceReports) | `—`
❌ Not implemented

## POST /api/v4/compliance/reports - Create report
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateComplianceReport) | `—`
❌ Not implemented

## GET /api/v4/compliance/reports/{report_id} - Get a report
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetComplianceReport) | `—`
❌ Not implemented

## GET /api/v4/compliance/reports/{report_id}/download - Download a report
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DownloadComplianceReport) | `—`
❌ Not implemented

# Conditions

## GET /plugins/playbooks/api/v0/playbooks/{id}/conditions - List playbook conditions
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybookConditions) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/playbooks/{id}/conditions - Create a playbook condition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/createPlaybookCondition) | `—`
❌ Not implemented

## DELETE /plugins/playbooks/api/v0/playbooks/{id}/conditions/{conditionID} - Delete a playbook condition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/deletePlaybookCondition) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/playbooks/{id}/conditions/{conditionID} - Update a playbook condition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/updatePlaybookCondition) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/{id}/conditions - List run conditions
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getRunConditions) | `—`
❌ Not implemented

# Content Flagging

## GET /api/v4/content_flagging/config - Get the system content flagging configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFConfig) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/config - Update the system content flagging configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateCFConfig) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/fields - Get content flagging property fields
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFFields) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/flag/config - Get content flagging configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFFlagConfig) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/post/{post_id} - Get a flagged post with all its content.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFPost) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/assign/{content_reviewer_id} - Assign a content reviewer to a flagged post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PostCFPostReviewer) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/post/{post_id}/field_values - Get content flagging property field values for a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFPostFieldValues) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/flag - Flag a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PostCFPostFlag) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/post/{post_id}/keep - Keep a flagged post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/KeepCFPost) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/post/{post_id}/remove - Remove a flagged post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveCFPost) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/report - Generate and download a flagged post report
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GenerateCFPostReport) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/team/{team_id}/reviewers/search - Search content reviewers in a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchCFTeamReviewers) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/team/{team_id}/status - Get content flagging status for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCFTeamStatus) | `—`
❌ Not implemented

# Custom Profile Attributes

## GET /api/v4/custom_profile_attributes/fields - List all the Custom Profile Attributes fields
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListAllCPAFields) | `—`
❌ Not implemented

## POST /api/v4/custom_profile_attributes/fields - Create a Custom Profile Attribute field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateCPAField) | `—`
❌ Not implemented

## DELETE /api/v4/custom_profile_attributes/fields/{field_id} - Delete a Custom Profile Attribute field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteCPAField) | `—`
❌ Not implemented

## PATCH /api/v4/custom_profile_attributes/fields/{field_id} - Patch a Custom Profile Attribute field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchCPAField) | `—`
❌ Not implemented

## GET /api/v4/custom_profile_attributes/group - Get Custom Profile Attribute property group data
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetCPAGroup) | `—`
❌ Not implemented

## PATCH /api/v4/custom_profile_attributes/values - Patch Custom Profile Attribute values
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchCPAValues) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/custom_profile_attributes - List Custom Profile Attribute values
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListCPAValues) | `—`
❌ Not implemented

## PATCH /api/v4/users/{user_id}/custom_profile_attributes - Update custom profile attribute values for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchCPAValuesForUser) | `—`
❌ Not implemented

# Data Retention

## GET /api/v4/data_retention/policies - Get the granular data retention policies
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDataRetentionPolicies) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies - Create a new granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateDataRetentionPolicy) | `—`
❌ Not implemented

## DELETE /api/v4/data_retention/policies/{policy_id} - Delete a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteDataRetentionPolicy) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id} - Get a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDataRetentionPolicyByID) | `—`
❌ Not implemented

## PATCH /api/v4/data_retention/policies/{policy_id} - Patch a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchDataRetentionPolicy) | `—`
❌ Not implemented

## DELETE /api/v4/data_retention/policies/{policy_id}/channels - Delete channels from a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveChannelsFromRetentionPolicy) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id}/channels - Get the channels for a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsForRetentionPolicy) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/channels - Add channels to a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddChannelsToRetentionPolicy) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/channels/search - Search for the channels in a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchChannelsForRetentionPolicy) | `—`
❌ Not implemented

## DELETE /api/v4/data_retention/policies/{policy_id}/teams - Delete teams from a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveTeamsFromRetentionPolicy) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id}/teams - Get the teams for a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamsForRetentionPolicy) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/teams - Add teams to a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddTeamsToRetentionPolicy) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/teams/search - Search for the teams in a granular data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchTeamsForRetentionPolicy) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies_count - Get the number of granular data retention policies
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDataRetentionPoliciesCount) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policy - Get the global data retention policy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDataRetentionPolicy) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/data_retention/channel_policies - Get the policies which are applied to a user's channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelPoliciesForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/data_retention/team_policies - Get the policies which are applied to a user's teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamPoliciesForUser) | `—`
❌ Not implemented

# Elasticsearch

## POST /api/v4/elasticsearch/purge_indexes - Purge all Elasticsearch indexes
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PurgeElasticsearchIndexes) | `—`
❌ Not implemented

## POST /api/v4/elasticsearch/test - Test Elasticsearch configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestElasticsearch) | `—`
❌ Not implemented

# Emoji

## GET /api/v4/emoji - Get a list of custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEmojiList) | `—`
❌ Not implemented

## POST /api/v4/emoji - Create a custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateEmoji) | `—`
❌ Not implemented

## GET /api/v4/emoji/autocomplete - Autocomplete custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AutocompleteEmoji) | `—`
❌ Not implemented

## GET /api/v4/emoji/name/{emoji_name} - Get a custom emoji by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEmojiByName) | `—`
❌ Not implemented

## POST /api/v4/emoji/names - Get custom emojis by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEmojisByNames) | `—`
❌ Not implemented

## POST /api/v4/emoji/search - Search custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchEmoji) | `—`
❌ Not implemented

## DELETE /api/v4/emoji/{emoji_id} - Delete a custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteEmoji) | `—`
❌ Not implemented

## GET /api/v4/emoji/{emoji_id} - Get a custom emoji
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEmoji) | `—`
❌ Not implemented

## GET /api/v4/emoji/{emoji_id}/image - Get custom emoji image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEmojiImage) | `—`
❌ Not implemented

# Exports

## GET /api/v4/exports - List export files
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListExports) | `—`
❌ Not implemented

## DELETE /api/v4/exports/{export_name} - Delete an export file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteExport) | `—`
❌ Not implemented

## GET /api/v4/exports/{export_name} - Download an export file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DownloadExport) | `—`
❌ Not implemented

## POST /api/v4/exports/{export_name}/presign-url - Create a presigned URL for export download
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PresignExport) | `—`
❌ Not implemented

# Files

## POST /api/v4/files - Upload a file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadFile) | `IMattermostClient.UploadFileAsync`
✅ Implemented

## POST /api/v4/files/search - Search files across the teams of the current user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchFiles) | `—`
❌ Not implemented

## GET /api/v4/files/{file_id} - Get a file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFile) | `IMattermostClient.GetFileAsync`, `IMattermostClient.GetFileStreamAsync`
✅ Implemented

## GET /api/v4/files/{file_id}/info - Get metadata for a file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFileInfo) | `IMattermostClient.GetFileDetailsAsync`
✅ Implemented

## GET /api/v4/files/{file_id}/link - Get a public file link
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFileLink) | `—`
❌ Not implemented

## GET /api/v4/files/{file_id}/preview - Get a file's preview
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFilePreview) | `—`
❌ Not implemented

## GET /api/v4/files/{file_id}/thumbnail - Get a file's thumbnail
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFileThumbnail) | `—`
❌ Not implemented

## GET /files/{file_id}/public - Get a public file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFilePublic) | `—`
❌ Not implemented

# Groups

## GET /api/v4/channels/{channel_id}/groups - Get channel groups
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupsByChannel) | `—`
❌ Not implemented

## GET /api/v4/groups - Get groups
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroups) | `—`
❌ Not implemented

## POST /api/v4/groups - Create a custom group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateGroup) | `—`
❌ Not implemented

## POST /api/v4/groups/names - Get groups by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupsByNames) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id} - Deletes a custom group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteGroup) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id} - Get a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroup) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/channels - Get channel syncables for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupSyncablesChannels) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/channels/{channel_id} - Get a channel syncable for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupSyncableForChannelId) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/channels/{channel_id}/link - Unlink a channel from a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnlinkGroupSyncableForChannel) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/channels/{channel_id}/link - Link a channel to a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LinkGroupSyncableForChannel) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/channels/{channel_id}/patch - Patch a channel syncable for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchGroupSyncableForChannel) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/members - Removes members from a custom group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteGroupMembers) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/members - Get group users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupUsers) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/members - Adds members to a custom group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddGroupMembers) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/patch - Patch a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchGroup) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/restore - Restore a previously deleted group.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RestoreGroup) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/stats - Get group stats
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupStats) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/teams - Get team syncables for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupSyncablesTeams) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/teams/{team_id} - Get a team syncable for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupSyncableForTeamId) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/teams/{team_id}/link - Unlink a team from a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnlinkGroupSyncableForTeam) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/teams/{team_id}/link - Link a team to a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LinkGroupSyncableForTeam) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/teams/{team_id}/patch - Patch a team syncable for a group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchGroupSyncableForTeam) | `—`
❌ Not implemented

## DELETE /api/v4/ldap/groups/{remote_id}/link - Delete a link for LDAP group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnlinkLdapGroup) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/groups - Get team groups
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupsByTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/groups_by_channels - Get team groups by channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupsAssociatedToChannelsByTeam) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/groups - Get groups for a userId
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetGroupsByUserId) | `—`
❌ Not implemented

# Imports

## GET /api/v4/imports - List import files
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListImports) | `—`
❌ Not implemented

## DELETE /api/v4/imports/{import_name} - Delete an import file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteImport) | `—`
❌ Not implemented

# Integration Actions

## POST /api/v4/actions/dialogs/lookup - Lookup dialog elements
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LookupInteractiveDialog) | `—`
❌ Not implemented

## POST /api/v4/actions/dialogs/open - Open a dialog
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/OpenInteractiveDialog) | `—`
❌ Not implemented

## POST /api/v4/actions/dialogs/submit - Submit a dialog
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SubmitInteractiveDialog) | `—`
❌ Not implemented

# Internal

## GET /plugins/playbooks/api/v0/runs/checklist-autocomplete - Get autocomplete data for /playbook check
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getChecklistAutocomplete) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/dialog - Create a new playbook run from dialog
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/createPlaybookRunFromDialog) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/{id}/end - End a playbook run from dialog
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/endPlaybookRunDialog) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/{id}/next-stage-dialog - Go to next stage from dialog
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/nextStageDialog) | `—`
❌ Not implemented

# Ip

## GET /api/v4/ip_filtering - Get all IP filters
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetIPFilters) | `—`
❌ Not implemented

## POST /api/v4/ip_filtering - Get all IP filters
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ApplyIPFilters) | `—`
❌ Not implemented

## GET /api/v4/ip_filtering/my_ip - Get all IP filters
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MyIP) | `—`
❌ Not implemented

# Jobs

## GET /api/v4/jobs - Get the jobs.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetJobs) | `—`
❌ Not implemented

## POST /api/v4/jobs - Create a new job.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateJob) | `—`
❌ Not implemented

## GET /api/v4/jobs/type/{job_type} - Get the jobs of the given type.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetJobsByType) | `—`
❌ Not implemented

## GET /api/v4/jobs/{job_id} - Get a job.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetJob) | `—`
❌ Not implemented

## POST /api/v4/jobs/{job_id}/cancel - Cancel a job.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CancelJob) | `—`
❌ Not implemented

## GET /api/v4/jobs/{job_id}/download - Download the results of a job.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DownloadJob) | `—`
❌ Not implemented

## PATCH /api/v4/jobs/{job_id}/status - Update the status of a job
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateJobStatus) | `—`
❌ Not implemented

# LDAP

## DELETE /api/v4/ldap/certificate/private - Remove private key
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteLdapPrivateCertificate) | `—`
❌ Not implemented

## POST /api/v4/ldap/certificate/private - Upload private key
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadLdapPrivateCertificate) | `—`
❌ Not implemented

## DELETE /api/v4/ldap/certificate/public - Remove public certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteLdapPublicCertificate) | `—`
❌ Not implemented

## POST /api/v4/ldap/certificate/public - Upload public certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadLdapPublicCertificate) | `—`
❌ Not implemented

## GET /api/v4/ldap/groups - Returns a list of LDAP groups
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLdapGroups) | `—`
❌ Not implemented

## POST /api/v4/ldap/groups/{remote_id}/link - Link a LDAP group
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LinkLdapGroup) | `—`
❌ Not implemented

## POST /api/v4/ldap/migrateid - Migrate Id LDAP
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MigrateIdLdap) | `—`
❌ Not implemented

## POST /api/v4/ldap/sync - Sync with LDAP
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SyncLdap) | `—`
❌ Not implemented

## POST /api/v4/ldap/test - Test LDAP configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestLdap) | `—`
❌ Not implemented

## POST /api/v4/ldap/test_connection - Test LDAP connection with specific settings
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestLdapConnection) | `—`
❌ Not implemented

## POST /api/v4/ldap/test_diagnostics - Test LDAP diagnostics with specific settings
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestLdapDiagnostics) | `—`
❌ Not implemented

## POST /api/v4/ldap/users/{user_id}/group_sync_memberships - Create memberships for LDAP configured channels and teams for this user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddUserToGroupSyncables) | `—`
❌ Not implemented

# Logs

## GET /api/v4/logs/download - Download system logs
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DownloadSystemLogs) | `—`
❌ Not implemented

# Metrics

## POST /api/v4/client_perf - Report client performance metrics
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SubmitPerformanceReport) | `—`
❌ Not implemented

# OAuth

## GET /.well-known/oauth-authorization-server - Get OAuth 2.0 Authorization Server Metadata
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAuthorizationServerMetadata) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps - Get OAuth apps
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOAuthApps) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps - Register OAuth app
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateOAuthApp) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps/register - Register OAuth client using Dynamic Client Registration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegisterOAuthClient) | `—`
❌ Not implemented

## DELETE /api/v4/oauth/apps/{app_id} - Delete an OAuth app
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteOAuthApp) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps/{app_id} - Get an OAuth app
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOAuthApp) | `—`
❌ Not implemented

## PUT /api/v4/oauth/apps/{app_id} - Update an OAuth app
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateOAuthApp) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps/{app_id}/info - Get info on an OAuth app
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOAuthAppInfo) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps/{app_id}/regen_secret - Regenerate OAuth app secret
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegenerateOAuthAppSecret) | `—`
❌ Not implemented

# Oauth

## GET /api/v4/oauth/outgoing_connections - List all connections
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListOutgoingOAuthConnections) | `—`
❌ Not implemented

## POST /api/v4/oauth/outgoing_connections - Create a connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateOutgoingOAuthConnection) | `—`
❌ Not implemented

## POST /api/v4/oauth/outgoing_connections/validate - Validate a connection configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ValidateOutgoingOAuthConnection) | `—`
❌ Not implemented

## DELETE /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Delete a connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteOutgoingOAuthConnection) | `—`
❌ Not implemented

## GET /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Get a connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOutgoingOAuthConnection) | `—`
❌ Not implemented

## PUT /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Update a connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateOutgoingOAuthConnection) | `—`
❌ Not implemented

# OAuth

## GET /api/v4/users/{user_id}/oauth/apps/authorized - Get authorized OAuth apps
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAuthorizedOAuthAppsForUser) | `—`
❌ Not implemented

# Permissions

## POST /api/v4/permissions/ancillary - Return all system console subsection ancillary permissions
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAncillaryPermissionsPost) | `—`
❌ Not implemented

# PlaybookAutofollows

## GET /plugins/playbooks/api/v0/playbooks/{id}/autofollows - Get the list of followers' user IDs of a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getAutoFollows) | `—`
❌ Not implemented

# PlaybookRuns

## GET /plugins/playbooks/api/v0/runs - List all playbook runs
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/listPlaybookRuns) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs - Create a new playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/createPlaybookRunFromPost) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/channel/{channel_id} - Find playbook run by channel ID
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybookRunByChannelId) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/channels - Get playbook run channels
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getChannels) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/owners - Get all owners
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getOwners) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/{id} - Get a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybookRun) | `—`
❌ Not implemented

## PATCH /plugins/playbooks/api/v0/runs/{id} - Update a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/updatePlaybookRun) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/add - Add an item to a playbook run's checklist
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/addChecklistItem) | `—`
❌ Not implemented

## DELETE /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/item/{item} - Delete an item of a playbook run's checklist
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/itemDelete) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/item/{item} - Update an item of a playbook run's checklist
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/itemRename) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/item/{item}/assignee - Update the assignee of an item
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/itemSetAssignee) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/item/{item}/run - Run an item's slash command
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/itemRun) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/item/{item}/state - Update the state of an item
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/itemSetState) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/checklists/{checklist}/reorder - Reorder an item in a playbook run's checklist
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/reoderChecklistItem) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/end - End a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/endPlaybookRun) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/finish - Finish a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/finish) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/{id}/metadata - Get playbook run metadata
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybookRunMetadata) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/{id}/owner - Update playbook run owner
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/changeOwner) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/{id}/property_fields - Get property fields for a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getRunPropertyFields) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/property_fields/{field_id}/value - Set a property value for a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/setRunPropertyValue) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/runs/{id}/property_values - Get property values for a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getRunPropertyValues) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/runs/{id}/restart - Restart a playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/restartPlaybookRun) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/runs/{id}/status - Update a playbook run's status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/status) | `—`
❌ Not implemented

# Playbooks

## GET /plugins/playbooks/api/v0/playbooks - List all playbooks
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybooks) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/playbooks - Create a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/createPlaybook) | `—`
❌ Not implemented

## DELETE /plugins/playbooks/api/v0/playbooks/{id} - Delete a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/deletePlaybook) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/playbooks/{id} - Get a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybook) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/playbooks/{id} - Update a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/updatePlaybook) | `—`
❌ Not implemented

## GET /plugins/playbooks/api/v0/playbooks/{id}/property_fields - Get property fields for a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPlaybookPropertyFields) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/playbooks/{id}/property_fields - Create a property field for a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/createPlaybookPropertyField) | `—`
❌ Not implemented

## POST /plugins/playbooks/api/v0/playbooks/{id}/property_fields/reorder - Reorder property fields for a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/reorderPlaybookPropertyFields) | `—`
❌ Not implemented

## DELETE /plugins/playbooks/api/v0/playbooks/{id}/property_fields/{field_id} - Delete a property field for a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/deletePlaybookPropertyField) | `—`
❌ Not implemented

## PUT /plugins/playbooks/api/v0/playbooks/{id}/property_fields/{field_id} - Update a property field for a playbook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/updatePlaybookPropertyField) | `—`
❌ Not implemented

# Plugins

## GET /api/v4/plugins - Get plugins
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPlugins) | `—`
❌ Not implemented

## POST /api/v4/plugins - Upload plugin
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadPlugin) | `—`
❌ Not implemented

## POST /api/v4/plugins/install_from_url - Install plugin from url
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InstallPluginFromUrl) | `—`
❌ Not implemented

## GET /api/v4/plugins/marketplace - Gets all the marketplace plugins
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetMarketplacePlugins) | `—`
❌ Not implemented

## POST /api/v4/plugins/marketplace - Installs a marketplace plugin
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InstallMarketplacePlugin) | `—`
❌ Not implemented

## GET /api/v4/plugins/marketplace/first_admin_visit - Get if the Plugin Marketplace has been visited by at least an admin.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetMarketplaceVisitedByAdmin) | `—`
❌ Not implemented

## POST /api/v4/plugins/reattach - Reattach a plugin process
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ReattachPlugin) | `—`
❌ Not implemented

## GET /api/v4/plugins/statuses - Get plugins status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPluginStatuses) | `—`
❌ Not implemented

## GET /api/v4/plugins/webapp - Get webapp plugins
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetWebappPlugins) | `—`
❌ Not implemented

## DELETE /api/v4/plugins/{plugin_id} - Remove plugin
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemovePlugin) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/detach - Detach a reattached plugin process
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DetachPlugin) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/disable - Disable plugin
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DisablePlugin) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/enable - Enable plugin
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/EnablePlugin) | `—`
❌ Not implemented

# Posts

## GET /api/v4/channels/{channel_id}/posts - Get posts for a channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostsForChannel) | `IMattermostClient.GetChannelPostsAsync`
✅ Implemented

## POST /api/v4/posts - Create a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreatePost) | `IMattermostClient.CreatePostAsync`, `IMattermostClient.CreatePostWithRawPropsAsync`
✅ Implemented

## POST /api/v4/posts/ephemeral - Create a ephemeral post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreatePostEphemeral) | `—`
❌ Not implemented

## POST /api/v4/posts/ids - Get posts by a list of ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/getPostsByIds) | `—`
❌ Not implemented

## POST /api/v4/posts/rewrite - Rewrite a message using AI
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RewriteMessage) | `—`
❌ Not implemented

## POST /api/v4/posts/search - Search posts across all teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchPostsInAllTeams) | `—`
❌ Not implemented

## DELETE /api/v4/posts/{post_id} - Delete a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeletePost) | `IMattermostClient.DeletePostAsync`
✅ Implemented

## GET /api/v4/posts/{post_id} - Get a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPost) | `IMattermostClient.GetPostAsync`
✅ Implemented

## PUT /api/v4/posts/{post_id} - Update a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdatePost) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/actions/{action_id} - Perform a post action
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DoPostAction) | `—`
❌ Not implemented

## DELETE /api/v4/posts/{post_id}/burn - Burn a burn-on-read post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/BurnPost) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/edit_history - Get post edit history
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEditHistoryForPost) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/files/info - Get file info for post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFileInfosForPost) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/info - Get post info
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostInfo) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/move - Move a post (and any posts within that post's thread)
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MoveThread) | `—`
❌ Not implemented

## PUT /api/v4/posts/{post_id}/patch - Patch a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchPost) | `IMattermostClient.UpdatePostAsync`, `IMattermostClient.UpdatePostWithRawPropsAsync`
✅ Implemented

## POST /api/v4/posts/{post_id}/pin - Pin a post to the channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PinPost) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/restore/{restore_version_id} - Restores a past version of a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RestorePostVersion) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/reveal - Reveal a burn-on-read post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevealPost) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/thread - Get a thread
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostThread) | `IMattermostClient.GetThreadPostsAsync`
✅ Implemented

## POST /api/v4/posts/{post_id}/unpin - Unpin a post to the channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnpinPost) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/posts/search - Search for team posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchPosts) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/channels/{channel_id}/posts/unread - Get posts around oldest unread
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostsAroundLastUnread) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/posts/flagged - Get a list of flagged posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetFlaggedPostsForUser) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/posts/{post_id}/ack - Delete a post acknowledgement
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteAcknowledgementForPost) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/ack - Acknowledge a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SaveAcknowledgementForPost) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/reminder - Set a post reminder
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetPostReminder) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/set_unread - Mark as unread from a post.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetPostUnread) | `—`
❌ Not implemented

# Preferences

## GET /api/v4/users/{user_id}/preferences - Get the user's preferences
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPreferences) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/preferences - Save the user's preferences
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdatePreferences) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/preferences/delete - Delete user's preferences
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeletePreferences) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/preferences/{category} - List a user's preferences by category
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPreferencesByCategory) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/preferences/{category}/name/{preference_name} - Get a specific user preference
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPreferencesByCategoryByName) | `—`
❌ Not implemented

# Properties

## POST /api/v4/properties/groups/{group_name}/fields/search - Search property fields across multiple object types
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchPropertyFields) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/system/values - Get property values for the system
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSystemPropertyValues) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/system/values - Update property values for the system
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateSystemPropertyValues) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/{object_type}/fields - Get property fields
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPropertyFields) | `—`
❌ Not implemented

## POST /api/v4/properties/groups/{group_name}/{object_type}/fields - Create a property field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreatePropertyField) | `—`
❌ Not implemented

## DELETE /api/v4/properties/groups/{group_name}/{object_type}/fields/{field_id} - Delete a property field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeletePropertyField) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/{object_type}/fields/{field_id} - Update a property field
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdatePropertyField) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/{object_type}/values/{target_id} - Get property values for a target
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPropertyValues) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/{object_type}/values/{target_id} - Update property values for a target
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdatePropertyValues) | `—`
❌ Not implemented

# Reactions

## POST /api/v4/posts/ids/reactions - Bulk get the reaction for posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetBulkReactions) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/reactions - Get a list of reactions to a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetReactions) | `—`
❌ Not implemented

## POST /api/v4/reactions - Create a reaction
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SaveReaction) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/posts/{post_id}/reactions/{emoji_name} - Remove a reaction from a post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteReaction) | `—`
❌ Not implemented

# Recaps

## GET /api/v4/recaps - Get current user's recaps
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRecapsForUser) | `—`
❌ Not implemented

## POST /api/v4/recaps - Create a channel recap
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateRecap) | `—`
❌ Not implemented

## POST /api/v4/recaps/mark_viewed - Mark all of the authenticated user's finished recaps as viewed
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkRecapsAsViewed) | `—`
❌ Not implemented

## DELETE /api/v4/recaps/{recap_id} - Delete a recap
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteRecap) | `—`
❌ Not implemented

## GET /api/v4/recaps/{recap_id} - Get a specific recap
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRecap) | `—`
❌ Not implemented

## POST /api/v4/recaps/{recap_id}/read - Mark a recap as read
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkRecapAsRead) | `—`
❌ Not implemented

## POST /api/v4/recaps/{recap_id}/regenerate - Regenerate a recap
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegenerateRecap) | `—`
❌ Not implemented

# Remote Clusters

## GET /api/v4/remotecluster - Get a list of remote clusters.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRemoteClusters) | `—`
❌ Not implemented

## POST /api/v4/remotecluster - Create a new remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateRemoteCluster) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/accept_invite - Accept a remote cluster invite code.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AcceptRemoteClusterInvite) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/confirm_invite - Confirm an invite with a remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoteClusterConfirmInvite) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/msg - Receive a remote cluster message.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoteClusterAcceptMessage) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/ping - Receive a ping from a remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoteClusterPing) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/upload/{upload_id} - Upload file data for a remote upload session.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadRemoteClusterData) | `—`
❌ Not implemented

## DELETE /api/v4/remotecluster/{remote_id} - Delete a remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteRemoteCluster) | `—`
❌ Not implemented

## GET /api/v4/remotecluster/{remote_id} - Get a remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRemoteCluster) | `—`
❌ Not implemented

## PATCH /api/v4/remotecluster/{remote_id} - Patch a remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchRemoteCluster) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{remote_id}/generate_invite - Generate invite code.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GenerateRemoteClusterInvite) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{user_id}/image - Set profile image for a remote user.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoteSetProfileImage) | `—`
❌ Not implemented

# Reports

## POST /api/v4/reports/posts - Get posts for reporting and compliance purposes using cursor-based pagination
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostsForReporting) | `—`
❌ Not implemented

## GET /api/v4/reports/users - Get a list of paged and sorted users for admin reporting purposes
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersForReporting) | `—`
❌ Not implemented

## GET /api/v4/reports/users/count - Gets the full count of users that match the filter.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserCountForReporting) | `—`
❌ Not implemented

## POST /api/v4/reports/users/export - Starts a job to export the users to a report file.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/StartBatchUsersExport) | `—`
❌ Not implemented

# Roles

## GET /api/v4/roles - Get a list of all the roles
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAllRoles) | `—`
❌ Not implemented

## GET /api/v4/roles/name/{role_name} - Get a role
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRoleByName) | `—`
❌ Not implemented

## POST /api/v4/roles/names - Get a list of roles by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRolesByNames) | `—`
❌ Not implemented

## GET /api/v4/roles/{role_id} - Get a role
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRole) | `—`
❌ Not implemented

## PUT /api/v4/roles/{role_id}/patch - Patch a role
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchRole) | `—`
❌ Not implemented

# Root

## POST /api/v4/notifications/ack - Acknowledge receiving of a notification
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AcknowledgeNotification) | `—`
❌ Not implemented

# SAML

## DELETE /api/v4/saml/certificate/idp - Remove IDP certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteSamlIdpCertificate) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/idp - Upload IDP certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadSamlIdpCertificate) | `—`
❌ Not implemented

## DELETE /api/v4/saml/certificate/private - Remove private key
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteSamlPrivateCertificate) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/private - Upload private key
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadSamlPrivateCertificate) | `—`
❌ Not implemented

## DELETE /api/v4/saml/certificate/public - Remove public certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteSamlPublicCertificate) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/public - Upload public certificate
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadSamlPublicCertificate) | `—`
❌ Not implemented

## GET /api/v4/saml/certificate/status - Get certificate status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSamlCertificateStatus) | `—`
❌ Not implemented

## GET /api/v4/saml/metadata - Get metadata
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSamlMetadata) | `—`
❌ Not implemented

## POST /api/v4/saml/metadatafromidp - Get metadata from Identity Provider
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSamlMetadataFromIdp) | `—`
❌ Not implemented

## POST /api/v4/saml/reset_auth_data - Reset AuthData to Email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ResetSamlAuthDataToEmail) | `—`
❌ Not implemented

# Scheduled Post

## POST /api/v4/posts/schedule - Creates a scheduled post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateScheduledPost) | `—`
❌ Not implemented

## DELETE /api/v4/posts/schedule/{scheduled_post_id} - Delete a scheduled post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteScheduledPost) | `—`
❌ Not implemented

## PUT /api/v4/posts/schedule/{scheduled_post_id} - Update a scheduled post
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateScheduledPost) | `—`
❌ Not implemented

## GET /api/v4/posts/scheduled/team/{team_id} - Gets all scheduled posts for a user for the specified team..
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserScheduledPosts) | `—`
❌ Not implemented

# Schemes

## GET /api/v4/schemes - Get the schemes.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSchemes) | `—`
❌ Not implemented

## POST /api/v4/schemes - Create a scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateScheme) | `—`
❌ Not implemented

## DELETE /api/v4/schemes/{scheme_id} - Delete a scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteScheme) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id} - Get a scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetScheme) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id}/channels - Get a page of channels which use this scheme.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelsForScheme) | `—`
❌ Not implemented

## PUT /api/v4/schemes/{scheme_id}/patch - Patch a scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchScheme) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id}/teams - Get a page of teams which use this scheme.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamsForScheme) | `—`
❌ Not implemented

# Shared Channels

## POST /api/v4/remotecluster/{remote_id}/channels/{channel_id}/invite - Invites a remote cluster to a channel.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InviteRemoteClusterToChannel) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{remote_id}/channels/{channel_id}/uninvite - Uninvites a remote cluster to a channel.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UninviteRemoteClusterToChannel) | `—`
❌ Not implemented

## GET /api/v4/remotecluster/{remote_id}/sharedchannelremotes - Get shared channel remotes by remote cluster.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSharedChannelRemotesByRemoteCluster) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/remote_info/{remote_id} - Get remote cluster info by ID for user.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRemoteClusterInfo) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/users/{user_id}/can_dm/{other_user_id} - Check if user can DM another user in shared channels context
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CanUserDirectMessage) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/{channel_id}/remotes - Get remote clusters for a shared channel
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSharedChannelRemotes) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/{team_id} - Get all shared channels for team.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAllSharedChannels) | `—`
❌ Not implemented

# Status

## POST /api/v4/users/status/ids - Get user statuses by id
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersStatusesByIds) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/status - Get user status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserStatus) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/status - Update user status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserStatus) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/status/custom - Unsets user custom status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UnsetUserCustomStatus) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/status/custom - Update user custom status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserCustomStatus) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/status/custom/recent - Delete user's recent custom status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveRecentCustomStatus) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/status/custom/recent/delete - Delete user's recent custom status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PostUserRecentCustomStatusDelete) | `—`
❌ Not implemented

# System

## GET /api/v4/analytics/old - Get analytics
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAnalyticsOld) | `—`
❌ Not implemented

## GET /api/v4/audits - Get audits
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAudits) | `—`
❌ Not implemented

## POST /api/v4/caches/invalidate - Invalidate all the caches
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InvalidateCaches) | `—`
❌ Not implemented

## GET /api/v4/config - Get configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetConfig) | `—`
❌ Not implemented

## PUT /api/v4/config - Update configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateConfig) | `—`
❌ Not implemented

## GET /api/v4/config/client - Get client configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetClientConfig) | `—`
❌ Not implemented

## GET /api/v4/config/environment - Get configuration made through environment variables
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetEnvironmentConfig) | `—`
❌ Not implemented

## POST /api/v4/config/migrate - Migrate config storage
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MigrateConfig) | `—`
❌ Not implemented

## PUT /api/v4/config/patch - Patch configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchConfig) | `—`
❌ Not implemented

## POST /api/v4/config/reload - Reload configuration
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ReloadConfig) | `—`
❌ Not implemented

## POST /api/v4/database/recycle - Recycle database connections
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DatabaseRecycle) | `—`
❌ Not implemented

## POST /api/v4/email/test - Send a test email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestEmail) | `—`
❌ Not implemented

## POST /api/v4/file/s3_test - Test AWS S3 connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestS3Connection) | `—`
❌ Not implemented

## POST /api/v4/file/test - Test the configured file storage backend
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestFileStoreConnection) | `—`
❌ Not implemented

## GET /api/v4/image - Get an image by url
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetImageByUrl) | `—`
❌ Not implemented

## POST /api/v4/integrity - Perform a database integrity check
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CheckIntegrity) | `—`
❌ Not implemented

## GET /api/v4/latest_version - Get latest public server release information
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLatestVersion) | `—`
❌ Not implemented

## DELETE /api/v4/license - Remove license file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveLicenseFile) | `—`
❌ Not implemented

## POST /api/v4/license - Upload license file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadLicenseFile) | `—`
❌ Not implemented

## GET /api/v4/license/client - Get client license
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetClientLicense) | `—`
❌ Not implemented

## GET /api/v4/license/load_metric - Get license load metric
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLicenseLoadMetric) | `—`
❌ Not implemented

## POST /api/v4/license/preview - Preview license file
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PreviewLicenseFile) | `—`
❌ Not implemented

## GET /api/v4/logs - Get logs
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLogs) | `—`
❌ Not implemented

## POST /api/v4/logs - Add log message
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PostLog) | `—`
❌ Not implemented

## POST /api/v4/logs/query - Query server logs with filters
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/QueryLogs) | `—`
❌ Not implemented

## POST /api/v4/notifications/test - Send a test notification
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestNotification) | `—`
❌ Not implemented

## POST /api/v4/plugins/marketplace/first_admin_visit - Stores that the Plugin Marketplace has been visited by at least an admin.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateMarketplaceVisitedByAdmin) | `—`
❌ Not implemented

## GET /api/v4/redirect_location - Get redirect location
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetRedirectLocation) | `—`
❌ Not implemented

## POST /api/v4/restart - Restart the system after an upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RestartServer) | `—`
❌ Not implemented

## DELETE /api/v4/server_busy - Clears the server busy (high load) flag
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ClearServerBusy) | `—`
❌ Not implemented

## GET /api/v4/server_busy - Get server busy expiry time.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetServerBusyExpires) | `—`
❌ Not implemented

## POST /api/v4/server_busy - Set the server busy (high load) flag
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetServerBusy) | `—`
❌ Not implemented

## POST /api/v4/site_url/test - Checks the validity of a Site URL
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TestSiteURL) | `—`
❌ Not implemented

## DELETE /api/v4/system/e2e/ai_bridge - Reset AI bridge E2E test helper
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteAIBridgeTestHelper) | `—`
❌ Not implemented

## GET /api/v4/system/e2e/ai_bridge - Get AI bridge E2E test helper state
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAIBridgeTestHelper) | `—`
❌ Not implemented

## PUT /api/v4/system/e2e/ai_bridge - Configure AI bridge E2E test helper
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetAIBridgeTestHelper) | `—`
❌ Not implemented

## PUT /api/v4/system/notices/view - Update notices as 'viewed'
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MarkNoticesViewed) | `—`
❌ Not implemented

## GET /api/v4/system/notices/{team_id} - Get notices for logged in user in specified team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetNotices) | `—`
❌ Not implemented

## GET /api/v4/system/onboarding/complete - Get first admin onboarding completion status
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOnboardingComplete) | `—`
❌ Not implemented

## POST /api/v4/system/onboarding/complete - Complete first admin onboarding
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CompleteOnboarding) | `—`
❌ Not implemented

## GET /api/v4/system/ping - Check system health
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPing) | `—`
❌ Not implemented

## GET /api/v4/system/schema/version - Get applied database schema migrations
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAppliedSchemaMigrations) | `—`
❌ Not implemented

## GET /api/v4/system/support_packet - Download a zip file which contains helpful and useful information for troubleshooting your mattermost instance.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GenerateSupportPacket) | `—`
❌ Not implemented

## GET /api/v4/system/timezones - Retrieve a list of supported timezones
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSupportedTimezone) | `—`
❌ Not implemented

## POST /api/v4/trial-license - Request and install a trial license for your server
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RequestTrialLicense) | `—`
❌ Not implemented

## GET /api/v4/trial-license/prev - Get last trial license used
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPrevTrialLicense) | `—`
❌ Not implemented

## POST /api/v4/upgrade_to_enterprise - Executes an inplace upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpgradeToEnterprise) | `—`
❌ Not implemented

## GET /api/v4/upgrade_to_enterprise/allowed - Check if the user is allowed to upgrade to Enterprise Edition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/IsAllowedToUpgradeToEnterprise) | `—`
❌ Not implemented

## GET /api/v4/upgrade_to_enterprise/status - Get the current status for the inplace upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpgradeToEnterpriseStatus) | `—`
❌ Not implemented

## GET /api/v4/websocket - Open a WebSocket connection
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ConnectWebSocket) | `IMattermostClient.StartReceivingAsync`
✅ Implemented

## GET /manualtest - Run manual testing helpers
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ManualTest) | `—`
❌ Not implemented

# Teams

## GET /api/v4/teams - Get teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetAllTeams) | `—`
❌ Not implemented

## POST /api/v4/teams - Create a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/invite/{invite_id} - Get invite info for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamInviteInfo) | `—`
❌ Not implemented

## DELETE /api/v4/teams/invites/email - Invalidate active email invitations
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InvalidateEmailInvites) | `—`
❌ Not implemented

## POST /api/v4/teams/members/invite - Add user to team from invite
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddTeamMemberFromInvite) | `—`
❌ Not implemented

## GET /api/v4/teams/name/{team_name} - Get a team by name
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamByName) | `—`
❌ Not implemented

## GET /api/v4/teams/name/{team_name}/exists - Check if team exists
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TeamExists) | `—`
❌ Not implemented

## POST /api/v4/teams/search - Search teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchTeams) | `—`
❌ Not implemented

## DELETE /api/v4/teams/{team_id} - Delete a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SoftDeleteTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id} - Get a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeam) | `IMattermostClient.GetTeamAsync`
✅ Implemented

## PUT /api/v4/teams/{team_id} - Update a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/access_control/policy - Get the access control policy for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamAccessControlPolicy) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/files/search - Search files in a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchFiles) | `—`
❌ Not implemented

## DELETE /api/v4/teams/{team_id}/image - Remove the team icon
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveTeamIcon) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/image - Get the team icon
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamIcon) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/image - Sets the team icon
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetTeamIcon) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/import - Import a Team from other application
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ImportTeam) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/invite-guests/email - Invite guests to the team by email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InviteGuestsToTeam) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/invite/email - Invite users to the team by email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/InviteUsersToTeam) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/members - Get team members
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamMembers) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members - Add user to team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddTeamMember) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members/batch - Add multiple users to team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AddTeamMembers) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members/ids - Get team members by ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamMembersByIds) | `—`
❌ Not implemented

## DELETE /api/v4/teams/{team_id}/members/{user_id} - Remove user from team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RemoveTeamMember) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/members/{user_id} - Get a team member
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamMember) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/members/{user_id}/roles - Update a team member roles
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateTeamMemberRoles) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/members/{user_id}/schemeRoles - Update the scheme-derived roles of a team member.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateTeamMemberSchemeRoles) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/members_minus_group_members - Team members minus group members.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TeamMembersMinusGroupMembers) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/patch - Patch a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchTeam) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/privacy - Update teams's privacy
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateTeamPrivacy) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/regenerate_invite_id - Regenerate the Invite ID from a Team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegenerateTeamInviteId) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/restore - Restore a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RestoreTeam) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/scheme - Set a team's scheme
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateTeamScheme) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/stats - Get a team stats
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamStats) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams - Get a user's teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamsForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/members - Get team members for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamMembersForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/unread - Get team unreads for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamsUnreadForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/unread - Get unreads for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamUnread) | `—`
❌ Not implemented

# Terms Of Service

## GET /api/v4/terms_of_service - Get latest terms of service
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTermsOfService) | `—`
❌ Not implemented

## POST /api/v4/terms_of_service - Creates a new terms of service
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateTermsOfService) | `—`
❌ Not implemented

# Threads

## GET /api/v4/users/{user_id}/teams/{team_id}/threads - Get all threads that user is following
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserThreads) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/read - Mark all threads that user is following as read
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateThreadsReadForUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id} - Get a thread followed by the user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserThread) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/following - Stop following a thread
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/StopFollowingThread) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/following - Start following a thread
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/StartFollowingThread) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/read/{timestamp} - Mark a thread that user is following read state to the timestamp
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateThreadReadForUser) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/set_unread/{post_id} - Mark a thread that user is following as unread based on a post id
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetThreadUnreadByPostId) | `—`
❌ Not implemented

# Timeline

## DELETE /plugins/playbooks/api/v0/runs/{id}/timeline/{event_id} - Remove a timeline event from the playbook run
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/removeTimelineEvent) | `—`
❌ Not implemented

# Uploads

## POST /api/v4/uploads - Create an upload
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateUpload) | `—`
❌ Not implemented

## GET /api/v4/uploads/{upload_id} - Get an upload session
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUpload) | `—`
❌ Not implemented

## POST /api/v4/uploads/{upload_id} - Perform a file upload
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UploadData) | `—`
❌ Not implemented

# Usage

## GET /api/v4/usage/posts - Get current usage of posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostsUsage) | `—`
❌ Not implemented

## GET /api/v4/usage/storage - Get the total file storage usage for the instance in bytes.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetStorageUsage) | `—`
❌ Not implemented

## GET /api/v4/usage/teams - Get current usage of teams
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTeamsUsage) | `—`
❌ Not implemented

# Users

## POST /api/v4/drafts - Upsert synced draft
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpsertDraft) | `—`
❌ Not implemented

## GET /api/v4/limits/server - Gets the server limits for the server
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetServerLimits) | `—`
❌ Not implemented

## DELETE /api/v4/users - Permanent delete all users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PermanentDeleteAllUsers) | `—`
❌ Not implemented

## GET /api/v4/users - Get users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsers) | `—`
❌ Not implemented

## POST /api/v4/users - Create a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateUser) | `—`
❌ Not implemented

## GET /api/v4/users/auth_data - Get a user by auth data
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserByAuthData) | `—`
❌ Not implemented

## GET /api/v4/users/autocomplete - Autocomplete users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AutocompleteUsers) | `—`
❌ Not implemented

## POST /api/v4/users/email/verify - Verify user email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/VerifyUserEmail) | `—`
❌ Not implemented

## POST /api/v4/users/email/verify/send - Send verification email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SendVerificationEmail) | `—`
❌ Not implemented

## GET /api/v4/users/email/{email} - Get a user by email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserByEmail) | `IMattermostClient.GetUserByEmailAsync`
✅ Implemented

## POST /api/v4/users/group_channels - Get users by group channels ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersByGroupChannelIds) | `—`
❌ Not implemented

## POST /api/v4/users/ids - Get users by ids
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersByIds) | `—`
❌ Not implemented

## GET /api/v4/users/invalid_emails - Get users with invalid emails
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersWithInvalidEmails) | `—`
❌ Not implemented

## GET /api/v4/users/known - Get user IDs of known users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetKnownUsers) | `—`
❌ Not implemented

## POST /api/v4/users/login - Login to Mattermost server
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/Login) | `IMattermostClient.LoginAsync`
✅ Implemented

## POST /api/v4/users/login/cws - Auto-Login to Mattermost server using CWS token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LoginByCwsToken) | `—`
❌ Not implemented

## POST /api/v4/users/login/desktop_token - Login using desktop token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LoginWithDesktopToken) | `—`
❌ Not implemented

## POST /api/v4/users/login/sso/code-exchange - Exchange SSO login code for session tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LoginSSOCodeExchange) | `—`
❌ Not implemented

## POST /api/v4/users/login/switch - Switch login method
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SwitchAccountType) | `—`
❌ Not implemented

## POST /api/v4/users/login/type - Get login authentication type
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetLoginType) | `—`
❌ Not implemented

## POST /api/v4/users/logout - Logout from the Mattermost server
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/Logout) | `IMattermostClient.LogoutAsync`
✅ Implemented

## GET /api/v4/users/me - Get current user information
[Not listed in current Mattermost OpenAPI specification](https://developers.mattermost.com/mattermost-openapi-v4.yaml) | `IMattermostClient.GetMeAsync`
✅ Implemented

## POST /api/v4/users/migrate_auth/ldap - Migrate user accounts authentication type to LDAP.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MigrateAuthToLdap) | `—`
❌ Not implemented

## POST /api/v4/users/migrate_auth/saml - Migrate user accounts authentication type to SAML.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/MigrateAuthToSaml) | `—`
❌ Not implemented

## POST /api/v4/users/notify-admin - Save notify-admin intent
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/NotifyAdmin) | `—`
❌ Not implemented

## POST /api/v4/users/password/reset - Reset password
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ResetPassword) | `—`
❌ Not implemented

## POST /api/v4/users/password/reset/send - Send password reset email
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SendPasswordResetEmail) | `—`
❌ Not implemented

## POST /api/v4/users/search - Search users
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchUsers) | `—`
❌ Not implemented

## GET /api/v4/users/sessions/attributes/manifest - Get the session attributes manifest
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSessionAttributesManifest) | `—`
❌ Not implemented

## PUT /api/v4/users/sessions/device - Attach mobile device and extra props to the session object
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/AttachDeviceExtraProps) | `—`
❌ Not implemented

## POST /api/v4/users/sessions/revoke/all - Revoke all sessions from all users.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevokeSessionsFromAllUsers) | `—`
❌ Not implemented

## GET /api/v4/users/stats - Get total count of users in the system
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTotalUsersStats) | `—`
❌ Not implemented

## GET /api/v4/users/stats/filtered - Get total count of users in the system matching the specified filters
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetTotalUsersStatsFiltered) | `—`
❌ Not implemented

## GET /api/v4/users/tokens - Get user access tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserAccessTokens) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/disable - Disable personal access token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DisableUserAccessToken) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/enable - Enable personal access token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/EnableUserAccessToken) | `—`
❌ Not implemented

## GET /api/v4/users/tokens/non_compliant/count - Count non-compliant personal access tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetNonCompliantUserAccessTokenCount) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/non_compliant/revoke - Revoke non-compliant personal access tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevokeNonCompliantUserAccessTokens) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/revoke - Revoke a user access token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevokeUserAccessToken) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/search - Search tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SearchUserAccessTokens) | `—`
❌ Not implemented

## GET /api/v4/users/tokens/{token_id} - Get a user access token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserAccessToken) | `—`
❌ Not implemented

## POST /api/v4/users/trigger-notify-admin-posts - Trigger notify-admin posts
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/TriggerNotifyAdminPosts) | `—`
❌ Not implemented

## GET /api/v4/users/username/{username} - Get a user by username
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserByUsername) | `IMattermostClient.GetUserByUsernameAsync`
✅ Implemented

## POST /api/v4/users/usernames - Get users by usernames
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUsersByUsernames) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id} - Deactivate a user account.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteUser) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id} - Get a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUser) | `IMattermostClient.GetUserAsync`
✅ Implemented

## PUT /api/v4/users/{user_id} - Update a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUser) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/active - Activate or deactivate a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserActive) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/audits - Get user's audits
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserAudits) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/auth - Update a user's authentication method
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserAuth) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/channel_members - Get all channel members from all teams for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelMembersWithTeamDataForUser) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/channels/{channel_id}/drafts - Delete synced draft
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteDraft) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/channels/{channel_id}/drafts/{thread_id} - Delete synced thread draft
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteDraftForThread) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/demote - Demote a user to a guest
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DemoteUserToGuest) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/email/verify/member - Verify user email by ID
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/VerifyUserEmailWithoutToken) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/image - Delete user's profile image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetDefaultProfileImage) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/image - Get user's profile image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetProfileImage) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/image - Set user's profile image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/SetProfileImage) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/image/default - Return user's default (generated) profile image
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDefaultProfileImage) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/mfa - Update a user's MFA
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserMfa) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/mfa/generate - Generate MFA secret
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GenerateMfaSecret) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/password - Update a user's password
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserPassword) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/patch - Patch a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PatchUser) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/promote - Promote a guest to user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PromoteGuestToUser) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/reset_failed_attempts - Reset the failed password attempts for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/resetPasswordFailedAttempts) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/roles - Update a user's roles
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateUserRoles) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/sessions - Get user's sessions
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetSessions) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/sessions/revoke - Revoke a user session
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevokeSession) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/sessions/revoke/all - Revoke all active sessions for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RevokeAllSessions) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/drafts - Get synced drafts for a team
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetDrafts) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/terms_of_service - Fetches user's latest terms of service action if the latest action was for acceptance.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserTermsOfService) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/terms_of_service - Records user action when they accept or decline custom terms of service
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegisterTermsOfServiceAction) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/tokens - Get user access tokens
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUserAccessTokensForUser) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/tokens - Create a user access token
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateUserAccessToken) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/typing - Publish a user typing websocket event.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/PublishUserTyping) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/uploads - Get uploads for a user
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetUploadsForUser) | `—`
❌ Not implemented

## POST /oauth/intune - Login with Microsoft Intune MAM
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/LoginIntune) | `—`
❌ Not implemented

# Views

## GET /api/v4/channels/{channel_id}/views - List channel views
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/ListChannelViews) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/views - Create channel view
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateChannelView) | `—`
❌ Not implemented

## DELETE /api/v4/channels/{channel_id}/views/{view_id} - Delete a channel view
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteChannelView) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/views/{view_id} - Get a channel view
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetChannelView) | `—`
❌ Not implemented

## PATCH /api/v4/channels/{channel_id}/views/{view_id} - Update a channel view
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelView) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/views/{view_id}/posts - Get posts for a view
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetPostsForView) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/views/{view_id}/sort_order - Update a channel view's sort order
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateChannelViewSortOrder) | `—`
❌ Not implemented

# Webhooks

## GET /api/v4/hooks/incoming - List incoming webhooks
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetIncomingWebhooks) | `—`
❌ Not implemented

## POST /api/v4/hooks/incoming - Create an incoming webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateIncomingWebhook) | `—`
❌ Not implemented

## DELETE /api/v4/hooks/incoming/{hook_id} - Delete an incoming webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteIncomingWebhook) | `—`
❌ Not implemented

## GET /api/v4/hooks/incoming/{hook_id} - Get an incoming webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetIncomingWebhook) | `—`
❌ Not implemented

## PUT /api/v4/hooks/incoming/{hook_id} - Update an incoming webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateIncomingWebhook) | `—`
❌ Not implemented

## GET /api/v4/hooks/outgoing - List outgoing webhooks
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOutgoingWebhooks) | `—`
❌ Not implemented

## POST /api/v4/hooks/outgoing - Create an outgoing webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/CreateOutgoingWebhook) | `—`
❌ Not implemented

## DELETE /api/v4/hooks/outgoing/{hook_id} - Delete an outgoing webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/DeleteOutgoingWebhook) | `—`
❌ Not implemented

## GET /api/v4/hooks/outgoing/{hook_id} - Get an outgoing webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/GetOutgoingWebhook) | `—`
❌ Not implemented

## PUT /api/v4/hooks/outgoing/{hook_id} - Update an outgoing webhook
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/UpdateOutgoingWebhook) | `—`
❌ Not implemented

## POST /api/v4/hooks/outgoing/{hook_id}/regen_token - Regenerate the token for the outgoing webhook.
[Mattermost API](https://developers.mattermost.com/api-documentation/#/operations/RegenOutgoingHookToken) | `—`
❌ Not implemented
