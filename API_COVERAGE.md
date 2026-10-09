# Mattermost.NET API Coverage

This document maps operations in the official Mattermost OpenAPI specification to the public Mattermost.NET SDK surface.

- Mattermost API documentation: https://docs.mattermost.com/api
- Mattermost OpenAPI source: https://github.com/mattermost/mattermost/tree/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source
- Last reviewed: 2026-10-09
- Implemented specification operations: 72/608 (11.8%)
- Additional Calls plugin routes: 3

The SDK uses the current `/api/v4` protocol. Its version is independent of the Mattermost server release number.

Coverage counts unique HTTP method/path pairs from the canonical core API source, including the WebSocket handshake. It does not measure completeness of optional parameters, response models, or WebSocket messages. Administrative and enterprise operations are included in the denominator; plugin APIs such as Calls and Playbooks are excluded. The three implemented Calls plugin routes are listed separately. `GetMeAsync` uses the `me` alias of `GET /api/v4/users/{user_id}` and is not counted as another operation.

## Implemented categories

| Category | Implemented / total |
|---|---|
| Channels | 20 / 61 |
| Emoji | 6 / 9 |
| Files | 3 / 12 |
| Integration Actions | 1 / 4 |
| Posts | 10 / 28 |
| Reactions | 3 / 3 |
| Roles | 4 / 5 |
| Status | 2 / 7 |
| System | 1 / 51 |
| Teams | 12 / 38 |
| Users | 10 / 77 |

## Legend

| Status | Meaning |
|---|---|
| ✅ Implemented | Public `IMattermostClient` method exists and maps to this operation. |
| ❌ Not implemented | No public wrapper currently exists in `IMattermostClient`. |

## Coverage

# Access Control

## DELETE /api/v4/access_control_policies/{policy_id} - Delete an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L302) | `—`
❌ Not implemented

## DELETE /api/v4/access_control_policies/{policy_id}/unassign - Unassign an access control policy from channels or teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L435) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id} - Get an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L269) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id}/activate - Activate or deactivate an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L336) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/{policy_id}/resources/channels - Get channels for an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L489) | `—`
❌ Not implemented

## GET /api/v4/access_control_policies/cel/autocomplete/fields - Get autocomplete fields for access control policies
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L230) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/access_control/attributes - Get access control attributes for a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L576) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/{policy_id}/assign - Assign an access control policy to channels or teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L381) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/{policy_id}/resources/channels/search - Search channels for an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L536) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/check - Check an access control policy expression
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L33) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/simulate_users - Simulate an access control policy decision for an explicit user list
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L148) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/test - Test an access control policy expression
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L117) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/validate_requester - Validate if the current user matches a CEL expression
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L70) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/cel/visual_ast - Get the visual AST for a CEL expression
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L614) | `—`
❌ Not implemented

## POST /api/v4/access_control_policies/search - Search access control policies
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L199) | `—`
❌ Not implemented

## POST /api/v4/access_control/decisions/actions/search - Search allowed actions for the current user (render-time decision)
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L679) | `—`
❌ Not implemented

## PUT /api/v4/access_control_policies - Create an access control policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L2) | `—`
❌ Not implemented

## PUT /api/v4/access_control_policies/activate - Activate or deactivate access control policies
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/access_control.yaml#L645) | `—`
❌ Not implemented

# Agents

## GET /api/v4/agents - Get available agents
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/agents.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/agents/status - Get agents bridge status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/agents.yaml#L28) | `—`
❌ Not implemented

## GET /api/v4/llmservices - Get available LLM services
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/agents.yaml#L54) | `—`
❌ Not implemented

# Audit Logs

## DELETE /api/v4/audit_logs/certificate - Remove audit log certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/audit_logging.yaml#L44) | `—`
❌ Not implemented

## POST /api/v4/audit_logs/certificate - Upload audit log certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/audit_logging.yaml#L2) | `—`
❌ Not implemented

# Boards

## POST /api/v4/boards - Create a board channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/boards.yaml#L2) | `—`
❌ Not implemented

# Bookmarks

## DELETE /api/v4/channels/{channel_id}/bookmarks/{bookmark_id} - Delete channel bookmark
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bookmarks.yaml#L193) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/bookmarks - Get channel bookmarks for Channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bookmarks.yaml#L2) | `—`
❌ Not implemented

## PATCH /api/v4/channels/{channel_id}/bookmarks/{bookmark_id} - Update channel bookmark
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bookmarks.yaml#L112) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/bookmarks - Create channel bookmark
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bookmarks.yaml#L42) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/bookmarks/{bookmark_id}/sort_order - Update channel bookmark's order
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bookmarks.yaml#L239) | `—`
❌ Not implemented

# Bots

## GET /api/v4/bots - Get bots
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L41) | `—`
❌ Not implemented

## GET /api/v4/bots/{bot_user_id} - Get a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L145) | `—`
❌ Not implemented

## POST /api/v4/bots - Create a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/assign/{user_id} - Assign a bot to a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L246) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/convert_to_user - Convert a bot into a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L283) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/disable - Disable a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L184) | `—`
❌ Not implemented

## POST /api/v4/bots/{bot_user_id}/enable - Enable a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L215) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/convert_to_bot - Convert a user into a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1805) | `—`
❌ Not implemented

## PUT /api/v4/bots/{bot_user_id} - Patch a bot
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/bots.yaml#L94) | `—`
❌ Not implemented

# Brand

## DELETE /api/v4/brand/image - Delete current brand image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/brand.yaml#L63) | `—`
❌ Not implemented

## GET /api/v4/brand/image - Get brand image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/brand.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/brand/image - Upload brand image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/brand.yaml#L25) | `—`
❌ Not implemented

# Calls Plugin

## GET /plugins/com.mattermost.calls/calls/{channel_id}/active - Check whether a channel has an active call
[Not listed in the core OpenAPI specification](https://github.com/mattermost/mattermost/tree/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source) | `IMattermostClient.GetCallActiveAsync`
✅ Implemented

## POST /plugins/com.mattermost.calls/{channel_id} - Set channel call state
[Not listed in the core OpenAPI specification](https://github.com/mattermost/mattermost/tree/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source) | `IMattermostClient.SetChannelCallStateAsync`
✅ Implemented

## POST /plugins/com.mattermost.calls/calls/{channel_id}/host/end - End the active call in a channel
[Not listed in the core OpenAPI specification](https://github.com/mattermost/mattermost/tree/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source) | `IMattermostClient.EndCallAsync`
✅ Implemented

# Channels

## DELETE /api/v4/channels/{channel_id} - Delete a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L570) | `IMattermostClient.ArchiveChannelAsync`
✅ Implemented

## DELETE /api/v4/channels/{channel_id}/members/{user_id} - Remove user from channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1731) | `IMattermostClient.DeleteUserFromChannelAsync`
✅ Implemented

## DELETE /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Delete sidebar category
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2917) | `—`
❌ Not implemented

## GET /api/v4/channels - Get a list of all channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id} - Get a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L480) | `IMattermostClient.GetChannelAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/common_teams - Get common teams for members of a Group Message.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2967) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/member_counts_by_group - Channel members counts for each group that has atleast one member in the channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2464) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/members - Get channel members
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1377) | `IMattermostClient.GetChannelMembersAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/members_minus_group_members - Channel members minus group members.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2407) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/members/{user_id} - Get channel member
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1696) | `IMattermostClient.GetChannelMemberAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/moderations - Get information about channel's moderation.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2500) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/pinned - Get a channel's pinned posts
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L882) | `IMattermostClient.GetPinnedPostsAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/stats - Get channel statistics
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L852) | `IMattermostClient.GetChannelStatsAsync`
✅ Implemented

## GET /api/v4/channels/{channel_id}/timezones - Get timezones in a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L445) | `IMattermostClient.GetChannelTimezonesAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/channels - Get public channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L909) | `IMattermostClient.GetTeamChannelsAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/channels/autocomplete - Autocomplete channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1103) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/deleted - Get deleted channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1055) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/managed_categories - Get managed category mappings
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1197) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/name/{channel_name} - Get a channel by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1293) | `IMattermostClient.FindChannelByNameAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/channels/private - Get private channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L958) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/recommended - Get recommended public channels for the current user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1008) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/channels/search_autocomplete - Autocomplete channels for search
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1150) | `—`
❌ Not implemented

