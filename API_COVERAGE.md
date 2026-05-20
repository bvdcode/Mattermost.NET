# Mattermost.NET API Coverage

This document tracks which parts of the official Mattermost HTTP/WebSocket API are currently wrapped by Mattermost.NET.

Official references:

- Mattermost API documentation: https://developers.mattermost.com/api-documentation/
- Mattermost API source definitions: https://github.com/mattermost/mattermost/tree/master/api/v4/source

Last reviewed: 2026-04-25

## Legend

| Status | Meaning |
|---|---|
| ✅ Implemented | Public Mattermost.NET method exists and maps to the Mattermost endpoint. |
| 🟡 Partially implemented | Some useful endpoints or events are wrapped, but the official API group is not complete. |
| ❌ Not implemented | No public wrapper currently exists. |
| ⚠️ Non-core / plugin | Implemented endpoint is outside the official `/api/v4` REST API surface. |

## Current coverage summary

| Official API area | Coverage | Notes |
|---|---:|---|
| Authentication / Sessions | 🟡 Partial | Login, logout, and token-based client initialization are supported. Full session management is not wrapped. |
| Users | 🟡 Partial | Current user, user by ID, username, and email are supported. User creation, update, search, roles, preferences, status REST endpoints, etc. are not fully wrapped. |
| Teams | 🟡 Partial | Get team by ID is supported. Team creation, listing, team membership, team stats, search, patch/update, and delete are not wrapped. |
| Channels | 🟡 Partial | Core channel read/create/archive/member operations are supported. Many channel management endpoints are still missing. |
| Posts | 🟡 Partial | Create, get, patch, delete, channel posts, and thread posts are supported. Search, pinning, reactions, files-for-post, drafts/scheduled posts, etc. are not wrapped. |
| Files | 🟡 Partial | Upload, download, stream download, and file info are supported. Link previews, thumbnails/previews, public links, search, and some file metadata endpoints are not wrapped. |
| WebSocket | 🟡 Partial | WebSocket connection/authentication and generic event dispatch are supported. Convenience handlers exist for posted messages and status changes. |
| Calls plugin | ⚠️ Non-core | Calls plugin channel state is supported through `/plugins/com.mattermost.calls/...`; this is not a core `/api/v4` endpoint. |
| Webhooks | ❌ Not implemented | Incoming/outgoing webhook management is not wrapped. |
| Commands | ❌ Not implemented | Slash command management/execution endpoints are not wrapped. |
| Bots | ❌ Not implemented | Bot account management endpoints are not wrapped. |
| Preferences | ❌ Not implemented | User preferences endpoints are not wrapped. |
| Reactions | ❌ Not implemented | Post reaction endpoints are not wrapped. |
| Emoji | ❌ Not implemented | Custom emoji endpoints are not wrapped. |
| Status | ❌ Not implemented | REST status endpoints are not wrapped, although WebSocket status-change events are handled. |
| Plugins | ❌ Not implemented | General plugin management endpoints are not wrapped. |
| System / Config / Admin | ❌ Not implemented | Admin/system/config/license/logs/cluster/jobs/etc. endpoints are not wrapped. |
| Compliance / Audit / Reports | ❌ Not implemented | Admin/compliance/reporting areas are not wrapped. |
| OAuth / SAML / LDAP | ❌ Not implemented | Authentication provider administration endpoints are not wrapped. |
| Groups / Roles / Permissions / Schemes | ❌ Not implemented | Access-control administration endpoints are not wrapped. |
| Bookmarks / Shared Channels / Remote Clusters | ❌ Not implemented | Newer collaboration/federation areas are not wrapped. |

## Implemented endpoint map

### Authentication / Sessions

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `POST /api/v4/users/login` | `LoginAsync(string username, string password)` | ✅ | Logs in with username/email and password, stores returned token. |
| `POST /api/v4/users/logout` | `LogoutAsync()` | ✅ | Logs out and stops the WebSocket receiver. |
| Bearer/token auth for regular requests | Constructor with API key/token | ✅ | Supports bot tokens and personal access tokens through client initialization. |

### Users

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `GET /api/v4/users/me` | `GetMeAsync()` | ✅ | Also refreshes `CurrentUserInfo`. |
| `GET /api/v4/users/{user_id}` | `GetUserAsync(string userId)` | ✅ | Gets a user by ID. |
| `GET /api/v4/users/username/{username}` | `GetUserByUsernameAsync(string username)` | ✅ | Strips `@` from username input. |
| `GET /api/v4/users/email/{email}` | `GetUserByEmailAsync(string email)` | ✅ | Gets a user by email. |