## GET /api/v4/teams/name/{team_name}/channels/name/{channel_name} - Get a channel by name and team name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1335) | `IMattermostClient.FindChannelByNameAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/channels - Get all channels from all teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2265) | `IMattermostClient.GetUserChannelsAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/channels/{channel_id}/unread - Get unread messages
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2315) | `IMattermostClient.GetChannelUnreadAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels - Get channels for user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2211) | `IMattermostClient.GetUserTeamChannelsAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Get user's sidebar categories
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2578) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Get sidebar category
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2817) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/categories/order - Get user's sidebar category order
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2720) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/channels/members - Get channel memberships and roles for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2170) | `IMattermostClient.GetUserTeamChannelMembersAsync`
✅ Implemented

## POST /api/v4/channels - Create a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L70) | `IMattermostClient.CreateChannelAsync`
✅ Implemented

## POST /api/v4/channels/{channel_id}/convert_to_channel - Convert group message to private channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L3006) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/members - Add user(s) to channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1420) | `IMattermostClient.AddUserToChannelAsync`
✅ Implemented

## POST /api/v4/channels/{channel_id}/members/ids - Get channel members by ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1653) | `IMattermostClient.GetChannelMembersByIdsAsync`
✅ Implemented

## POST /api/v4/channels/{channel_id}/move - Move a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L802) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/restore - Restore a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L769) | `—`
❌ Not implemented

## POST /api/v4/channels/direct - Create a direct message channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L137) | `IMattermostClient.CreateDirectChannelAsync`
✅ Implemented

## POST /api/v4/channels/group - Create a group message channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L173) | `IMattermostClient.CreateGroupChannelAsync`
✅ Implemented

## POST /api/v4/channels/group/search - Search Group Channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L365) | `—`
❌ Not implemented

## POST /api/v4/channels/members/{user_id}/mark_read - Mark multiple channels as read
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1984) | `—`
❌ Not implemented

## POST /api/v4/channels/members/{user_id}/view - View channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2061) | `—`
❌ Not implemented

## POST /api/v4/channels/search - Search all private and open type channels across all teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L210) | `—`
❌ Not implemented

## POST /api/v4/channels/stats/member_count - Get member counts for multiple channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2027) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/channels/ids - Get a list of channels by ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L404) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/channels/search - Search channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1239) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Create user's sidebar category
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2622) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id} - Update a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L509) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members - Set channel members
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1467) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/autotranslation - Update channel member autotranslation setting
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1929) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/notify_props - Update channel notifications
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1882) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/roles - Update channel roles
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1774) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/members/{user_id}/schemeRoles - Update the scheme-derived roles of a channel member.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L1822) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/moderations/patch - Update a channel's moderation settings.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2535) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/patch - Patch a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L612) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/privacy - Update channel's privacy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L715) | `—`
❌ Not implemented

## PUT /api/v4/channels/{channel_id}/scheme - Set a channel's scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2355) | `—`
❌ Not implemented

## PUT /api/v4/channels/members/{user_id}/direct/read - Mark all direct and group messages as read
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2124) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories - Update user's sidebar categories
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2669) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories/{category_id} - Update sidebar category
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2864) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/channels/categories/order - Update user's sidebar category order
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/channels.yaml#L2764) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/read - Mark all channels and threads in a team as read
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3389) | `—`
❌ Not implemented

# Cloud

## GET /api/v4/cloud/check-cws-connection - Check CWS connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L382) | `—`
❌ Not implemented

## GET /api/v4/cloud/customer - Get cloud customer
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L62) | `—`
❌ Not implemented

## GET /api/v4/cloud/installation - GET endpoint for Installation information
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L273) | `—`
❌ Not implemented

## GET /api/v4/cloud/limits - Get cloud workspace limits
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/cloud/preview/modal_data - Get cloud preview modal data
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L436) | `—`
❌ Not implemented

## GET /api/v4/cloud/products - Get cloud products
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L30) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription - Get cloud subscription
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L243) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription/invoices - Get cloud subscription invoices
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L303) | `—`
❌ Not implemented

## GET /api/v4/cloud/subscription/invoices/{invoice_id}/pdf - Get cloud invoice PDF
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L335) | `—`
❌ Not implemented

## GET /api/v4/hosted_customer/signup_available - Check hosted signup availability
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L366) | `—`
❌ Not implemented

## POST /api/v4/cloud/validate-business-email - Validate business email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L176) | `—`
❌ Not implemented

## POST /api/v4/cloud/validate-workspace-business-email - Validate workspace business email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L216) | `—`
❌ Not implemented

## POST /api/v4/cloud/webhook - POST endpoint for CWS Webhooks
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L412) | `—`
❌ Not implemented

## PUT /api/v4/cloud/customer - Update cloud customer
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L91) | `—`
❌ Not implemented

## PUT /api/v4/cloud/customer/address - Update cloud customer address
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cloud.yaml#L139) | `—`
❌ Not implemented

# Cluster

## GET /api/v4/cluster/status - Get cluster status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/cluster.yaml#L2) | `—`
❌ Not implemented

# Commands

## DELETE /api/v4/commands/{command_id} - Delete a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L235) | `—`
❌ Not implemented

## GET /api/v4/commands - List commands for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L51) | `—`
❌ Not implemented

## GET /api/v4/commands/{command_id} - Get a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L164) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/commands/autocomplete - List autocomplete commands
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L93) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/commands/autocomplete_suggestions - List commands' autocomplete data
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L125) | `—`
❌ Not implemented

## POST /api/v4/commands - Create a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/commands/execute - Execute a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L352) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id} - Update a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L198) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id}/move - Move a command
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L269) | `—`
❌ Not implemented

## PUT /api/v4/commands/{command_id}/regen_token - Generate a new token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/commands.yaml#L316) | `—`
❌ Not implemented

# Compliance

## GET /api/v4/compliance/reports - Get reports
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/compliance.yaml#L26) | `—`
❌ Not implemented

## GET /api/v4/compliance/reports/{report_id} - Get a report
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/compliance.yaml#L69) | `—`
❌ Not implemented

## GET /api/v4/compliance/reports/{report_id}/download - Download a report
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/compliance.yaml#L101) | `—`
❌ Not implemented

## POST /api/v4/compliance/reports - Create report
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/compliance.yaml#L2) | `—`
❌ Not implemented

# Content Flagging

## GET /api/v4/content_flagging/config - Get the system content flagging configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L268) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/fields - Get content flagging property fields
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L114) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/flag/config - Get content flagging configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/post/{post_id} - Get a flagged post with all its content.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L173) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/post/{post_id}/field_values - Get content flagging property field values for a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L139) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/team/{team_id}/reviewers/search - Search content reviewers in a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L315) | `—`
❌ Not implemented

## GET /api/v4/content_flagging/team/{team_id}/status - Get content flagging status for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L33) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/assign/{content_reviewer_id} - Assign a content reviewer to a flagged post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L356) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/exposure_report - Generate and download a post exposure report
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L446) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/flag - Flag a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L68) | `—`
❌ Not implemented

## POST /api/v4/content_flagging/post/{post_id}/report - Generate and download a flagged post report
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L395) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/config - Update the system content flagging configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L289) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/post/{post_id}/keep - Keep a flagged post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L236) | `—`
❌ Not implemented

## PUT /api/v4/content_flagging/post/{post_id}/remove - Remove a flagged post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/content_flagging.yaml#L204) | `—`
❌ Not implemented

# Custom Profile Attributes

## DELETE /api/v4/custom_profile_attributes/fields/{field_id} - Delete a Custom Profile Attribute field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L243) | `—`
❌ Not implemented

## GET /api/v4/custom_profile_attributes/fields - List all the Custom Profile Attributes fields
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/custom_profile_attributes/group - Get Custom Profile Attribute property group data
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L340) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/custom_profile_attributes - List Custom Profile Attribute values
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L369) | `—`
❌ Not implemented

## PATCH /api/v4/custom_profile_attributes/fields/{field_id} - Patch a Custom Profile Attribute field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L129) | `—`
❌ Not implemented

## PATCH /api/v4/custom_profile_attributes/values - Patch Custom Profile Attribute values
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L278) | `—`
❌ Not implemented

## PATCH /api/v4/users/{user_id}/custom_profile_attributes - Update custom profile attribute values for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L408) | `—`
❌ Not implemented

## POST /api/v4/custom_profile_attributes/fields - Create a Custom Profile Attribute field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/custom_profile_attributes.yaml#L26) | `—`
❌ Not implemented

# Data Retention

## DELETE /api/v4/data_retention/policies/{policy_id} - Delete a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L236) | `—`
❌ Not implemented

## DELETE /api/v4/data_retention/policies/{policy_id}/channels - Delete channels from a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L563) | `—`
❌ Not implemented

## DELETE /api/v4/data_retention/policies/{policy_id}/teams - Delete teams from a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L371) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies - Get the granular data retention policies
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L71) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies_count - Get the number of granular data retention policies
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L35) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id} - Get a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L155) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id}/channels - Get the channels for a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L468) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policies/{policy_id}/teams - Get the teams for a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L274) | `—`
❌ Not implemented

## GET /api/v4/data_retention/policy - Get the global data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/data_retention/channel_policies - Get the policies which are applied to a user's channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3899) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/data_retention/team_policies - Get the policies which are applied to a user's teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3849) | `—`
❌ Not implemented

## PATCH /api/v4/data_retention/policies/{policy_id} - Patch a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L192) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies - Create a new granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L117) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/channels - Add channels to a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L517) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/channels/search - Search for the channels in a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L610) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/teams - Add teams to a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L325) | `—`
❌ Not implemented

## POST /api/v4/data_retention/policies/{policy_id}/teams/search - Search for the teams in a granular data retention policy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/dataretention.yaml#L418) | `—`
❌ Not implemented

# Delivery Tracking

## GET /api/v4/delivery_tracking/config - Get the post delivery audit logging configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/delivery_tracking.yaml#L2) | `—`
❌ Not implemented

## PUT /api/v4/delivery_tracking/config - Update the post delivery audit logging configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/delivery_tracking.yaml#L29) | `—`
❌ Not implemented

# Elasticsearch

## POST /api/v4/elasticsearch/purge_indexes - Purge all Elasticsearch indexes
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/elasticsearch.yaml#L35) | `—`
❌ Not implemented

## POST /api/v4/elasticsearch/test - Test Elasticsearch configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/elasticsearch.yaml#L2) | `—`
❌ Not implemented

# Emoji

## DELETE /api/v4/emoji/{emoji_id} - Delete a custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L125) | `—`
❌ Not implemented

## GET /api/v4/emoji - Get a list of custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L46) | `IMattermostClient.GetEmojisAsync`
✅ Implemented

## GET /api/v4/emoji/{emoji_id} - Get a custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L94) | `IMattermostClient.GetEmojiAsync`
✅ Implemented

## GET /api/v4/emoji/{emoji_id}/image - Get custom emoji image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L193) | `—`
❌ Not implemented

## GET /api/v4/emoji/autocomplete - Autocomplete custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L272) | `IMattermostClient.AutocompleteEmojisAsync`
✅ Implemented

## GET /api/v4/emoji/name/{emoji_name} - Get a custom emoji by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L159) | `IMattermostClient.GetEmojiByNameAsync`
✅ Implemented

## POST /api/v4/emoji - Create a custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/emoji/names - Get custom emojis by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L310) | `IMattermostClient.GetEmojisByNamesAsync`
✅ Implemented

## POST /api/v4/emoji/search - Search custom emoji
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/emoji.yaml#L223) | `IMattermostClient.SearchEmojisAsync`
✅ Implemented

# Ephemeral mode

## POST /api/v4/ephemeral_mode/cleanup - Log an ephemeral mode cleanup run
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ephemeral_mode.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/ephemeral_mode/purge - Log an ephemeral mode offline purge
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ephemeral_mode.yaml#L77) | `—`
❌ Not implemented

## POST /api/v4/ephemeral_mode/wipe - Log an ephemeral mode session wipe
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ephemeral_mode.yaml#L144) | `—`
❌ Not implemented

# Exports

## DELETE /api/v4/exports/{export_name} - Delete an export file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/exports.yaml#L57) | `—`
❌ Not implemented

## GET /api/v4/exports - List export files
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/exports.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/exports/{export_name} - Download an export file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/exports.yaml#L25) | `—`
❌ Not implemented

## POST /api/v4/exports/{export_name}/presign-url - Create a presigned URL for export download
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/exports.yaml#L88) | `—`
❌ Not implemented

# Files

## GET /api/v4/files/{file_id} - Get a file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L93) | `IMattermostClient.GetFileAsync`, `IMattermostClient.GetFileStreamAsync`
✅ Implemented

## GET /api/v4/files/{file_id}/info - Get metadata for a file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L328) | `IMattermostClient.GetFileDetailsAsync`
✅ Implemented

## GET /api/v4/files/{file_id}/link - Get a public file link
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L279) | `—`
❌ Not implemented

## GET /api/v4/files/{file_id}/preview - Get a file's preview
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L217) | `—`
❌ Not implemented

## GET /api/v4/files/{file_id}/thumbnail - Get a file's thumbnail
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L155) | `—`
❌ Not implemented

## GET /files/{file_id}/public - Get a public file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L371) | `—`
❌ Not implemented

## HEAD /api/v4/files/{file_id} - Get file metadata headers
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L129) | `—`
❌ Not implemented

## HEAD /api/v4/files/{file_id}/preview - Get preview metadata headers
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L253) | `—`
❌ Not implemented

## HEAD /api/v4/files/{file_id}/thumbnail - Get thumbnail metadata headers
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L191) | `—`
❌ Not implemented

## HEAD /files/{file_id}/public - Get public file metadata headers
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L412) | `—`
❌ Not implemented

## POST /api/v4/files - Upload a file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L2) | `IMattermostClient.UploadFileAsync`
✅ Implemented

## POST /api/v4/files/search - Search files across the teams of the current user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L521) | `—`
❌ Not implemented

# Groups

## DELETE /api/v4/groups/{group_id} - Deletes a custom group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L180) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/channels/{channel_id}/link - Unlink a channel from a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L432) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/members - Removes members from a custom group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L800) | `—`
❌ Not implemented

## DELETE /api/v4/groups/{group_id}/teams/{team_id}/link - Unlink a team from a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L346) | `—`
❌ Not implemented

## DELETE /api/v4/ldap/groups/{remote_id}/link - Delete a link for LDAP group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L215) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/groups - Get channel groups
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L935) | `—`
❌ Not implemented

## GET /api/v4/groups - Get groups
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id} - Get a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L146) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/channels - Get channel syncables for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L602) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/channels/{channel_id} - Get a channel syncable for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L520) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/members - Get group users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L745) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/stats - Get group stats
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L893) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/teams - Get team syncables for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L563) | `—`
❌ Not implemented

## GET /api/v4/groups/{group_id}/teams/{team_id} - Get a team syncable for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L477) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/groups - Get team groups
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L992) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/groups_by_channels - Get team groups by channels
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L1102) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/groups - Get groups for a userId
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L1166) | `—`
❌ Not implemented

## POST /api/v4/groups - Create a custom group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L90) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/channels/{channel_id}/link - Link a channel to a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L388) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/members - Adds members to a custom group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L846) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/restore - Restore a previously deleted group.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L268) | `—`
❌ Not implemented

## POST /api/v4/groups/{group_id}/teams/{team_id}/link - Link a team to a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L305) | `—`
❌ Not implemented

## POST /api/v4/groups/names - Get groups by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L1196) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/channels/{channel_id}/patch - Patch a channel syncable for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L693) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/patch - Patch a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L213) | `—`
❌ Not implemented