### Teams

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `GET /api/v4/teams/{team_id}` | `GetTeamAsync(string teamId)` | ✅ | Gets a team by ID. |

### Channels

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `GET /api/v4/channels/{channel_id}` | `GetChannelAsync(string channelId)` | ✅ | Gets a channel by ID. |
| `POST /api/v4/channels` | `CreateChannelAsync(...)` | ✅ | Supports open/private channel creation. |
| `DELETE /api/v4/channels/{channel_id}` | `ArchiveChannelAsync(string channelId)` | ✅ | Archives a channel. |
| `POST /api/v4/channels/direct` | `CreateDirectChannelAsync(...)` | ✅ | Supports direct-channel creation with current user or two explicit users. |
| `POST /api/v4/channels/group` | `CreateGroupChannelAsync(params string[] userIds)` | ✅ | Requires at least two user IDs. |
| `POST /api/v4/channels/{channel_id}/members` | `AddUserToChannelAsync(string channelId, string userId)` | ✅ | Adds a user to a channel. |
| `DELETE /api/v4/channels/{channel_id}/members/{user_id}` | `DeleteUserFromChannelAsync(string channelId, string userId)` | ✅ | Removes a user from a channel. |
| `GET /api/v4/teams/{team_id}/channels/name/{channel_name}` | `FindChannelByNameAsync(..., isTeamId: true, ...)` | ✅ | Returns `null` on 404. Supports `include_deleted`. |
| `GET /api/v4/teams/name/{team_name}/channels/name/{channel_name}` | `FindChannelByNameAsync(..., isTeamId: false, ...)` | ✅ | Returns `null` on 404. Supports `include_deleted`. |
| `GET /api/v4/channels/{channel_id}/posts` | `GetChannelPostsAsync(...)` | ✅ | Implemented in post methods but endpoint belongs to channels/posts. Supports paging, before/after, deleted flag, and since timestamp. |

### Posts

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `POST /api/v4/posts` | `CreatePostAsync(...)` | ✅ | Supports message, root/reply ID, file IDs, priority metadata, and typed `PostProps`, including interactive buttons and select menus. |
| `POST /api/v4/posts` | `CreatePostWithRawPropsAsync(...)` | ✅ | Same endpoint, but accepts raw props dictionary. |
| `GET /api/v4/posts/{post_id}` | `GetPostAsync(string postId)` | ✅ | Gets a post by ID. |
| `PUT /api/v4/posts/{post_id}/patch` | `UpdatePostAsync(...)` | ✅ | Patch-style update for message and typed props. |
| `PUT /api/v4/posts/{post_id}/patch` | `UpdatePostWithRawPropsAsync(...)` | ✅ | Patch-style update for message and raw props dictionary. |
| `DELETE /api/v4/posts/{post_id}` | `DeletePostAsync(string postId)` | ✅ | Deletes a post by ID. |
| `GET /api/v4/posts/{post_id}/thread` | `GetThreadPostsAsync(string postId, string? fromPostId = null)` | ✅ | Supports optional `fromPost`. |

### Files

| Mattermost endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `POST /api/v4/files?channel_id={channel_id}` | `UploadFileAsync(string channelId, string filePath, ...)` | ✅ | Uploads file from local path. |
| `POST /api/v4/files?channel_id={channel_id}` | `UploadFileAsync(string channelId, string fileName, Stream stream, ...)` | ✅ | Uploads file from stream. Supports progress callback. |
| `GET /api/v4/files/{file_id}` | `GetFileAsync(string fileId)` | ✅ | Downloads file bytes. |
| `GET /api/v4/files/{file_id}` | `GetFileStreamAsync(string fileId)` | ✅ | Downloads file as stream. |
| `GET /api/v4/files/{file_id}/info` | `GetFileDetailsAsync(string fileId)` | ✅ | Gets file metadata. |

### WebSocket

| Mattermost feature | Mattermost.NET surface | Status | Notes |
|---|---|---:|---|
| `GET /api/v4/websocket` | `StartReceivingAsync(...)` | ✅ | Connects and starts receive loop. |
| WebSocket authentication challenge | Internal auth request | ✅ | Sends token during WebSocket auth challenge. |
| Stop WebSocket receive loop | `StopReceivingAsync()` | ✅ | Closes and recreates internal socket. |
| Generic WebSocket events | `OnEventReceived` | ✅ | Exposes raw/parsed event wrapper. |
| Posted message events | `OnMessageReceived` | ✅ | Convenience event for `posted`. Respects `MattermostClientOptions`. |
| User status-change events | `OnStatusUpdated` | ✅ | Convenience event for `status_change`. |
| Connection lifecycle | `OnConnected`, `OnDisconnected` | ✅ | Exposes connect/disconnect notifications. |
| Known event enum values | `MattermostEvent` | 🟡 Partial | Includes `posted`, `status_change`, `typing`, `multiple_channels_viewed`, `preferences_changed`, `sidebar_category_updated`, `user_added`, `ephemeral_message`, and `user_updated`. Other events fall back to `Unknown` or generic handling. |