## PUT /api/v4/groups/{group_id}/teams/{team_id}/patch - Patch a team syncable for a group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/groups.yaml#L641) | `—`
❌ Not implemented

# Health

## DELETE /api/v4/health/findings/{fingerprint}/mute - Unmute a health finding
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/health.yaml#L101) | `—`
❌ Not implemented

## GET /api/v4/health/findings - Get health findings
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/health.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/health/findings/{fingerprint}/mute - Mute a health finding
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/health.yaml#L57) | `—`
❌ Not implemented

# Imports

## DELETE /api/v4/imports/{import_name} - Delete an import file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/imports.yaml#L26) | `—`
❌ Not implemented

## GET /api/v4/imports - List import files
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/imports.yaml#L2) | `—`
❌ Not implemented

# Integration Actions

## POST /api/v4/actions/dialogs/execute - Execute a dialog action button
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/actions.yaml#L304) | `—`
❌ Not implemented

## POST /api/v4/actions/dialogs/lookup - Lookup dialog elements
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/actions.yaml#L210) | `—`
❌ Not implemented

## POST /api/v4/actions/dialogs/open - Open a dialog
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/actions.yaml#L2) | `IMattermostClient.OpenInteractiveDialogAsync`
✅ Implemented

## POST /api/v4/actions/dialogs/submit - Submit a dialog
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/actions.yaml#L118) | `—`
❌ Not implemented

# Ip

## GET /api/v4/ip_filtering - Get all IP filters
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ip_filters.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/ip_filtering/my_ip - Get all IP filters
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ip_filters.yaml#L64) | `—`
❌ Not implemented

## POST /api/v4/ip_filtering - Get all IP filters
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ip_filters.yaml#L28) | `—`
❌ Not implemented

# Jobs

## GET /api/v4/jobs - Get the jobs.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/jobs/{job_id} - Get a job.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L115) | `—`
❌ Not implemented

## GET /api/v4/jobs/{job_id}/download - Download the results of a job.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L156) | `—`
❌ Not implemented

## GET /api/v4/jobs/type/{job_type} - Get the jobs of the given type.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L224) | `—`
❌ Not implemented

## PATCH /api/v4/jobs/{job_id}/status - Update the status of a job
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L288) | `—`
❌ Not implemented

## POST /api/v4/jobs - Create a new job.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L66) | `—`
❌ Not implemented

## POST /api/v4/jobs/{job_id}/cancel - Cancel a job.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/jobs.yaml#L183) | `—`
❌ Not implemented

# LDAP

## DELETE /api/v4/ldap/certificate/private - Remove private key
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L393) | `—`
❌ Not implemented

## DELETE /api/v4/ldap/certificate/public - Remove public certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L329) | `—`
❌ Not implemented

## GET /api/v4/ldap/groups - Returns a list of LDAP groups
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L137) | `—`
❌ Not implemented

## POST /api/v4/ldap/certificate/private - Upload private key
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L354) | `—`
❌ Not implemented

## POST /api/v4/ldap/certificate/public - Upload public certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L290) | `—`
❌ Not implemented

## POST /api/v4/ldap/groups/{remote_id}/link - Link a LDAP group
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L184) | `—`
❌ Not implemented

## POST /api/v4/ldap/migrateid - Migrate Id LDAP
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L247) | `—`
❌ Not implemented

## POST /api/v4/ldap/sync - Sync with LDAP
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/ldap/test - Test LDAP configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L24) | `—`
❌ Not implemented

## POST /api/v4/ldap/test_connection - Test LDAP connection with specific settings
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L48) | `—`
❌ Not implemented

## POST /api/v4/ldap/test_diagnostics - Test LDAP diagnostics with specific settings
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L85) | `—`
❌ Not implemented

## POST /api/v4/ldap/users/{user_id}/group_sync_memberships - Create memberships for LDAP configured channels and teams for this user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/ldap.yaml#L418) | `—`
❌ Not implemented

# Logs

## GET /api/v4/logs/download - Download system logs
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/logs.yaml#L2) | `—`
❌ Not implemented

# Metrics

## POST /api/v4/client_perf - Report client performance metrics
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/metrics.yaml#L2) | `—`
❌ Not implemented

# Oauth

## DELETE /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Delete a connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L159) | `—`
❌ Not implemented

## GET /api/v4/oauth/outgoing_connections - List all connections
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Get a connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L75) | `—`
❌ Not implemented

## POST /api/v4/oauth/outgoing_connections - Create a connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L35) | `—`
❌ Not implemented

## POST /api/v4/oauth/outgoing_connections/validate - Validate a connection configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L195) | `—`
❌ Not implemented

## PUT /api/v4/oauth/outgoing_connections/{outgoing_oauth_connection_id} - Update a connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/outgoing_oauth_connections.yaml#L112) | `—`
❌ Not implemented

# OAuth

## DELETE /api/v4/oauth/apps/{app_id} - Delete an OAuth app
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L215) | `—`
❌ Not implemented

## GET /.well-known/oauth-authorization-server - Get OAuth 2.0 Authorization Server Metadata
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L323) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps - Get OAuth apps
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L65) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps/{app_id} - Get an OAuth app
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L107) | `—`
❌ Not implemented

## GET /api/v4/oauth/apps/{app_id}/info - Get info on an OAuth app
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L288) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/oauth/apps/authorized - Get authorized OAuth apps
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L398) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps - Register OAuth app
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps/{app_id}/regen_secret - Regenerate OAuth app secret
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L251) | `—`
❌ Not implemented

## POST /api/v4/oauth/apps/register - Register OAuth client using Dynamic Client Registration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L358) | `—`
❌ Not implemented

## PUT /api/v4/oauth/apps/{app_id} - Update an OAuth app
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/oauth.yaml#L142) | `—`
❌ Not implemented

# Permissions

## POST /api/v4/permissions/ancillary - Return all system console subsection ancillary permissions
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/permissions.yaml#L2) | `—`
❌ Not implemented

# Plugins

## DELETE /api/v4/plugins/{plugin_id} - Remove plugin
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L137) | `—`
❌ Not implemented

## GET /api/v4/plugins - Get plugins
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L52) | `—`
❌ Not implemented

## GET /api/v4/plugins/marketplace - Gets all the marketplace plugins
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L356) | `—`
❌ Not implemented

## GET /api/v4/plugins/marketplace/first_admin_visit - Get if the Plugin Marketplace has been visited by at least an admin.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L422) | `—`
❌ Not implemented

## GET /api/v4/plugins/statuses - Get plugins status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L290) | `—`
❌ Not implemented

## GET /api/v4/plugins/webapp - Get webapp plugins
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L260) | `—`
❌ Not implemented

## POST /api/v4/plugins - Upload plugin
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/detach - Detach a reattached plugin process
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L499) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/disable - Disable plugin
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L219) | `—`
❌ Not implemented

## POST /api/v4/plugins/{plugin_id}/enable - Enable plugin
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L178) | `—`
❌ Not implemented

## POST /api/v4/plugins/install_from_url - Install plugin from url
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L93) | `—`
❌ Not implemented

## POST /api/v4/plugins/marketplace - Installs a marketplace plugin
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L320) | `—`
❌ Not implemented

## POST /api/v4/plugins/reattach - Reattach a plugin process
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L472) | `—`
❌ Not implemented

# Posts

## DELETE /api/v4/posts/{post_id} - Delete a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L242) | `IMattermostClient.DeletePostAsync`
✅ Implemented

## DELETE /api/v4/posts/{post_id}/burn - Burn a burn-on-read post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1432) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/posts/{post_id}/ack - Delete a post acknowledgement
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1232) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/posts - Get posts for a channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L700) | `IMattermostClient.GetChannelPostsAsync`
✅ Implemented

## GET /api/v4/posts/{post_id} - Get a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L187) | `IMattermostClient.GetPostAsync`
✅ Implemented

## GET /api/v4/posts/{post_id}/edit_history - Get post edit history
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L666) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/files/info - Get file info for post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L592) | `IMattermostClient.GetPostFilesAsync`
✅ Implemented

## GET /api/v4/posts/{post_id}/info - Get post info
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L634) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/reveal - Reveal a burn-on-read post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1381) | `—`
❌ Not implemented

## GET /api/v4/posts/{post_id}/thread - Get a thread
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L436) | `IMattermostClient.GetThreadPostsAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/channels/{channel_id}/posts/unread - Get posts around oldest unread
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L808) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/posts/flagged - Get a list of flagged posts
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L534) | `—`
❌ Not implemented

## POST /api/v4/posts - Create a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L2) | `IMattermostClient.CreatePostAsync`, `IMattermostClient.CreatePostWithRawPropsAsync`
✅ Implemented

## POST /api/v4/posts/{post_id}/actions/{action_id} - Perform a post action
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1031) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/move - Move a post (and any posts within that post's thread)
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1277) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/pin - Pin a post to the channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L966) | `IMattermostClient.PinPostAsync`
✅ Implemented

## POST /api/v4/posts/{post_id}/restore/{restore_version_id} - Restores a past version of a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1333) | `—`
❌ Not implemented

## POST /api/v4/posts/{post_id}/unpin - Unpin a post to the channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L998) | `IMattermostClient.UnpinPostAsync`
✅ Implemented

## POST /api/v4/posts/ephemeral - Create a ephemeral post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L91) | `—`
❌ Not implemented

## POST /api/v4/posts/ids - Get posts by a list of ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1092) | `IMattermostClient.GetPostsByIdsAsync`
✅ Implemented

## POST /api/v4/posts/rewrite - Rewrite a message using AI
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1484) | `—`
❌ Not implemented

## POST /api/v4/posts/search - Search posts across all teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L143) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/posts/search - Search for team posts
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L898) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/ack - Acknowledge a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1189) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/reminder - Set a post reminder
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L1133) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/posts/{post_id}/set_unread - Mark as unread from a post.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L332) | `—`
❌ Not implemented

## PUT /api/v4/posts/{post_id} - Update a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L274) | `—`
❌ Not implemented

## PUT /api/v4/posts/{post_id}/patch - Patch a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/posts.yaml#L377) | `IMattermostClient.PatchPostAsync`, `IMattermostClient.UpdatePostAsync`, `IMattermostClient.UpdatePostWithRawPropsAsync`
✅ Implemented

# Preferences

## GET /api/v4/users/{user_id}/preferences - Get the user's preferences
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/preferences.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/preferences/{category} - List a user's preferences by category
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/preferences.yaml#L123) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/preferences/{category}/name/{preference_name} - Get a specific user preference
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/preferences.yaml#L163) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/preferences/delete - Delete user's preferences
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/preferences.yaml#L80) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/preferences - Save the user's preferences
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/preferences.yaml#L35) | `—`
❌ Not implemented

# Properties

## DELETE /api/v4/properties/groups/{group_name}/{object_type}/fields/{field_id} - Delete a property field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L450) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/{object_type}/fields - Get property fields
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L112) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/{object_type}/values/{target_id} - Get property values for a target
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L499) | `—`
❌ Not implemented

## GET /api/v4/properties/groups/{group_name}/system/values - Get property values for the system
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L648) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/{object_type}/fields/{field_id} - Update a property field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L385) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/{object_type}/values/{target_id} - Update property values for a target
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L586) | `—`
❌ Not implemented

## PATCH /api/v4/properties/groups/{group_name}/system/values - Update property values for the system
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L722) | `—`
❌ Not implemented

## POST /api/v4/properties/groups/{group_name}/{object_type}/fields - Create a property field
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/properties/groups/{group_name}/fields/search - Search property fields across multiple object types
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/properties.yaml#L251) | `—`
❌ Not implemented

# Reactions

## DELETE /api/v4/users/{user_id}/posts/{post_id}/reactions/{emoji_name} - Remove a reaction from a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reactions.yaml#L63) | `IMattermostClient.RemoveReactionAsync`
✅ Implemented

## GET /api/v4/posts/{post_id}/reactions - Get a list of reactions to a post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reactions.yaml#L31) | `IMattermostClient.GetReactionsAsync`
✅ Implemented

## POST /api/v4/reactions - Create a reaction
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reactions.yaml#L2) | `IMattermostClient.AddReactionAsync`
✅ Implemented

# Recaps

## DELETE /api/v4/recaps/{recap_id} - Delete a recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L199) | `—`
❌ Not implemented

## GET /api/v4/recaps - Get current user's recaps
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L56) | `—`
❌ Not implemented

## GET /api/v4/recaps/{recap_id} - Get a specific recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L164) | `—`
❌ Not implemented

## GET /api/v4/recaps/limit_status - Get recap limit status for the current user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L97) | `—`
❌ Not implemented

## POST /api/v4/recaps - Create a channel recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/recaps/{recap_id}/read - Mark a recap as read
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L235) | `—`
❌ Not implemented

## POST /api/v4/recaps/{recap_id}/regenerate - Regenerate a recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L271) | `—`
❌ Not implemented

## POST /api/v4/recaps/mark_viewed - Mark all of the authenticated user's finished recaps as viewed
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/recaps.yaml#L124) | `—`
❌ Not implemented

# Remote Clusters

## DELETE /api/v4/remotecluster/{remote_id} - Delete a remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L200) | `—`
❌ Not implemented

## GET /api/v4/remotecluster - Get a list of remote clusters.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/remotecluster/{remote_id} - Get a remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L129) | `—`
❌ Not implemented

## PATCH /api/v4/remotecluster/{remote_id} - Patch a remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L159) | `—`
❌ Not implemented

## POST /api/v4/remotecluster - Create a new remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L72) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{remote_id}/generate_invite - Generate invite code.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L228) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{user_id}/image - Set profile image for a remote user.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L439) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/accept_invite - Accept a remote cluster invite code.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L262) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/confirm_invite - Confirm an invite with a remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L367) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/msg - Receive a remote cluster message.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L337) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/ping - Receive a ping from a remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L307) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/upload/{upload_id} - Upload file data for a remote upload session.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/remoteclusters.yaml#L393) | `—`
❌ Not implemented

# Reports

## GET /api/v4/reports/users - Get a list of paged and sorted users for admin reporting purposes
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reports.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/reports/users/count - Gets the full count of users that match the filter.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reports.yaml#L105) | `—`
❌ Not implemented

## POST /api/v4/reports/posts - Get posts for reporting and compliance purposes using cursor-based pagination
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reports.yaml#L191) | `—`
❌ Not implemented

## POST /api/v4/reports/users/export - Starts a job to export the users to a report file.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/reports.yaml#L155) | `—`
❌ Not implemented

# Roles

## GET /api/v4/roles - Get a list of all the roles
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/roles.yaml#L2) | `IMattermostClient.GetRolesAsync`
✅ Implemented

## GET /api/v4/roles/{role_id} - Get a role
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/roles.yaml#L29) | `IMattermostClient.GetRoleAsync`
✅ Implemented

## GET /api/v4/roles/name/{role_name} - Get a role
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/roles.yaml#L60) | `IMattermostClient.GetRoleByNameAsync`
✅ Implemented

## POST /api/v4/roles/names - Get a list of roles by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/roles.yaml#L145) | `IMattermostClient.GetRolesByNamesAsync`
✅ Implemented

## PUT /api/v4/roles/{role_id}/patch - Patch a role
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/roles.yaml#L91) | `—`
❌ Not implemented

# Root

## POST /api/v4/notifications/ack - Acknowledge receiving of a notification
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1522) | `—`
❌ Not implemented

# SAML

## DELETE /api/v4/saml/certificate/idp - Remove IDP certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L89) | `—`
❌ Not implemented

## DELETE /api/v4/saml/certificate/private - Remove private key
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L223) | `—`
❌ Not implemented

## DELETE /api/v4/saml/certificate/public - Remove public certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L156) | `—`
❌ Not implemented

## GET /api/v4/saml/certificate/status - Get certificate status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L250) | `—`
❌ Not implemented

## GET /api/v4/saml/metadata - Get metadata
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/idp - Upload IDP certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L49) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/private - Upload private key
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L183) | `—`
❌ Not implemented

## POST /api/v4/saml/certificate/public - Upload public certificate
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L116) | `—`
❌ Not implemented

## POST /api/v4/saml/metadatafromidp - Get metadata from Identity Provider
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L21) | `—`
❌ Not implemented

## POST /api/v4/saml/reset_auth_data - Reset AuthData to Email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/saml.yaml#L274) | `—`
❌ Not implemented

# Scheduled Post

## DELETE /api/v4/posts/schedule/{scheduled_post_id} - Delete a scheduled post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_post.yaml#L183) | `—`
❌ Not implemented

## GET /api/v4/posts/scheduled/team/{team_id} - Gets all scheduled posts for a user for the specified team..
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_post.yaml#L70) | `—`
❌ Not implemented

## POST /api/v4/posts/schedule - Creates a scheduled post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_post.yaml#L2) | `—`
❌ Not implemented

## PUT /api/v4/posts/schedule/{scheduled_post_id} - Update a scheduled post
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_post.yaml#L112) | `—`
❌ Not implemented

# Scheduled recaps

## DELETE /api/v4/scheduled_recaps/{scheduled_recap_id} - Delete a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L311) | `—`
❌ Not implemented

## GET /api/v4/scheduled_recaps - Get current user's scheduled recaps
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L114) | `—`
❌ Not implemented

## GET /api/v4/scheduled_recaps/{scheduled_recap_id} - Get a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L157) | `—`
❌ Not implemented

## POST /api/v4/scheduled_recaps - Create a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/scheduled_recaps/{scheduled_recap_id}/pause - Pause a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L349) | `—`
❌ Not implemented

## POST /api/v4/scheduled_recaps/{scheduled_recap_id}/resume - Resume a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L387) | `—`
❌ Not implemented

## PUT /api/v4/scheduled_recaps/{scheduled_recap_id} - Update a scheduled recap
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/scheduled_recaps.yaml#L194) | `—`
❌ Not implemented

# Schemes

## DELETE /api/v4/schemes/{scheme_id} - Delete a scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L136) | `—`
❌ Not implemented

## GET /api/v4/schemes - Get the schemes.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id} - Get a scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L104) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id}/channels - Get a page of channels which use this scheme.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L283) | `—`
❌ Not implemented

## GET /api/v4/schemes/{scheme_id}/teams - Get a page of teams which use this scheme.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L228) | `—`
❌ Not implemented

## POST /api/v4/schemes - Create a scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L53) | `—`
❌ Not implemented

## PUT /api/v4/schemes/{scheme_id}/patch - Patch a scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/schemes.yaml#L171) | `—`
❌ Not implemented

# Shared Channels

## GET /api/v4/remotecluster/{remote_id}/sharedchannelremotes - Get shared channel remotes by remote cluster.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L52) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/{channel_id}/remotes - Get remote clusters for a shared channel
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L241) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/{team_id} - Get all shared channels for team.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/remote_info/{remote_id} - Get remote cluster info by ID for user.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L121) | `—`
❌ Not implemented

## GET /api/v4/sharedchannels/users/{user_id}/can_dm/{other_user_id} - Check if user can DM another user in shared channels context
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L279) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{remote_id}/channels/{channel_id}/invite - Invites a remote cluster to a channel.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L163) | `—`
❌ Not implemented

## POST /api/v4/remotecluster/{remote_id}/channels/{channel_id}/uninvite - Uninvites a remote cluster to a channel.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/sharedchannels.yaml#L201) | `—`
❌ Not implemented

# Status

## DELETE /api/v4/users/{user_id}/status/custom - Unsets user custom status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L160) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/status/custom/recent - Delete user's recent custom status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L184) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/status - Get user status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L2) | `IMattermostClient.GetUserStatusAsync`
✅ Implemented

## POST /api/v4/users/{user_id}/status/custom/recent/delete - Delete user's recent custom status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L233) | `—`
❌ Not implemented

## POST /api/v4/users/status/ids - Get user statuses by id
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L81) | `IMattermostClient.GetUsersStatusesByIdsAsync`
✅ Implemented

## PUT /api/v4/users/{user_id}/status - Update user status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L29) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/status/custom - Update user custom status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/status.yaml#L113) | `—`
❌ Not implemented

# System

## DELETE /api/v4/license - Remove license file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L892) | `—`
❌ Not implemented

## DELETE /api/v4/server_busy - Clears the server busy (high load) flag
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1496) | `—`
❌ Not implemented

## DELETE /api/v4/system/e2e/ai_bridge - Reset AI bridge E2E test helper
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L406) | `—`
❌ Not implemented

## GET /api/v4/analytics/old - Get analytics
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1321) | `—`
❌ Not implemented

## GET /api/v4/audits - Get audits
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1110) | `—`
❌ Not implemented

## GET /api/v4/config - Get configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L629) | `—`
❌ Not implemented

## GET /api/v4/config/client - Get client configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L765) | `—`
❌ Not implemented

## GET /api/v4/config/environment - Get configuration made through environment variables
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L778) | `—`
❌ Not implemented

## GET /api/v4/image - Get an image by url
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1574) | `—`
❌ Not implemented

## GET /api/v4/latest_version - Get latest public server release information
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1366) | `—`
❌ Not implemented

## GET /api/v4/license/client - Get client license
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1005) | `—`
❌ Not implemented

## GET /api/v4/license/load_metric - Get license load metric
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1020) | `—`
❌ Not implemented

## GET /api/v4/logs - Get logs
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1170) | `—`
❌ Not implemented

## GET /api/v4/redirect_location - Get redirect location
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1543) | `—`
❌ Not implemented

## GET /api/v4/server_busy - Get server busy expiry time.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1471) | `—`
❌ Not implemented

## GET /api/v4/system/e2e/ai_bridge - Get AI bridge E2E test helper state
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L380) | `—`
❌ Not implemented

## GET /api/v4/system/notices/{team_id} - Get notices for logged in user in specified team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L198) | `—`
❌ Not implemented

## GET /api/v4/system/onboarding/complete - Get first admin onboarding completion status
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L281) | `—`
❌ Not implemented

## GET /api/v4/system/ping - Check system health
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L25) | `—`
❌ Not implemented

## GET /api/v4/system/schema/version - Get applied database schema migrations
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1405) | `—`
❌ Not implemented

## GET /api/v4/system/support_packet - Download a zip file which contains helpful and useful information for troubleshooting your mattermost instance.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1723) | `—`
❌ Not implemented

## GET /api/v4/system/timezones - Retrieve a list of supported timezones
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/trial-license/prev - Get last trial license used
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1087) | `—`
❌ Not implemented

## GET /api/v4/upgrade_to_enterprise/allowed - Check if the user is allowed to upgrade to Enterprise Edition
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1659) | `—`
❌ Not implemented

## GET /api/v4/upgrade_to_enterprise/status - Get the current status for the inplace upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1627) | `—`
❌ Not implemented

## GET /api/v4/websocket - Open a WebSocket connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L106) | `IMattermostClient.StartReceivingAsync`
✅ Implemented

## GET /manualtest - Run manual testing helpers
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L150) | `—`
❌ Not implemented

## POST /api/v4/caches/invalidate - Invalidate all the caches
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1147) | `—`
❌ Not implemented

## POST /api/v4/config/migrate - Migrate config storage
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L729) | `—`
❌ Not implemented

## POST /api/v4/config/reload - Reload configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L708) | `—`
❌ Not implemented

## POST /api/v4/database/recycle - Recycle database connections
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L433) | `—`
❌ Not implemented

## POST /api/v4/email/test - Send a test email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L455) | `—`
❌ Not implemented

## POST /api/v4/file/s3_test - Test AWS S3 connection
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L592) | `—`
❌ Not implemented

## POST /api/v4/file/test - Test the configured file storage backend
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L554) | `—`
❌ Not implemented

## POST /api/v4/integrity - Perform a database integrity check
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1697) | `—`
❌ Not implemented

## POST /api/v4/license - Upload license file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L853) | `—`
❌ Not implemented

## POST /api/v4/license/preview - Preview license file
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L916) | `—`
❌ Not implemented

## POST /api/v4/logs - Add log message
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1207) | `—`
❌ Not implemented

## POST /api/v4/logs/query - Query server logs with filters
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1250) | `—`
❌ Not implemented

## POST /api/v4/notifications/test - Send a test notification
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L488) | `—`
❌ Not implemented

## POST /api/v4/plugins/marketplace/first_admin_visit - Stores that the Plugin Marketplace has been visited by at least an admin.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/plugins.yaml#L443) | `—`
❌ Not implemented

## POST /api/v4/restart - Restart the system after an upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1673) | `—`
❌ Not implemented

## POST /api/v4/server_busy - Set the server busy (high load) flag
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1436) | `—`
❌ Not implemented

## POST /api/v4/site_url/test - Checks the validity of a Site URL
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L512) | `—`
❌ Not implemented

## POST /api/v4/system/onboarding/complete - Complete first admin onboarding
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L304) | `—`
❌ Not implemented

## POST /api/v4/trial-license - Request and install a trial license for your server
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1051) | `—`
❌ Not implemented

## POST /api/v4/upgrade_to_enterprise - Executes an inplace upgrade from Team Edition to Enterprise Edition
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L1599) | `—`
❌ Not implemented

## PUT /api/v4/config - Update configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L670) | `—`
❌ Not implemented

## PUT /api/v4/config/patch - Patch configuration
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L812) | `—`
❌ Not implemented

## PUT /api/v4/system/e2e/ai_bridge - Configure AI bridge E2E test helper
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L343) | `—`
❌ Not implemented

## PUT /api/v4/system/notices/view - Update notices as 'viewed'
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/system.yaml#L249) | `—`
❌ Not implemented

# Teams

## DELETE /api/v4/teams/{team_id} - Delete a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L198) | `—`
❌ Not implemented

## DELETE /api/v4/teams/{team_id}/image - Remove the team icon
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1148) | `—`
❌ Not implemented

## DELETE /api/v4/teams/{team_id}/members/{user_id} - Remove user from team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L834) | `—`
❌ Not implemented

## DELETE /api/v4/teams/invites/email - Invalidate active email invitations
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1550) | `—`
❌ Not implemented

## GET /api/v4/teams - Get teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L45) | `IMattermostClient.GetTeamsAsync`
✅ Implemented

## GET /api/v4/teams/{team_id} - Get a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L102) | `IMattermostClient.GetTeamAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/access_control/attributes - Get the access control attributes governing a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L992) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/access_control/policy - Get the access control policy for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L948) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/image - Get the team icon
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1065) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/members - Get team members
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L567) | `IMattermostClient.GetTeamMembersAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/members_minus_group_members - Team members minus group members.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1670) | `—`
❌ Not implemented

## GET /api/v4/teams/{team_id}/members/{user_id} - Get a team member
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L797) | `IMattermostClient.GetTeamMemberAsync`
✅ Implemented

## GET /api/v4/teams/{team_id}/stats - Get a team stats
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L916) | `IMattermostClient.GetTeamStatsAsync`
✅ Implemented

## GET /api/v4/teams/invite/{invite_id} - Get invite info for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1576) | `—`
❌ Not implemented

## GET /api/v4/teams/name/{team_name} - Get a team by name
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L382) | `IMattermostClient.GetTeamByNameAsync`
✅ Implemented

## GET /api/v4/teams/name/{team_name}/exists - Check if team exists
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L503) | `IMattermostClient.TeamExistsAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams - Get a user's teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L533) | `IMattermostClient.GetUserTeamsAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/unread - Get unreads for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1349) | `IMattermostClient.GetTeamUnreadAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams/members - Get team members for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L760) | `IMattermostClient.GetUserTeamMembersAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/teams/unread - Get team unreads for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1301) | `IMattermostClient.GetUserTeamsUnreadAsync`
✅ Implemented