### Calls plugin

| Endpoint | Mattermost.NET method | Status | Notes |
|---|---|---:|---|
| `POST /plugins/com.mattermost.calls/{channel_id}` | `SetChannelCallStateAsync(string channelId, bool isCallsEnabled)` | ⚠️ | Plugin endpoint, not part of the core `/api/v4` REST API. Keep this separate from official API coverage. |

## Backlog tracker by official API group

This section is intentionally broad. Use it to track future work without pretending the library wraps the entire Mattermost API.

| API group | Status | Useful next targets |
|---|---:|---|
| `users` | 🟡 Partial | List users, search users, update user, patch user, deactivate/reactivate, user teams/channels, roles, status REST endpoints. |
| `teams` | 🟡 Partial | Get team by name, list teams, create team, update/patch team, delete team, team members, team stats, team search. |
| `channels` | 🟡 Partial | List team channels, list user channels, update/patch channel, restore channel, channel members, channel stats, unread counts, channel search. |
| `posts` | 🟡 Partial | Search posts, pin/unpin, get file infos for post, get posts around post, get flagged posts. |
| `files` | 🟡 Partial | Thumbnail/preview/link endpoints, public links, file search, file deletion if supported by server version. |
| `status` | ❌ Not implemented | REST get/set user status. |
| `preferences` | ❌ Not implemented | Get/update/delete user preferences. |
| `reactions` | ❌ Not implemented | Save/delete/list reactions for posts. |
| `emoji` | ❌ Not implemented | Create/list/delete custom emoji. |
| `webhooks` | ❌ Not implemented | Incoming/outgoing webhook CRUD. |
| `commands` | ❌ Not implemented | Slash command CRUD and execution. |
| `bots` | ❌ Not implemented | Bot account CRUD, assign tokens, disable/enable bots. |
| `bookmarks` | ❌ Not implemented | Channel bookmarks. |
| `scheduled_post` | ❌ Not implemented | Scheduled posts. |
| `uploads` | ❌ Not implemented | Upload sessions / newer upload APIs. |
| `plugins` | ❌ Not implemented | Plugin install/enable/disable/config APIs. |
| `system`, `config`, `cluster`, `logs`, `metrics`, `jobs` | ❌ Not implemented | Admin/system operations. Probably not a priority for a general bot/client wrapper. |
| `groups`, `roles`, `permissions`, `schemes`, `access_control` | ❌ Not implemented | Enterprise/admin access-control APIs. |
| `oauth`, `saml`, `ldap` | ❌ Not implemented | Identity-provider administration. Probably low priority unless library expands into admin SDK territory. |
| `compliance`, `audit_logging`, `reports`, `exports`, `dataretention` | ❌ Not implemented | Compliance/admin reporting areas. Low priority for normal bot/client usage. |
| `sharedchannels`, `remoteclusters` | ❌ Not implemented | Federation/shared-channel APIs. |
| `actions`, `views`, `properties`, `custom_profile_attributes`, `content_flagging`, `recaps`, `ai`, `agents` | ❌ Not implemented | Newer/specialized API areas. Track only if users request them. |

## Suggested implementation priorities

1. **Users + Teams + Channels basic list/search methods** — these make the client much more usable without turning it into a huge admin SDK.
2. **Post search + reactions + pin/unpin** — common bot workflows need these often.
3. **Webhooks + slash commands** — useful for integrations, but may be separate from the WebSocket client story.
4. **Status REST endpoints** — small surface and matches the existing WebSocket status event support.
5. **Admin/system APIs** — only implement on demand; wrapping all of Mattermost admin API would massively increase maintenance burden.

## Maintenance notes

- Keep this file manual and honest. Do not claim full support for an API group unless most endpoints in that group are wrapped.
- Prefer endpoint-level tracking for implemented features and group-level tracking for backlog.
- When adding a new client method, add one row in the implemented endpoint map and update the coverage summary if needed.
- Plugin endpoints should stay under their own section instead of being counted as official `/api/v4` coverage.
- If generated clients are ever considered, keep them separate from the hand-written ergonomic client API.