## POST /api/v4/teams - Create a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/files/search - Search files in a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/files.yaml#L445) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/image - Sets the team icon
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1100) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/invite-guests/email - Invite guests to the team by email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1477) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/invite/email - Invite users to the team by email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1390) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members - Add user to team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L627) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members/batch - Add multiple users to team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L707) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/members/ids - Get team members by ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L875) | `IMattermostClient.GetTeamMembersByIdsAsync`
✅ Implemented

## POST /api/v4/teams/{team_id}/regenerate_invite_id - Regenerate the Invite ID from a Team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1033) | `—`
❌ Not implemented

## POST /api/v4/teams/{team_id}/restore - Restore a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L349) | `—`
❌ Not implemented

## POST /api/v4/teams/members/invite - Add user to team from invite
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L672) | `—`
❌ Not implemented

## POST /api/v4/teams/search - Search teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L416) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id} - Update a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L133) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/members/{user_id}/roles - Update a team member roles
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1185) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/members/{user_id}/schemeRoles - Update the scheme-derived roles of a team member.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1239) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/patch - Patch a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L245) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/privacy - Update teams's privacy
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L297) | `—`
❌ Not implemented

## PUT /api/v4/teams/{team_id}/scheme - Set a team's scheme
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/teams.yaml#L1618) | `—`
❌ Not implemented

# Terms Of Service

## GET /api/v4/terms_of_service - Get latest terms of service
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/service_terms.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/terms_of_service - Creates a new terms of service
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/service_terms.yaml#L24) | `—`
❌ Not implemented

# Threads

## DELETE /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/following - Stop following a thread
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3614) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/threads - Get all threads that user is following
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3303) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id} - Get a thread followed by the user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3655) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/set_unread/{post_id} - Mark a thread that user is following as unread based on a post id
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3525) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/following - Start following a thread
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3574) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/{thread_id}/read/{timestamp} - Mark a thread that user is following read state to the timestamp
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3478) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/teams/{team_id}/threads/read - Mark all threads that user is following as read
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3443) | `—`
❌ Not implemented

# Uploads

## GET /api/v4/uploads/{upload_id} - Get an upload session
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/uploads.yaml#L56) | `—`
❌ Not implemented

## POST /api/v4/uploads - Create an upload
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/uploads.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/uploads/{upload_id} - Perform a file upload
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/uploads.yaml#L84) | `—`
❌ Not implemented

# Usage

## GET /api/v4/usage/posts - Get current usage of posts
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/usage.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/usage/storage - Get the total file storage usage for the instance in bytes.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/usage.yaml#L28) | `—`
❌ Not implemented

## GET /api/v4/usage/teams - Get current usage of teams
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/usage.yaml#L54) | `—`
❌ Not implemented

# Users

## DELETE /api/v4/users - Permanent delete all users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L666) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id} - Deactivate a user account.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1157) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/channels/{channel_id}/drafts - Delete synced draft
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3769) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/channels/{channel_id}/drafts/{thread_id} - Delete synced thread draft
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3806) | `—`
❌ Not implemented

## DELETE /api/v4/users/{user_id}/image - Delete user's profile image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1434) | `—`
❌ Not implemented

## GET /api/v4/limits/server - Gets the server limits for the server
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/limits.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/users - Get users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L475) | `IMattermostClient.GetUsersAsync`
✅ Implemented

## GET /api/v4/users/{user_id} - Get a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1060) | `IMattermostClient.GetUserAsync`, `IMattermostClient.GetMeAsync`
✅ Implemented

## GET /api/v4/users/{user_id}/audits - Get user's audits
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2167) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/channel_members - Get all channel members from all teams for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3160) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/image - Get user's profile image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1352) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/image/default - Return user's default (generated) profile image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1473) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/sessions - Get user's sessions
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1964) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/teams/{team_id}/drafts - Get synced drafts for a team
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3730) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/terms_of_service - Fetches user's latest terms of service action if the latest action was
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3011) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/tokens - Get user access tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2477) | `—`
❌ Not implemented

## GET /api/v4/users/{user_id}/uploads - Get uploads for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3125) | `—`
❌ Not implemented

## GET /api/v4/users/auth_data - Get a user by auth data
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1541) | `—`
❌ Not implemented

## GET /api/v4/users/autocomplete - Autocomplete users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L881) | `—`
❌ Not implemented

## GET /api/v4/users/email/{email} - Get a user by email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1929) | `IMattermostClient.GetUserByEmailAsync`
✅ Implemented

## GET /api/v4/users/invalid_emails - Get users with invalid emails
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3949) | `—`
❌ Not implemented

## GET /api/v4/users/known - Get user IDs of known users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L935) | `—`
❌ Not implemented

## GET /api/v4/users/sessions/attributes/manifest - Get the session attributes manifest
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2128) | `—`
❌ Not implemented

## GET /api/v4/users/stats - Get total count of users in the system
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L959) | `—`
❌ Not implemented

## GET /api/v4/users/stats/filtered - Get total count of users in the system matching the specified filters
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L984) | `—`
❌ Not implemented

## GET /api/v4/users/tokens - Get user access tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2528) | `—`
❌ Not implemented

## GET /api/v4/users/tokens/{token_id} - Get a user access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2690) | `—`
❌ Not implemented

## GET /api/v4/users/tokens/non_compliant/count - Count non-compliant personal access tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2615) | `—`
❌ Not implemented

## GET /api/v4/users/username/{username} - Get a user by username
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1508) | `IMattermostClient.GetUserByUsernameAsync`
✅ Implemented

## POST /api/v4/drafts - Upsert synced draft
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3696) | `—`
❌ Not implemented

## POST /api/v4/users - Create a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L397) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/demote - Demote a user to a guest
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1727) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/email/verify/member - Verify user email by ID
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2199) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/image - Set user's profile image
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1387) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/mfa/generate - Generate MFA secret
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1683) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/promote - Promote a guest to user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1766) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/reset_failed_attempts - Reset the failed password attempts for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3992) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/sessions/revoke - Revoke a user session
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1999) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/sessions/revoke/all - Revoke all active sessions for a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2043) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/terms_of_service - Records user action when they accept or decline custom terms of service
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2955) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/tokens - Create a user access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2429) | `—`
❌ Not implemented

## POST /api/v4/users/{user_id}/typing - Publish a user typing websocket event.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3080) | `IMattermostClient.SendTypingAsync`
✅ Implemented

## POST /api/v4/users/email/verify - Verify user email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2233) | `—`
❌ Not implemented

## POST /api/v4/users/email/verify/send - Send verification email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2264) | `—`
❌ Not implemented

## POST /api/v4/users/group_channels - Get users by group channels ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L726) | `—`
❌ Not implemented

## POST /api/v4/users/ids - Get users by ids
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L683) | `IMattermostClient.GetUsersByIdsAsync`
✅ Implemented

## POST /api/v4/users/login - Login to Mattermost server
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2) | `IMattermostClient.LoginAsync`
✅ Implemented

## POST /api/v4/users/login/cws - Auto-Login to Mattermost server using CWS token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L87) | `—`
❌ Not implemented

## POST /api/v4/users/login/desktop_token - Login using desktop token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L50) | `—`
❌ Not implemented

## POST /api/v4/users/login/sso/code-exchange - Exchange SSO login code for session tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L121) | `—`
❌ Not implemented

## POST /api/v4/users/login/switch - Switch login method
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2299) | `—`
❌ Not implemented

## POST /api/v4/users/login/type - Get login authentication type
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2381) | `—`
❌ Not implemented

## POST /api/v4/users/logout - Logout from the Mattermost server
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L317) | `IMattermostClient.LogoutAsync`
✅ Implemented

## POST /api/v4/users/migrate_auth/ldap - Migrate user accounts authentication type to LDAP.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3207) | `—`
❌ Not implemented

## POST /api/v4/users/migrate_auth/saml - Migrate user accounts authentication type to SAML.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3255) | `—`
❌ Not implemented

## POST /api/v4/users/notify-admin - Save notify-admin intent
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L338) | `—`
❌ Not implemented

## POST /api/v4/users/password/reset - Reset password
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1589) | `—`
❌ Not implemented

## POST /api/v4/users/password/reset/send - Send password reset email
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1890) | `—`
❌ Not implemented

## POST /api/v4/users/search - Search users
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L800) | `IMattermostClient.SearchUsersAsync`
✅ Implemented

## POST /api/v4/users/sessions/revoke/all - Revoke all sessions from all users.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L3054) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/disable - Disable personal access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2729) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/enable - Enable personal access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2771) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/non_compliant/revoke - Revoke non-compliant personal access tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2650) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/revoke - Revoke a user access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2574) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/rotate - Rotate a personal access token
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2812) | `—`
❌ Not implemented

## POST /api/v4/users/tokens/search - Search tokens
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2866) | `—`
❌ Not implemented

## POST /api/v4/users/trigger-notify-admin-posts - Trigger notify-admin posts
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L366) | `—`
❌ Not implemented

## POST /api/v4/users/usernames - Get users by usernames
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L768) | `IMattermostClient.GetUsersByUsernamesAsync`
✅ Implemented

## POST /oauth/intune - Login with Microsoft Intune MAM
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L178) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id} - Update a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1089) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/active - Activate or deactivate a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1303) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/auth - Update a user's authentication method
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2906) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/mfa - Update a user's MFA
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1631) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/password - Update a user's password
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1841) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/patch - Patch a user
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1195) | `—`
❌ Not implemented

## PUT /api/v4/users/{user_id}/roles - Update a user's roles
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L1257) | `—`
❌ Not implemented

## PUT /api/v4/users/sessions/device - Attach mobile device and extra props to the session object
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/users.yaml#L2078) | `—`
❌ Not implemented

# Views

## DELETE /api/v4/channels/{channel_id}/views/{view_id} - Delete a channel view
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L243) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/views - List channel views
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L2) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/views/{view_id} - Get a channel view
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L145) | `—`
❌ Not implemented

## GET /api/v4/channels/{channel_id}/views/{view_id}/posts - Get posts for a view
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L289) | `—`
❌ Not implemented

## PATCH /api/v4/channels/{channel_id}/views/{view_id} - Update a channel view
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L190) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/views - Create channel view
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L73) | `—`
❌ Not implemented

## POST /api/v4/channels/{channel_id}/views/{view_id}/sort_order - Update a channel view's sort order
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/views.yaml#L361) | `—`
❌ Not implemented

# Webhooks

## DELETE /api/v4/hooks/incoming/{hook_id} - Delete an incoming webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L145) | `—`
❌ Not implemented

## DELETE /api/v4/hooks/outgoing/{hook_id} - Delete an outgoing webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L409) | `—`
❌ Not implemented

## GET /api/v4/hooks/incoming - List incoming webhooks
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L59) | `—`
❌ Not implemented

## GET /api/v4/hooks/incoming/{hook_id} - Get an incoming webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L112) | `—`
❌ Not implemented

## GET /api/v4/hooks/outgoing - List outgoing webhooks
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L323) | `—`
❌ Not implemented

## GET /api/v4/hooks/outgoing/{hook_id} - Get an outgoing webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L376) | `—`
❌ Not implemented

## POST /api/v4/hooks/incoming - Create an incoming webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L2) | `—`
❌ Not implemented

## POST /api/v4/hooks/outgoing - Create an outgoing webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L248) | `—`
❌ Not implemented

## POST /api/v4/hooks/outgoing/{hook_id}/regen_token - Regenerate the token for the outgoing webhook.
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L502) | `—`
❌ Not implemented

## PUT /api/v4/hooks/incoming/{hook_id} - Update an incoming webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L178) | `—`
❌ Not implemented

## PUT /api/v4/hooks/outgoing/{hook_id} - Update an outgoing webhook
[Mattermost API](https://github.com/mattermost/mattermost/blob/18c34098b13ba9b78c9613c517b913ef0a330650/api/v4/source/webhooks.yaml#L442) | `—`
❌ Not implemented
