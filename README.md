[![GitHub](https://img.shields.io/github/license/bvdcode/Mattermost.NET)](https://github.com/bvdcode/Mattermost.NET/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/dt/Mattermost.NET?color=%239100ff)](https://www.nuget.org/packages/Mattermost.NET/)
[![Static Badge](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/Mattermost.NET)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bvdcode/Mattermost.NET/.github%2Fworkflows%2Fpublish-release.yml)](https://github.com/bvdcode/Mattermost.NET/actions)
[![NuGet version (Mattermost.NET)](https://img.shields.io/nuget/v/Mattermost.NET.svg?label=stable)](https://www.nuget.org/packages/Mattermost.NET/)
[![CodeFactor](https://www.codefactor.io/repository/github/bvdcode/Mattermost.NET/badge)](https://www.codefactor.io/repository/github/bvdcode/Mattermost.NET)
![GitHub repo size](https://img.shields.io/github/repo-size/bvdcode/Mattermost.NET)

<a id="readme-top"></a>

# Mattermost.NET

Mattermost.NET is a ready-to-use .NET Standard library for building Mattermost bots and integrations in C#.

It provides a clean, strongly typed wrapper around the Mattermost API, including messages, channels, users, file uploads, post props, and real-time WebSocket events. The client supports token-based authentication, username/password login, automatic WebSocket reconnects, custom `HttpClient` transport, and configurable incoming message filtering.

For a detailed endpoint map and implementation status, see [API coverage](https://github.com/bvdcode/Mattermost.NET/blob/main/API_COVERAGE.md).

---

# Installation

Install the package from NuGet:

```bash
dotnet add package Mattermost.NET
```

---

# Quick start

```csharp
using Mattermost;

const string server = "https://mm.your-server.com";
const string token = "your-personal-access-token-or-bot-token";
const string channelId = "target-channel-id";

using var client = new MattermostClient(server, token);

await client.CreatePostAsync(channelId, "Hello from Mattermost.NET!");
```

---

# Client interfaces

`IMattermostClient` combines the complete API and connection management. A service can instead depend on the narrower `IMattermostUsersClient`, `IMattermostPostsClient`, `IMattermostChannelsClient`, `IMattermostTeamsClient`, `IMattermostFilesClient`, `IMattermostIntegrationsClient`, `IMattermostEmojisClient`, or `IMattermostSystemClient` contract.

```csharp
using Mattermost;

using MattermostClient client = new MattermostClient(server, token);
IMattermostPostsClient posts = client;
await posts.CreatePostAsync(channelId, "Hello!");
```

All contracts refer to the same client instance, authentication state and HTTP transport. The owning application disposes the `MattermostClient`; no separate client instances are needed for different API groups.

---

# Authentication

## Use a personal access token or bot token

```csharp
using Mattermost;

const string server = "https://mm.your-server.com";
const string token = "your-personal-access-token-or-bot-token";

using var client = new MattermostClient(server, token);

var me = await client.GetMeAsync();
Console.WriteLine($"Authenticated as @{me.Username}");
```

When a token is provided through the constructor, Mattermost.NET validates and caches the current user on the first authorized API call.

## Use username and password

```csharp
using Mattermost;

const string server = "https://mm.your-server.com";

using var client = new MattermostClient(server);

var me = await client.LoginAsync("username-or-email", "password");
Console.WriteLine($"Authenticated as @{me.Username}");
```

You cannot use `LoginAsync` on a client that was constructed with an API token.

---

# Receiving real-time events

Call `StartReceivingAsync` to connect to the Mattermost WebSocket API and receive events.

```csharp
using Mattermost;

const string server = "https://mm.your-server.com";
const string token = "your-personal-access-token-or-bot-token";

using var client = new MattermostClient(server, token);

client.OnConnected += (_, e) =>
{
    Console.WriteLine($"Connected to {e.Uri}");
};

client.OnDisconnected += (_, e) =>
{
    Console.WriteLine($"Disconnected: {e.CloseStatusDescription}");
};

client.OnLogMessage += (_, e) =>
{
    Console.WriteLine(e.Message);
};

client.OnMessageReceived += (_, e) =>
{
    string text = e.Message.Post.Text ?? string.Empty;

    if (string.Equals(text, "ping", StringComparison.OrdinalIgnoreCase))
    {
        _ = e.Client.CreatePostAsync(e.Message.Post.ChannelId, "pong");
    }
};

await client.StartReceivingAsync();

Console.WriteLine("Bot is running. Press Enter to stop.");
Console.ReadLine();

await client.StopReceivingAsync();
```

The client automatically reconnects when the WebSocket connection is lost. You only need `StartReceivingAsync` when you want to receive WebSocket events; regular REST API calls work without it.

## Typing notifications

Notify a channel or thread that the authenticated user or bot is typing:

```csharp
await client.SendTypingAsync(channelId, cancellationToken: cancellationToken);
await client.SendTypingAsync(channelId, parentId: rootPostId, cancellationToken: cancellationToken);
```

This uses Mattermost's [REST typing endpoint](https://api.mattermost.com/#tag/users/operation/PublishUserTyping), available in server version 5.26 and later. It works without `StartReceivingAsync` or a WebSocket connection. Omit `parentId` for a channel notification, or supply the thread's root post ID.

Each call publishes one notification; repeat calls while typing continues. Completion means the server accepted the request, not that a recipient displayed the indicator. Server errors use the usual `MattermostClientException`, and cancellation also covers automatic token authentication.

---

# Incoming message filtering

By default, `MattermostClient` ignores messages authored by the currently authorized user. This prevents common bot loops where a bot reacts to its own posts.

```csharp
client.Options.IgnoreOwnMessages = true; // default
```

To receive the bot's own messages too, disable this option:

```csharp
client.Options.IgnoreOwnMessages = false;
```

`MessageEventArgs.IsCurrentUser` tells whether the received message was authored by the currently authorized user.

```csharp
client.OnMessageReceived += (_, e) =>
{
    if (e.IsCurrentUser)
    {
        Console.WriteLine("Received my own message.");
    }
};
```

You can also provide a custom incoming message filter. Return `true` to dispatch `OnMessageReceived`; return `false` to suppress the event.

```csharp
client.Options.IncomingMessageFilter = e =>
{
    string text = e.Message.Post.Text ?? string.Empty;
    return text.StartsWith("!", StringComparison.Ordinal);
};
```

The custom filter runs after the built-in own-message filter. If you want the custom filter to evaluate own messages, set `IgnoreOwnMessages` to `false`.

```csharp
client.Options.IgnoreOwnMessages = false;
client.Options.IncomingMessageFilter = e => !e.IsCurrentUser;
```

---

# Using a custom HttpClient

You can pass your own `HttpClient` when you need custom transport behavior, such as a proxy, timeout, custom handler, logging handler, or `IHttpClientFactory` integration.

```csharp
using Mattermost;
using System.Net;
using System.Net.Http;

const string server = "https://mm.your-server.com";
const string token = "your-personal-access-token-or-bot-token";

var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://corp-proxy:8080")
};

using var httpClient = new HttpClient(handler)
{
    Timeout = TimeSpan.FromSeconds(20)
};

using var client = new MattermostClient(server, token, httpClient);

var me = await client.GetMeAsync();
Console.WriteLine($"Authenticated as @{me.Username}");
```

When an external `HttpClient` is provided, Mattermost.NET uses it only as transport and does not dispose it. The Mattermost server URL still comes from `server` / `serverUri`; `HttpClient.BaseAddress` is not used as the Mattermost server identity.

Mattermost.NET sends authentication per request and does not mutate `HttpClient.DefaultRequestHeaders.Authorization`. Any default headers configured by the caller remain owned by the caller.

Available constructors:

```csharp
new MattermostClient();
new MattermostClient(string serverUrl);
new MattermostClient(Uri serverUri);
new MattermostClient(string serverUrl, string apiKey);
new MattermostClient(Uri serverUri, string apiKey);
new MattermostClient(string serverUrl, HttpClient httpClient);
new MattermostClient(Uri serverUri, HttpClient httpClient);
new MattermostClient(string serverUrl, string apiKey, HttpClient httpClient);
new MattermostClient(Uri serverUri, string apiKey, HttpClient httpClient);
```

---

# Common operations

## Send a message

```csharp
await client.CreatePostAsync(channelId, "Hello, World!");
```

## Send a priority message

```csharp
using Mattermost.Enums;

await client.CreatePostAsync(
    channelId,
    "@username Please acknowledge this incident.",
    priority: MessagePriority.Urgent,
    requestedAck: true,
    persistentNotifications: true);
```

Acknowledgement requests require `Important` or `Urgent` priority. Persistent notifications require `Urgent` priority and may also depend on the Mattermost server license, configuration, and mention rules.

## Reply to a thread

```csharp
await client.CreatePostAsync(
    channelId: channelId,
    message: "Thread reply",
    replyToPostId: rootPostId);
```

## Edit a post

```csharp
await client.UpdatePostAsync(postId, "Updated message text");
```

## Partially update a post

```csharp
await client.PatchPostAsync(postId, isPinned: false);
await client.PatchPostAsync(postId, text: "Updated text", cancellationToken: cancellationToken);
await client.PatchPostAsync(postId, fileIds: Array.Empty<string>());
await client.PatchPostAsync(postId, props: new Dictionary<string, object>
{
    ["custom_key"] = "replacement value"
});
```

Only supplied, non-null parameters are sent; omitted fields remain unchanged. `null` means leave the field unchanged, not clear it. Empty text, an empty file list, an empty props dictionary, and `false` are sent as explicit values. Server validation and editing restrictions still apply.

Supplied `props` replace the property bag rather than merging individual keys. Include any existing properties you need to preserve. `hasReactions` changes the post's reaction flag, not individual reactions; use `AddReactionAsync` and `RemoveReactionAsync` to manage reactions.

See Mattermost's [patch post documentation](https://docs.mattermost.com/api/reference/patch-post) for the endpoint contract and permissions.

## Delete a post

```csharp
await client.DeletePostAsync(postId);
```

## Upload a file and attach it to a post

```csharp
var file = await client.UploadFileAsync(channelId, "report.pdf", stream);

await client.CreatePostAsync(
    channelId: channelId,
    message: "Uploaded report",
    files: new[] { file.Id });
```

## Read channel posts

```csharp
var posts = await client.GetChannelPostsAsync(channelId, perPage: 60);

foreach (var post in posts.Posts.Values)
{
    Console.WriteLine(post.Text);
}
```

## Read selected posts and attachments

```csharp
var selected = await client.GetPostsByIdsAsync(new[] { firstPostId, secondPostId }, cancellationToken);
var pinned = await client.GetPinnedPostsAsync(channelId, cancellationToken);
var files = await client.GetPostFilesAsync(postId, cancellationToken: cancellationToken);
```

Bulk lookup accepts 1–1000 post IDs. Results may omit missing or inaccessible posts and do not preserve the input order; match them by `Post.Id`. Pinned posts use the same `Order` and `Posts` structure as `GetChannelPostsAsync`.

`GetPostFilesAsync` returns file metadata, not file contents. Use `GetFileAsync(file.Id)` or `GetFileStreamAsync(file.Id)` to download an attachment. Its optional `includeDeleted: true` flag requires system administrator permissions.

## Post history, location and reminders

```csharp
var history = await client.GetPostEditHistoryAsync(postId, cancellationToken);
var location = await client.GetPostInfoAsync(postId, cancellationToken);
await client.RestorePostVersionAsync(postId, historicalVersionId, cancellationToken);
var readState = await client.MarkPostUnreadAsync("me", postId, cancellationToken: cancellationToken);
await client.SetPostReminderAsync("me", postId, DateTimeOffset.UtcNow.AddHours(1), cancellationToken);
```

Edit history normally requires the post's author and `edit_post` permission. Each historical entry has its own `Id` and an `OriginalId` pointing to the current post; pass the historical `Id` to restore a version. A post without saved history may return HTTP 404. Restoration updates the current post and requires permission to edit it. Server retention and attachment permissions still apply.

`GetPostInfoAsync` returns `PostLocationInfo` from `Mattermost.Models.Posts`: channel/team identifiers, display names and the current user's membership flags, without the post's contents. This information does not grant access to the channel. Direct and group channels have no team information.

`MarkPostUnreadAsync` returns `ChannelReadState` from `Mattermost.Models.Channels`. Its `MessageCount` and `RootMessageCount` are counters at the updated read boundary, not unread totals; use `GetChannelUnreadAsync` for unread counts. `LastViewedAt` is Unix milliseconds. Set `collapsedThreadsSupported: true` when the caller supports collapsed-thread read states. Updating another user's state requires appropriate permissions.

`SetPostReminderAsync` requires access to the post and accepts a `DateTimeOffset`; it sends Unix seconds, discarding subsecond precision. Scheduling the same post for the same user replaces that user's reminder. Cancellation cannot undo a restoration or reminder already accepted by the server.

## Get current user

```csharp
var me = await client.GetMeAsync();
Console.WriteLine(me.Username);
```

## Bot accounts

```csharp
var bots = await client.GetBotsAsync(cancellationToken: cancellationToken);
var bot = await client.CreateBotAsync("notification-bot", "Notifications", cancellationToken: cancellationToken);
var details = await client.GetBotAsync(bot.UserId, cancellationToken: cancellationToken);
await client.PatchBotAsync(bot.UserId, description: "Channel notifications", cancellationToken: cancellationToken);
await client.DisableBotAsync(bot.UserId, cancellationToken);
await client.EnableBotAsync(bot.UserId, cancellationToken);
await client.AssignBotAsync(bot.UserId, ownerUserId, cancellationToken);
```

These methods require Mattermost 5.10+. `Bot` is in `Mattermost.Models.Bots`; its `UserId` identifies the associated user account, and its `OwnerId` may identify a user or plugin. Bot creation must be enabled and requires `create_bot`; bots cannot create other bots. The current user becomes the owner. Creation does not issue an API token.

Reading owned bots requires `read_bots`; reading others' bots also requires `read_others_bots`. Lists use zero-based pages and positive page sizes. `includeDeleted: true` includes disabled bots in list/single reads. `onlyOrphaned: true` filters to bots whose owner was deactivated. An inaccessible or missing bot may produce HTTP 404.

`PatchBotAsync` changes only supplied fields: null leaves a field unchanged, while an empty display name or description clears it. At least one field is required; usernames cannot be blank. Management methods require permission to manage the bot. Disabling preserves the account but prevents its credentials from authenticating; protected system bots may reject this operation. Assignment changes ownership to a non-bot user.

On Mattermost 5.26 or later, administrators with `manage_system` can convert accounts in either direction:

```csharp
Bot convertedBot = await client.ConvertUserToBotAsync(userId, cancellationToken);
User account = await client.ConvertBotToUserAsync(convertedBot.UserId, newPassword,
    email: "account@example.com", cancellationToken: cancellationToken);
```

Both methods retain the account identifier. User-to-bot conversion makes the account its own bot owner, clears external authentication credentials and revokes its sessions. Bot-to-user conversion sets the supplied password and removes bot metadata. Null `email` preserves the existing address. `setSystemAdmin: true` requests administrator privileges; the default does not demote an existing administrator. Use `PatchUserAsync` for other profile fields.

Live verification passed for both conversions on a dedicated account: preserved identifiers and profile fields, revoked pre-conversion session, login with the new password, replaced and omitted email, administrator-role readback and permission denial for a non-administrator. The account was retained without administrator privileges.

## Personal access tokens

```csharp
var createdToken = await client.CreateUserAccessTokenAsync(userId, "Notification service", cancellationToken);
var metadata = await client.GetUserAccessTokenAsync(createdToken.Id, cancellationToken);
var userTokens = await client.GetUserAccessTokensAsync(userId, cancellationToken: cancellationToken);
var allTokens = await client.GetAllUserAccessTokensAsync(cancellationToken: cancellationToken);
var matches = await client.SearchUserAccessTokensAsync(username, cancellationToken);
await client.DisableUserAccessTokenAsync(createdToken.Id, cancellationToken);
await client.EnableUserAccessTokenAsync(createdToken.Id, cancellationToken);
long policyViolations = await client.GetNonCompliantUserAccessTokenCountAsync(cancellationToken);
```

`UserAccessToken` is in `Mattermost.Models.Users`. `Id` is the metadata identifier used by management methods, not the authentication secret. Only creation returns `Token`; save that value securely without logging it. Metadata reads and searches omit it. `ExpiresAt` is Unix milliseconds, with zero indicating no expiry; an enabled token may still be expired.

Creation and user-specific reads require Mattermost 4.1+, token enable/disable 4.4+, server-wide lists and search 4.7+, and the policy count 11.1+. Lists use zero-based pages and positive page sizes. Creation requires `create_user_access_token`, metadata reads `read_user_access_token`, and disabling `revoke_user_access_token`, along with permission to edit the owning user or bot. Global lists, search and policy counts require `manage_system`. Personal tokens must be enabled for non-bot accounts. Creation requests no expiry; server lifetime policies may reject that request. OAuth sessions may not create or change token activation.

Run activation changes using another credential: disabling a token invalidates its sessions and prevents that token from re-enabling itself. Re-enabling restores activation but does not extend the expiry time. The policy count is read-only and returns zero when no maximum token lifetime is configured.

## Find users

```csharp
var byId = await client.GetUserAsync(userId);
var byUsername = await client.GetUserByUsernameAsync("username");
var byEmail = await client.GetUserByEmailAsync("user@example.com");
```

For bulk profile lookup, send IDs or usernames in one request:

```csharp
var users = await client.GetUsersByIdsAsync(userIds, cancellationToken: cancellationToken);
var namedUsers = await client.GetUsersByUsernamesAsync(new[] { "alice", "@bob" }, cancellationToken);
```

Collections must be nonempty and contain no blank entries. Results may omit missing or inaccessible users and do not preserve the input order; match profiles by `User.Id`. The optional `since` parameter of `GetUsersByIdsAsync` filters profiles updated after a Unix timestamp in milliseconds (requires Mattermost 5.14 or later). Zero disables filtering.

For suggestions, channel memberships and account activity:

```csharp
var knownIds = await client.GetKnownUserIdsAsync(cancellationToken);
var suggestions = await client.AutocompleteUsersAsync("alice", teamId, channelId, cancellationToken: cancellationToken);
long totalUsers = await client.GetTotalUserCountAsync(cancellationToken);
var groupMembers = await client.GetUsersByGroupChannelIdsAsync(groupChannelIds, cancellationToken);
var memberships = await client.GetUserChannelMembersAsync("me", page: 0, perPage: 60, cancellationToken: cancellationToken);
var sessions = await client.GetUserSessionsAsync("me", cancellationToken);
var audits = await client.GetUserAuditsAsync("me", page: 0, perPage: 60, cancellationToken: cancellationToken);
```

Known IDs are users sharing any channel with the current account, including direct and group channels (Mattermost 5.23+). Autocomplete does not search email addresses; an empty search string requests initial suggestions. A channel filter requires its team ID. `UserAutocomplete.Users` contains in-channel matches and `OutOfChannel` contains matches outside that channel; without a channel filter, `Users` is the main result list. Optional suggestion lists may be absent.

The user count is subject to server visibility restrictions. Group-channel lookup (Mattermost 5.14+) accepts 1–50 IDs, returns other members keyed by channel ID, excludes the current user, and omits channels the current user does not belong to. Membership listing (Mattermost 6.2+) always requests a JSON-array page, with team metadata on each `ChannelMemberWithTeamData`; increase `page` to retrieve more. Session and audit methods return metadata, not usable authentication tokens. Reading another user's memberships, sessions or audits requires `edit_other_users` permission.

## Read user presence

```csharp
var presence = await client.GetUserStatusAsync("me", cancellationToken);
var statuses = await client.GetUsersStatusesByIdsAsync(userIds, cancellationToken);
Console.WriteLine(presence.Status);
```

These methods return `UserPresence` with a typed `UserStatus` (`Online`, `Offline`, `Away`, `DoNotDisturb`, or `OutOfOffice`), `IsManual`, `LastActivityAt` (Unix milliseconds), and `DoNotDisturbEndTime` (Unix seconds; zero means no expiry). Bulk status lookup requires actual user IDs, not `"me"`; match responses by `UserId`, not list position. Server errors are propagated, including a missing single-user status.

## Update user presence and custom status

```csharp
using Mattermost.Models.Enums;

UserPresence presence = await client.UpdateUserStatusAsync("me", UserStatus.Away,
    cancellationToken: cancellationToken);
long dndExpiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
await client.UpdateUserStatusAsync("me", UserStatus.DoNotDisturb, dndExpiry, cancellationToken);

DateTimeOffset expiry = DateTimeOffset.UtcNow.AddHours(1);
await client.UpdateUserCustomStatusAsync("me", "coffee", "Taking a break",
    CustomStatusDuration.OneHour, expiry, cancellationToken);
await client.ClearUserCustomStatusAsync("me", cancellationToken);
await client.DeleteRecentUserCustomStatusAsync("me", "coffee", "Taking a break",
    CustomStatusDuration.OneHour, expiry, cancellationToken);
```

Presence updates accept `Online`, `Offline`, `Away` and `DoNotDisturb`; `OutOfOffice` is readable but cannot be set through this endpoint. Setting `Online` resumes automatic activity-based presence. DND expiry is in Unix seconds; zero means no expiry.

Custom statuses require the server feature to be enabled. Omitting `duration` and `expiresAt` creates a non-expiring status. Every other `CustomStatusDuration` requires an explicit future `expiresAt`; the duration is a label, not a calculation of the deadline. Updating another user requires `edit_other_users` permission. HTTP errors are propagated.

Clearing the current custom status preserves its recent history. History deletion uses the POST endpoint and must match the stored emoji, text, duration and expiry exactly. Strings are preserved without trimming; use the stored text if the server truncated it to its 100-character limit. Removing a recent entry does not clear the current status.

## Read teams and memberships

```csharp
IList<TeamMember> memberships = await client.GetUserTeamMembersAsync("me", cancellationToken);
IList<TeamMember> members = await client.GetTeamMembersAsync(teamId, page: 0, perPage: 100,
    sortByUsername: true, excludeDeletedUsers: true, cancellationToken: cancellationToken);
TeamMember member = await client.GetTeamMemberAsync(teamId, "me", cancellationToken);
IList<TeamMember> selectedMembers = await client.GetTeamMembersByIdsAsync(teamId, userIds, cancellationToken);
TeamStats stats = await client.GetTeamStatsAsync(teamId, cancellationToken);
bool exists = await client.TeamExistsAsync("team-name", cancellationToken);
TeamUnread unread = await client.GetTeamUnreadAsync(teamId, "me", cancellationToken);
IList<TeamUnread> otherTeamsUnread = await client.GetUserTeamsUnreadAsync("me", excludeTeamId: teamId,
    includeCollapsedThreads: true, cancellationToken: cancellationToken);
```

`TeamMember` describes membership and roles, not the user's profile. Use `GetUserAsync(member.UserId)` for a profile. Reading team members and statistics requires permission to view the team; server visibility restrictions still apply. The server may hide another member's role fields and return `-1` for `DeletedAt`. A missing single membership produces an API error.

Membership pages are zero-based and contain 1–200 entries. Bulk lookup takes a nonempty collection of actual user IDs, not `"me"`; match results by `UserId` because missing or inaccessible members may be omitted and input order is not preserved. `GetUserTeamMembersAsync` reads memberships across teams, whereas `GetUserTeamsAsync` returns the team objects themselves.

`TeamExistsAsync` uses the team's URL name, not its display name. It returns false for a missing team or a team hidden from the current user; HTTP errors are propagated.

`TeamStats` reports total and active membership counts. `TeamUnread` contains unread message, mention, root-post and followed-thread counters. All counts are 64-bit (`long`). Unread reads do not mark messages as read. `includeCollapsedThreads: true` requests followed-thread counts from servers supporting collapsed threads; reading another user's all-team unread counts requires system administrator permissions.

## Work with channels

```csharp
var channel = await client.GetChannelAsync(channelId);
var found = await client.FindChannelByNameAsync(teamId, "town-square");
var direct = await client.CreateDirectChannelAsync(userId);
```

## Read channels and memberships

```csharp
var channels = await client.GetUserChannelsAsync("me", cancellationToken: cancellationToken);
var members = await client.GetChannelMembersAsync(channelId, page: 0, perPage: 60, cancellationToken: cancellationToken);
var membership = await client.GetChannelMemberAsync(channelId, userId, cancellationToken);

IList<ChannelUserInfo> selectedMembers = await client.GetChannelMembersByIdsAsync(channelId, userIds, cancellationToken);
IList<Channel> teamChannels = await client.GetUserTeamChannelsAsync("me", teamId,
    includeDeleted: true, lastDeleteAt: 0, cancellationToken: cancellationToken);
IList<ChannelUserInfo> teamMemberships = await client.GetUserTeamChannelMembersAsync("me", teamId, cancellationToken);
ChannelStats stats = await client.GetChannelStatsAsync(channelId, cancellationToken: cancellationToken);
ChannelUnread unread = await client.GetChannelUnreadAsync(channelId, "me", cancellationToken);
IList<string> timezones = await client.GetChannelTimezonesAsync(channelId, cancellationToken);
```

`GetUserChannelsAsync` requires Mattermost 6.1 or later and returns the user's channels across all teams, including direct and group chats. Reading another user's channels requires `edit_other_users` permission. Archived channels are excluded by default; set `includeDeleted: true` to include them. The optional `lastDeleteAt` Unix timestamp in milliseconds filters archived channels only when that flag is enabled.

Membership reads require `read_channel` permission. Pages are zero-based and the page size must be 1–200. These methods return `ChannelUserInfo` (membership roles, counters, and notification settings), not user profiles; use `GetUserAsync(membership.UserId)` for the profile. A missing membership is reported as an API error, not `null`.

`ChannelUserInfo.LastViewedAt`, `MessageCount`, and `MentionCount` are 64-bit (`long`) values. The server may return `-1` for another member's `LastViewedAt` and `UpdatedAt` when those timestamps are hidden.

`GetChannelMembersByIdsAsync` takes a nonempty collection of actual user IDs, not `"me"`. Missing members may be omitted and input order is not preserved; match results by `UserId`.

`GetUserTeamChannelsAsync` returns the user's channels in one team, plus direct and group chats that have no team. It accepts the same archived-channel filters as `GetUserChannelsAsync`. `GetUserTeamChannelMembersAsync` returns membership records for those channels; reading another user's memberships requires system administrator permissions and permission to view the team.

`ChannelStats` contains active member and guest counts, pinned post count, and file count. Set `excludeFilesCount: true` to skip counting files; supporting servers return `-1` for `FilesCount`. `ChannelUnread` contains message, mention, root-post and urgent-mention counters. These counters are 64-bit (`long`). Reading unread counts does not mark messages as read and requires permission to access the user and read the channel.

`GetChannelTimezonesAsync` requires Mattermost 5.6 or later and `read_channel` permission. It returns timezone names as reported by the server, or an empty list when members have no timezone configured.

## Edit and find channels

```csharp
Channel channel = await client.GetChannelAsync(channelId);
channel.Header = "New header";
channel = await client.UpdateChannelAsync(channel, cancellationToken);
channel = await client.PatchChannelAsync(channelId, purpose: "", cancellationToken: cancellationToken);
channel = await client.UpdateChannelPrivacyAsync(channelId, ChannelType.Private, cancellationToken);
channel = await client.RestoreChannelAsync(channelId, cancellationToken);

IList<Channel> selected = await client.GetPublicChannelsByIdsAsync(teamId, channelIds, cancellationToken);
IList<Channel> matches = await client.SearchTeamChannelsAsync(teamId, "channel-name", cancellationToken);
IList<Channel> archived = await client.GetDeletedChannelsAsync(teamId, perPage: 100, cancellationToken: cancellationToken);
IList<Channel> privateChannels = await client.GetPrivateChannelsAsync(teamId, perPage: 100, cancellationToken: cancellationToken);
```

`UpdateChannelAsync` sends the channel's ID, name, display name, header, and purpose; other properties are not sent. Header and purpose replace the stored values. For a partial change, use `PatchChannelAsync`: `null` omits a field, while `""` clears header or purpose. Channel name and display-name validity are enforced by the server. Metadata edits require the appropriate public/private channel-management permission; direct and group channels have additional restrictions.

Privacy conversion accepts only `Public` and `Private`, requires Mattermost 5.16 or later and the corresponding conversion permission. Restoration requires permission to manage the channel's team. Archived-channel pages require `list_team_channels`; private-channel pages require `manage_system` and Mattermost 5.26 or later. Page sizes are 1–200.

Bulk lookup returns only active public channels in the requested team and requires `view_team`. Missing IDs may be omitted; a batch with no matches produces an API error. Team search matches channel name or display name; users without `list_team_channels` are limited to their memberships on Mattermost 5.16 or later. HTTP errors are propagated.

## Move and convert channels

```csharp
Channel moved = await client.MoveChannelAsync(channelId, destinationTeamId, cancellationToken: cancellationToken);
Channel converted = await client.ConvertGroupMessageToChannelAsync(groupChannelId, teamId,
    "project-chat", "Project chat", cancellationToken);
```

`MoveChannelAsync` requires Mattermost 5.26+ and `manage_system`. It moves a public or private channel, retaining its identifier and messages. Active members must belong to the destination team; `force: true` removes members who do not. The server may remove deactivated members even with `force: false`, and updates related threads, sidebar categories and channel webhooks. An error does not guarantee rollback of server changes.

`ConvertGroupMessageToChannelAsync` turns a group message into a private channel without copying it. Supply a team shared by all participants, plus the new channel name and display name. The caller must be a non-guest group member with `create_private_channel` permission in that team, and becomes channel administrator. IDs and names are trimmed; other validity rules are enforced by the server.

Live checks on Mattermost 12.0.0 passed conversion with preserved identity, creation time, participants and an existing message; private-channel movement without forced removal; public-channel movement with forced removal; native metadata readback; non-administrator move rejection; and rejection of conversion on a non-group channel.

## User preferences

```csharp
IList<Preference> preferences = await client.GetPreferencesAsync("me", cancellationToken);
IList<Preference> category = await client.GetPreferencesByCategoryAsync("me", "display_settings", cancellationToken);
Preference preference = await client.GetPreferenceAsync("me", "display_settings", "use_military_time", cancellationToken);
preference.Value = "true";
await client.UpdatePreferencesAsync("me", new[] { preference }, cancellationToken);
await client.DeletePreferencesAsync("me", new[] { preference }, cancellationToken);
```

`Preference` is in `Mattermost.Models.Users`. Updating or deleting preferences accepts 1–100 objects with the target user's actual `UserId`, even when the route uses `"me"`. Updates change only the supplied preference keys. Names and values are preserved, including empty names and whitespace in values; use category lookup for preferences with empty names. Deletion identifies preferences by user, category, and name and ignores their values. Reading another user's preferences or changing them requires `edit_other_users`. Missing category and single-preference errors are propagated.

## Read roles

```csharp
Role role = await client.GetRoleByNameAsync("channel_user", cancellationToken);
Role byId = await client.GetRoleAsync(role.Id, cancellationToken);
IList<Role> selectedRoles = await client.GetRolesByNamesAsync(new[] { "system_user", "channel_user" }, cancellationToken);
IList<Role> allRoles = await client.GetRolesAsync(cancellationToken);
```

Use `Mattermost.Models.Roles` for `Role`. It contains permission identifiers, built-in and scheme-management flags, an optional scheme ID, and 64-bit Unix-millisecond timestamps. Single and bulk lookups require authentication and Mattermost 4.9 or later. Reading all roles requires `manage_system` and Mattermost 5.33 or later. Bulk lookup accepts up to 100 distinct names; whitespace and duplicates are removed. Match responses by `Id` or `Name`, not input position. HTTP errors are propagated.

## Read custom emojis

```csharp
using Mattermost.Models.Emojis;

IList<Emoji> emojis = await client.GetEmojisAsync(page: 0, perPage: 100,
    sortByName: true, cancellationToken: cancellationToken);
Emoji emoji = await client.GetEmojiAsync(emojiId, cancellationToken);
Emoji namedEmoji = await client.GetEmojiByNameAsync(":party_parrot:", cancellationToken);
byte[] image = await client.GetEmojiImageAsync(namedEmoji.Id, cancellationToken);
IList<Emoji> selected = await client.GetEmojisByNamesAsync(new[] { "party_parrot", "custom_rocket" }, cancellationToken);
IList<Emoji> matches = await client.SearchEmojisAsync("parrot", prefixOnly: false, cancellationToken: cancellationToken);
IList<Emoji> suggestions = await client.AutocompleteEmojisAsync("party_", cancellationToken);
```

The lookup methods return metadata for custom emojis, not the built-in emoji catalog. `GetEmojiImageAsync` returns the original image bytes without decoding or converting them. All these methods require authentication and custom emojis enabled on the server. `Emoji` contains `Id`, `Name`, `CreatorId`, and 64-bit creation, update, and deletion timestamps in Unix milliseconds.

Pages are zero-based with 1–200 entries; the default page size is 60. Name sorting, lookup by name, search, and autocomplete require Mattermost 4.7 or later. Bulk name lookup requires Mattermost 9.2 or later and accepts up to 200 distinct names. Duplicate names are removed; missing and built-in names are omitted from the response. Match results by `Name` or `Id`, not input position.

Names and search terms have surrounding whitespace and colons removed; case is preserved. Search matches anywhere in the name unless `prefixOnly: true` is set and returns up to 200 results. Autocomplete always matches a prefix and returns up to 100 results. Both are sorted by name. A missing single emoji produces an API error; a search without matches returns an empty list. HTTP errors, including disabled custom emojis or unsupported endpoints, are propagated.

## Manage team lifecycle and memberships

```csharp
Team team = await client.GetTeamAsync(teamId);
team.Description = "Project discussion";
team = await client.UpdateTeamAsync(team, cancellationToken);
team = await client.UpdateTeamPrivacyAsync(teamId, TeamType.InviteOnly, cancellationToken);
IList<TeamMember> members = await client.AddTeamMembersAsync(teamId, new[] { userId }, cancellationToken);
await client.UpdateTeamMemberSchemeRolesAsync(teamId, userId, schemeUser: true, schemeAdmin: false,
    cancellationToken: cancellationToken);
await client.RemoveTeamMemberAsync(teamId, userId, cancellationToken);
await client.DeleteTeamAsync(teamId, cancellationToken: cancellationToken);
team = await client.RestoreTeamAsync(teamId, cancellationToken);
```

`UpdateTeamAsync` sends complete writable metadata; read the team first to avoid clearing fields. `UpdateTeamPrivacyAsync` changes privacy and open-invitation settings together. Team management requires `manage_team`, invitation changes also require `invite_user`, and membership role changes require `manage_team_roles`.

`DeleteTeamAsync` is a reversible soft deletion by default. `permanent: true` irreversibly removes the team and its data and requires the `EnableAPITeamDeletion` server setting. Restore requires Mattermost 5.24 or later. Removing a team member also removes their channel memberships.

Batch member addition accepts at most 256 actual user IDs, not `me`. Match returned memberships by `UserId`; an error does not guarantee rollback of users already added. This method uses the normal fail-on-error endpoint mode. `RegenerateTeamInviteIdAsync` returns the updated team and invalidates its previous invite ID. `GetTeamInviteInfoAsync` requires an open team and returns only `Id`, `Name`, `DisplayName` and `Description`, without authentication; it does not return complete metadata for an update. HTTP errors are propagated.

```csharp
TeamMember membership = await client.JoinTeamFromInviteAsync(inviteId: inviteId, cancellationToken: cancellationToken);
// For an email invitation, pass token instead of inviteId.
```

`JoinTeamFromInviteAsync` joins as the authenticated user and requires exactly one argument: a reusable team invitation ID or an email invitation token. Both are trimmed and query-escaped; the request has no JSON body. Guests cannot use reusable invitation IDs. The server validates invitation expiry, team restrictions and membership eligibility. Treat invitation tokens as secrets.

Live checks on Mattermost 12.0.0 passed reusable invitation acceptance by a previously non-member account, independent membership/team-list readback and the invalid-invitation error. Email-token request construction is covered by tests; accepting an actual email invitation was not live-tested.

## Update channel membership settings and read state

```csharp
await client.UpdateChannelMemberRolesAsync(channelId, userId, "channel_user channel_admin", cancellationToken);
await client.UpdateChannelMemberSchemeRolesAsync(channelId, userId, schemeUser: true, schemeAdmin: true,
    cancellationToken: cancellationToken);
await client.UpdateChannelMemberNotifyPropsAsync(channelId, "me",
    new Dictionary<string, string> { ["desktop"] = "mention" }, cancellationToken);
ChannelViewResponse viewed = await client.ViewChannelAsync("me", channelId,
    collapsedThreadsSupported: true, cancellationToken: cancellationToken);
ChannelViewResponse read = await client.MarkChannelsReadAsync("me", new[] { channelId }, cancellationToken);
IDictionary<string, long> counts = await client.GetChannelsMemberCountsAsync(new[] { channelId }, cancellationToken);
IList<Channel> groups = await client.SearchGroupChannelsAsync("member name", cancellationToken);
```

Role updates require `manage_channel_roles` in the channel. Scheme roles replace all supplied flags; `schemeGuest` and `schemeUser` are mutually exclusive. Notification updates change only supplied settings, whose values are strings. Unsupported keys may be ignored by the server.

Viewing a channel and marking channels read are writes, not metadata reads: they affect unread counters, push notifications and, where enabled, thread read state. `ViewChannelAsync` accepts an empty channel ID to report loss of focus; a null previous channel ID is omitted. `ChannelViewResponse` is in `Mattermost.Models.Responses`. View times and member counts are 64-bit values; times are Unix milliseconds, and the view-time map may be null when nothing was updated. Match bulk counts by channel ID; missing channels may be omitted. Group search matches member names in the current user's group channels and returns an empty list when nothing matches. HTTP errors are propagated.

## Manage sidebar categories

```csharp
using Mattermost.Models.Channels;

OrderedSidebarCategories sidebar = await client.GetSidebarCategoriesAsync("me", teamId, cancellationToken);
SidebarCategory category = await client.CreateSidebarCategoryAsync(new SidebarCategory
{
    UserId = client.CurrentUserInfo.Id,
    TeamId = teamId,
    Type = "custom",
    DisplayName = "Project channels",
    Sorting = "manual",
    ChannelIds = new List<string> { channelId }
}, cancellationToken);
category.Collapsed = true;
category = await client.UpdateSidebarCategoryAsync(category, cancellationToken);
IList<string> order = await client.GetSidebarCategoryOrderAsync("me", teamId, cancellationToken);
await client.UpdateSidebarCategoryOrderAsync("me", teamId, order, cancellationToken);
```

Sidebar category operations require Mattermost 5.26 or later and access to the team. Reads, ordering, and deletion accept the `me` alias; category objects must contain actual user and team identifiers. Accessing another user's sidebar requires `edit_other_users` permission.

`GetSidebarCategoriesAsync` returns an object containing `Categories` and `Order`. Updates replace writable settings of supplied categories: read them first to preserve settings and channel order. Bulk update leaves unspecified categories intact; moving a channel requires supplying both its source and destination categories. Category type and sort position are server-owned, and only custom categories can be renamed or deleted. Direct-message category channel IDs and mute settings cannot be replaced this way. Reordering requires every category ID exactly once, including built-in categories; these IDs are not necessarily ordinary Mattermost IDs. The server filters inaccessible channels, and changing a category's mute setting also affects its channels. An empty category may have a null `ChannelIds` list. HTTP errors are propagated.

## Work with calls

```csharp
bool callActive = await client.GetCallActiveAsync(channelId);

if (callActive)
{
    await client.EndCallAsync(channelId);
}
```

These methods require the Mattermost Calls plugin. `EndCallAsync` expects a channel identifier, despite the route parameter being named `call_id` by the plugin. Ending a call also requires host permissions.

---

# Post props and attachments

Mattermost.NET supports Mattermost post props, including attachments and interactive action metadata.

```csharp
using Mattermost.Models.Posts;

var props = new PostProps();
props.Attachments.Add(new PostPropsAttachment
{
    Text = "Attachment text"
});

await client.CreatePostAsync(
    channelId: channelId,
    message: "Message with props",
    props: props);
```

Raw props are also supported when you need to send a custom JSON property bag.

```csharp
var rawProps = new Dictionary<string, object>
{
    ["custom_key"] = "custom value"
};

await client.CreatePostWithRawPropsAsync(
    channelId: channelId,
    message: "Message with raw props",
    rawProps: rawProps);
```

## Interactive message buttons and menus

Message actions support Mattermost buttons and select menus.

```csharp
using Mattermost.Models.Posts;
using System.Collections.Generic;

var props = new PostProps();
props.Attachments.Add(new PostPropsAttachment
{
    Text = "Choose an option",
    Actions =
    {
        new PostPropsSelectAction
        {
            Id = "actionoptions",
            Name = "Select an option...",
            DefaultOption = "opt2",
            Integration = new Integration
            {
                Url = "https://example.com/actionoptions",
                Context =
                {
                    ["action"] = "do_something"
                }
            },
            Options = new List<PostActionOption>
            {
                new PostActionOption("Option1", "opt1"),
                new PostActionOption("Option2", "opt2"),
                new PostActionOption("Option3", "opt3")
            }
        }
    }
});

await client.CreatePostAsync(channelId, "Message with a select menu", props: props);
```

For server-populated menus, set `DataSource` instead of `Options`:

```csharp
new PostPropsSelectAction
{
    Id = "actionusers",
    Name = "Select a user...",
    DataSource = PostActionDataSource.Users,
    Integration = new Integration
    {
        Url = "https://example.com/actionusers"
    }
};
```

When a user clicks a button or selects a menu option, Mattermost sends an HTTP `POST` request to the action's `Integration.Url`. Host that URL in your application and deserialize the JSON body with `PostActionIntegrationRequest`.

```csharp
app.MapPost("/mattermost/actions", (PostActionIntegrationRequest request) =>
{
    string action = request.Context["action"].GetString() ?? string.Empty;

    return Results.Ok(new
    {
        ephemeral_text = $"Received {action}"
    });
});
```

## Interactive dialogs

Interactive message actions and slash commands can open Mattermost interactive dialogs. Use the action payload's `trigger_id`, build an `InteractiveDialog`, and call `OpenInteractiveDialogAsync`.

```csharp
using Mattermost;
using Mattermost.Models.Dialogs;
using Mattermost.Models.Posts;
using System.Collections.Generic;

app.MapPost("/mattermost/actions", async (
    PostActionIntegrationRequest request,
    IMattermostClient mattermostClient) =>
{
    InteractiveDialog dialog = new InteractiveDialog
    {
        Title = "Create ticket",
        Elements = new List<InteractiveDialogElement>
        {
            new InteractiveDialogElement
            {
                DisplayName = "Summary",
                Name = "summary",
                Type = InteractiveDialogElementType.Text
            }
        }
    };

    await mattermostClient.OpenInteractiveDialogAsync(
        request.TriggerId,
        "https://example.com/mattermost/dialogs/submit",
        dialog);

    return Results.Ok();
});
```

The submit URL is your application endpoint. Deserialize the submitted payload with `InteractiveDialogSubmissionRequest` and return `InteractiveDialogResponse` when validation errors or multi-step form updates are needed.

```csharp
using Mattermost.Models.Dialogs;
using System.Collections.Generic;
using System.Text.Json;

app.MapPost("/mattermost/dialogs/submit", (InteractiveDialogSubmissionRequest request) =>
{
    JsonElement summaryElement;
    if (!request.Submission.TryGetValue("summary", out summaryElement)
        || string.IsNullOrWhiteSpace(summaryElement.GetString()))
    {
        return Results.Ok(new InteractiveDialogResponse
        {
            Errors = new Dictionary<string, string>
            {
                ["summary"] = "Summary is required."
            }
        });
    }

    return Results.Ok(new InteractiveDialogResponse
    {
        Type = "ok"
    });
});
```

For dynamic selects, set `InteractiveDialogElement.DataSource` to `InteractiveDialogDataSource.Dynamic`, set `DataSourceUrl`, and return `InteractiveDialogLookupResponse` from that lookup endpoint. For refresh or multi-step flows, set `InteractiveDialog.SourceUrl` and return `InteractiveDialogResponse` with `Type = "form"` and the replacement `Form`.

See Mattermost's [interactive dialogs documentation](https://developers.mattermost.com/integrate/plugins/interactive-dialogs/) for the full server-side flow and field behavior.

## Server health and timezones

```csharp
SystemStatusResponse health = await client.GetSystemStatusAsync(cancellationToken: cancellationToken);
SystemStatusResponse backends = await client.GetSystemStatusAsync(checkBackends: true, useRestSemantics: true, cancellationToken: cancellationToken);
IList<string> timezones = await client.GetSupportedTimezonesAsync(cancellationToken);
```

Use `Mattermost.Models.Responses` for `SystemStatusResponse`. Health checks do not authenticate, so they also work before login. The default check reads basic server health; `checkBackends: true` additionally asks the server to test database and file-storage connectivity, including test writes. Backend statuses are null when not requested.

By default, unhealthy health checks return HTTP 500 and throw `MattermostClientException`, retaining the response body. On Mattermost 9.6 or later, `useRestSemantics: true` returns the status as data; check `health.Status` rather than assuming HTTP success means the server is healthy. Supported timezone names require authentication. Both endpoints require Mattermost 3.10 or later.

## OAuth applications

```csharp
using Mattermost.Models.OAuth;

OAuthServerMetadata metadata = await client.GetOAuthServerMetadataAsync();

OAuthApp application = await client.CreateOAuthAppAsync(new OAuthApp
{
    Name = "My integration",
    Description = "Account integration",
    Homepage = "https://example.com",
    CallbackUrls = new List<string> { "https://example.com/oauth/callback" }
});

OAuthApp information = await client.GetOAuthAppInfoAsync(application.Id);
IList<OAuthApp> authorized = await client.GetAuthorizedOAuthAppsAsync("me");

application.Description = "Updated integration description";
application = await client.UpdateOAuthAppAsync(application);
```

The OAuth service provider must be enabled. `GetOAuthServerMetadataAsync` reads the public discovery endpoint without authenticating or sending saved credentials. It returns issuer and endpoint URLs, supported scopes, grants and PKCE/authentication methods. `RegistrationEndpoint` is empty when dynamic registration is disabled. Registration, management reads, updates, deletion and secret regeneration require `manage_oauth`; managing another creator's application also requires `manage_system_wide_oauth`. Trust changes require `manage_system`. `CreateOAuthAppAsync` registers a confidential client with a server-generated secret; dynamic and public-client registration are not covered by this method.

`GetOAuthAppAsync`, `GetOAuthAppsAsync`, creation and regeneration responses can contain client secrets: store them securely and do not log them. `GetOAuthAppInfoAsync` needs only authentication and returns an empty secret. `GetAuthorizedOAuthAppsAsync` also omits secrets and requires `edit_other_users` when reading another account.

`UpdateOAuthAppAsync` replaces all editable settings, including callback URLs and the associated `MattermostAppId`; it is not a patch. Fetch the application first to retain values you are not changing. Creator, creation time, client secret and dynamic-registration state are preserved. Updates preserve custom-scheme callbacks and empty homepages of dynamic clients; the server applies the URL rules for the stored registration type. `RegenerateOAuthAppSecretAsync` returns the updated application; `DeleteOAuthAppAsync` removes the registration.

Live verification passed for confidential registration, nonempty paged reads, full updates with server-owned fields preserved, secret regeneration and readback, information-only reads by a non-administrator, authorized-app metadata after consent by a dedicated account, and deletion of a separate application with subsequent HTTP 404.

## Plugin metadata

```csharp
var installed = await client.GetPluginsAsync(cancellationToken);
var webapp = await client.GetWebappPluginsAsync(cancellationToken);
var statuses = await client.GetPluginStatusesAsync(cancellationToken);
var marketplace = await client.GetMarketplacePluginsAsync(filter: "calls", localOnly: true, cancellationToken: cancellationToken);
bool visited = await client.GetMarketplaceVisitedByAdminAsync(cancellationToken);
```

These methods only read metadata; they do not install, activate, disable or remove plugins. Installed manifests, active webapp manifests and runtime states require Mattermost 4.4+ and enabled plugins. Installed manifests and runtime states require `sysconsole_read_plugins`; webapp manifests omit server executables and setting definitions. Runtime states include cluster node identifiers, error details and server paths, which may be sensitive.

Models in `Mattermost.Models.Plugins` preserve manifest components, platform executable paths, setting definitions, grouped sections and selectable options. Plugin-defined property values and setting defaults use `JsonElement` to retain their native JSON types. Setting definitions are not current configured values. `PluginState` in `Mattermost.Enums` uses the native numeric values and retains unrecognized numeric states.

Marketplace reads require Mattermost 5.16+ with plugins and Marketplace enabled. `localOnly` excludes the remote catalog; `remoteOnly` excludes local and prepackaged metadata and may require remote Marketplace access to be enabled. They cannot both be true. Unless `remoteOnly` is true, `sysconsole_read_plugins` is required. The method returns the native matching list. Mattermost 12.0.0 ignores Marketplace pagination and `server_version` parameters, so this method does not expose them or apply client-side pagination. Reading the administrator visit flag requires Mattermost 5.33+ and `manage_system` and does not mark it visited.

Live checks on Mattermost 12.0.0 passed all five reads with nonempty installed/webapp/status and local/remote Marketplace metadata, manifest/default/runtime-state comparison with direct HTTP, search filtering and unchanged visit state. A non-administrator bot could read webapp and remote-only Marketplace metadata; installed manifests, runtime states, local Marketplace metadata and the administrator visit flag returned the native HTTP 403.

## User account management

```csharp
var user = await client.CreateUserAsync(username, email, password, cancellationToken);
await client.PatchUserAsync(user.Id, new UserPatch { Nickname = "" }, cancellationToken);
await client.UpdateUserPasswordAsync(user.Id, newPassword, currentPassword, cancellationToken);
await client.UpdateUserActiveAsync(user.Id, active: true, cancellationToken: cancellationToken);
```

`CreateUserAsync` creates an email/password account using the current authenticated session. Signup restrictions apply unless the session is a system administrator. Invitation signup and external authentication are not supported by this method. Passwords are sent unchanged and must meet server policy.

`UserPatch` in `Mattermost.Models.Users` omits null fields. Empty strings clear optional name/nickname/position fields; username, email and locale cannot be blank. A supplied `Props`, `NotifyProps` or `Timezone` replaces that whole native map, not individual entries. Preserve existing values when changing only one map entry. `Password` confirms an own-email change; it does not update the password. Other-user profile edits require `edit_other_users`; provider-managed or locked fields may be rejected.

`DeactivateUserAsync` archives the account and revokes sessions without permanent deletion. `UpdateUserActiveAsync` can reactivate it; administrative user-management permission is required except for permitted self-deactivation. `UpdateUserRolesAsync` replaces system roles and requires `manage_roles`. Own-password changes require the current password; authorized administrators may omit it for other accounts. Administrative changes may send email notifications.

`ResetUserFailedAttemptsAsync` resets email/password or LDAP lockout counters and requires `sysconsole_write_user_management_users`. `VerifyUserEmailAsync` marks email verified without sending a verification message and requires `manage_system`. `RevokeUserSessionAsync` takes the session ID, not its token; `RevokeAllUserSessionsAsync` revokes every session of one account and requires Mattermost 4.4+. Both require ownership or `edit_other_users`. Session revocation does not delete personal access tokens, which can create new sessions while valid.

Live checks on Mattermost 12.0.0 passed all ten methods on a separate account: profile clearing preserved other fields, map replacement persisted, role replacement was restored, both password-change flows authenticated, reset removed an actual login lockout, verification persisted in the account's own profile, session revocation invalidated old sessions, and deactivation/reactivation retained a working account.

## Background jobs

```csharp
var jobs = await client.GetJobsAsync(jobType: "migrations", status: "success", cancellationToken: cancellationToken);
var job = await client.GetJobAsync(jobId, cancellationToken);
var jobsByType = await client.GetJobsByTypeAsync("plugins", cancellationToken: cancellationToken);
```

These read-only methods require Mattermost 4.1+ and permission to read the requested job types. Without a type filter, the server lists only types readable by the session. `Job` in `Mattermost.Models.Jobs` preserves native type/status strings, 64-bit counters, Unix-millisecond timestamps and job-specific string data. Job data may contain sensitive information.

`GetJobsAsync` supports independent type/status filters. `GetJobsByTypeAsync` also accepts `teamId` on servers supporting team-scoped jobs; the server filters by the job's stored team data. Team administrators with `manage_team_access_rules` may read their team's `access_control_sync` jobs without system-wide access. Neither listing starts, cancels or updates jobs.

Live checks on Mattermost 12.0.0 passed nonempty pages, single-job retrieval, type/status filtering, paging and native field/data comparison. A team filter returned an empty matching list; nonempty team-scoped job data was not tested.

## Server diagnostics

```csharp
var audits = await client.GetServerAuditsAsync(cancellationToken: cancellationToken);
var logs = await client.GetServerLogsAsync(logsPerPage: 100, cancellationToken: cancellationToken);
var metrics = await client.GetServerAnalyticsAsync(AnalyticsReport.Standard, cancellationToken: cancellationToken);
var busy = await client.GetServerBusyAsync(cancellationToken);
```

Audit reads require `read_audits`, log reads `get_logs` and a non-restricted administrator session, analytics `get_analytics`, and busy-state reads `manage_system`. Logs and audit entries may include private event details, addresses and session identifiers; handle them accordingly. Paging is zero-based; log pages contain 1–10,000 lines, selected with the native `logs_per_page` parameter.

Analytics requires Mattermost 4.0+. `AnalyticsReport` in `Mattermost.Enums` selects standard metrics, daily bot posts, daily posts, daily posting-user counts, or extra counters. An optional `teamId` filters by team. `AnalyticsRow` in `Mattermost.Models.Responses` preserves native metric/date labels and numeric values, including fractions. Live checks on Mattermost 12.0.0 passed all five global reports and four team-filtered reports; team-filtered `extra_counts` returned HTTP 500 while counting outgoing webhooks. That server error is propagated unchanged.

Busy-state reads require Mattermost 5.20+ and return `ServerBusyState` in `Mattermost.Models.Responses`. `ExpiresAt` is Unix seconds, not milliseconds, and may be negative while idle; `ExpiresTimestamp` preserves the server's readable timestamp. This method does not set or clear the busy flag.

## Server configuration and release metadata

```csharp
IDictionary<string, JsonElement> config = await client.GetServerConfigAsync(cancellationToken: cancellationToken);
IDictionary<string, JsonElement> overrides = await client.GetEnvironmentConfigAsync(cancellationToken);
IList<AppliedSchemaMigration> migrations = await client.GetAppliedSchemaMigrationsAsync(cancellationToken);
bool setupComplete = await client.GetFirstAdminOnboardingCompleteAsync(cancellationToken);
ServerRelease release = await client.GetLatestServerReleaseAsync(cancellationToken);
```

Import `System.Text.Json` for configuration values and `Mattermost.Models.Responses` for migration/release models. Configuration retains native section and field names, including version-specific or plugin settings, and preserves JSON strings, booleans, numbers, arrays and objects. The server masks secrets and filters sections by system console read permissions; other configuration values may still be sensitive. Optional `removeMasked` and `removeDefaults` filters require Mattermost 10.4+.

`GetEnvironmentConfigAsync` requires Mattermost 4.10+ and returns permission-filtered flags indicating environment overrides, not environment variable values. A native `null` response becomes an empty dictionary. Migration reads require a system console read permission and return version/name pairs without running migrations. Initial-setup status requires `manage_system`; release metadata additionally requires a non-restricted administrator session. `ServerRelease` preserves all seven native fields, including release-note text and string timestamps. Reading release metadata does not upgrade the server.

Live checks on Mattermost 12.0.0 passed all five operations: all four configuration-filter combinations, nonempty environment overrides and migration lists, setup status and all release fields matched direct native responses. Non-administrator requests preserved four permission errors and the native empty environment result. No server settings were changed.

## File previews and profile images

```csharp
byte[] preview = await client.GetFilePreviewAsync(fileId, cancellationToken);
byte[] thumbnail = await client.GetFileThumbnailAsync(fileId, cancellationToken);
byte[] profileImage = await client.GetUserImageAsync("me", cancellationToken);
byte[] defaultImage = await client.GetDefaultUserImageAsync("me", cancellationToken);
string publicLink = await client.GetPublicFileLinkAsync(fileId, cancellationToken);
```

Image methods return the complete image as bytes. File reads require permission to read the channel or ownership of the upload. A missing preview or thumbnail produces the server's error, without substituting the original file. User-image reads require authentication and permission to view the user; the default-image endpoint requires Mattermost 5.5 or later.

To change a profile image or team icon:

```csharp
byte[] image = await File.ReadAllBytesAsync("icon.png", cancellationToken);
await client.SetUserImageAsync("me", image, "icon.png", cancellationToken);
await client.SetTeamImageAsync(teamId, image, "icon.png", cancellationToken);
byte[] teamImage = await client.GetTeamImageAsync(teamId, cancellationToken);
await client.DeleteUserImageAsync("me", cancellationToken);
await client.DeleteTeamImageAsync(teamId, cancellationToken);
```

Image writes use multipart uploads and require configured file storage. The server validates image format, dimensions and size. Profile-image changes require permission to edit the user; changing a bot image requires its owner's or an administrator's access, not just the bot's own token. Locked or externally synchronized images may be rejected. Removing a profile image restores the generated default. Team-icon management requires `manage_team`; upload/download require Mattermost 4.9+, removal 4.10+. Removing a team icon resets its metadata, but the stored image may remain downloadable; a team without a stored image returns HTTP 404.

Public file links must be enabled on the server. Anyone who has the returned URL can download the file without authentication; share it only when public access is intended. HTTP errors from these methods use `MattermostClientException`, including the status, response body, URL, and method.

To inspect a file without downloading its content:

```csharp
FileHeaders headers = await client.GetFileHeadersAsync(fileId, cancellationToken);
FileHeaders previewHeaders = await client.GetFilePreviewHeadersAsync(fileId, cancellationToken);
FileHeaders thumbnailHeaders = await client.GetFileThumbnailHeadersAsync(fileId, cancellationToken);
```

These methods issue `HEAD` requests and return available content type, length, disposition, modification time and ETag headers. Optional headers may be absent. Missing images and permission errors are propagated; a `HEAD` error response may have no JSON body.

For a resumable attachment upload (Mattermost 5.28+):

```csharp
UploadSession upload = await client.CreateUploadAsync(channelId, "report.txt", data.LongLength, cancellationToken);
FileDetails? completed = await client.UploadDataAsync(upload.Id, data, cancellationToken);
```

`FileHeaders`, `UploadSession` and `FileDetails` are in `Mattermost.Models`. File attachments must be enabled and the account must have `upload_file` permission. `CreateUploadAsync` creates an attachment session, not an administrative import. `UploadDataAsync` sends a binary chunk and returns null for an incomplete upload (`204`), or file details when the file is complete. The first partial chunk must be at least 5 MiB; a smaller file can be uploaded in one complete chunk. Chunks must not be sent concurrently for the same session.

To resume, call `GetUploadAsync` and send bytes starting at its `FileOffset`. Read this offset again after an interrupted request rather than assuming how many bytes reached the server. The server removes the session on completion, so keep the returned file details. Uploading does not create a post; pass the completed file's ID to `CreatePostAsync`. The input chunk is copied before authentication and is not modified by the client.

## File search

```csharp
using Mattermost.Models.Responses;

FileSearchResponse teamFiles = await client.SearchTeamFilesAsync(teamId, "report in:documents ext:pdf",
    cancellationToken: cancellationToken);
FileSearchResponse allFiles = await client.SearchFilesAsync("report ext:pdf", cancellationToken: cancellationToken);
```

File search must be enabled. Team search requires Mattermost 5.34 or later and `view_team`; global search requires 10.2 or later. Results are filtered by the current account's access. Use `Order` to display files and `FileInfos` to look up their existing `FileDetails` metadata.

Both methods support AND/OR terms, archived channels, zero-based pages, positive page sizes and a UTC offset in seconds for date filters. Pagination depends on the search backend; file-content matching requires enabled content extraction. Use team search when filtering an ordinary channel by name. On the test server, unqualified `in:channel` in global search returned the same empty result through the client and a direct request.

Live verification passed for nonempty team/global searches, native order/map/cursors, independent file metadata readback, channel and extension filters, unmatched searches and AND/OR behavior. Optional pagination and search settings matched the native backend response.

## Post search and saved posts

```csharp
PostSearchResponse teamPosts = await client.SearchTeamPostsAsync(teamId, "incident in:town-square", cancellationToken: cancellationToken);
PostSearchResponse allPosts = await client.SearchPostsAsync("incident", isOrSearch: true, cancellationToken: cancellationToken);
ChannelPostsResponse saved = await client.GetFlaggedPostsAsync("me", channelId: channelId, cancellationToken: cancellationToken);
ChannelPostsResponse unread = await client.GetUnreadPostsAsync("me", channelId, limitBefore: 10, limitAfter: 50, cancellationToken: cancellationToken);
```

Use `Mattermost.Models.Responses` for these response types. Access posts through `Order` and `Posts`; search results also expose `Matches`, which may be null when the backend does not provide matched terms. Search supports Mattermost operators, AND/OR logic, archived channels, and a UTC offset in **seconds** for date searches. Pagination support depends on the server's search backend.

`GetFlaggedPostsAsync` reads the user's saved posts; when both filters are supplied, the channel filter takes precedence. Reading another user's saved or unread posts requires `edit_other_users`. `GetUnreadPostsAsync` requires Mattermost 5.14 or later and returns posts around the oldest unread post, or recent posts when none are unread. It does not mark them read. Limits are 0–200 before and 1–200 at/after the unread boundary.

## Team management

```csharp
Team team = await client.CreateTeamAsync("support", "Support", TeamType.InviteOnly, cancellationToken);
team = await client.PatchTeamAsync(team.Id, description: "Customer support", cancellationToken: cancellationToken);
IList<Team> teams = await client.SearchTeamsAsync("support", cancellationToken: cancellationToken);
TeamMember member = await client.AddTeamMemberAsync(team.Id, userId, cancellationToken);
```

Use `Mattermost.Enums` for `TeamType` and `Mattermost.Models.Teams` for team models. Creating a team requires `create_team`; edits require `manage_team`; adding another user requires `add_user_to_team`. `AddTeamMemberAsync` requires an actual user ID, not `"me"`.

`PatchTeamAsync` leaves null parameters unchanged. Empty descriptions, company names, or allowed domains clear those values, and `allowOpenInvite: false` disables open invitations. Team names and membership type are not changed by this method. Search returns the unpaged list visible to the current user.

## Custom slash commands

Register and manage custom commands through the API:

```csharp
var command = await client.CreateCommandAsync(new SlashCommand
{
    TeamId = teamId,
    Trigger = "hello",
    Method = "P",
    Url = "https://example.com/commands/hello",
    DisplayName = "Hello",
    AutoComplete = true,
    AutoCompleteDescription = "Say hello",
    AutoCompleteHint = "[name]"
}, cancellationToken);
var commands = await client.GetCommandsAsync(teamId, customOnly: true, cancellationToken: cancellationToken);
command.Description = "Hello command";
command = await client.UpdateCommandAsync(command, cancellationToken);
var autocomplete = await client.GetAutocompleteCommandsAsync(teamId, cancellationToken);
var suggestions = await client.GetCommandAutocompleteSuggestionsAsync(teamId, "/hello ", channelId, cancellationToken: cancellationToken);
```

`SlashCommand` and the autocomplete models are in `Mattermost.Models.SlashCommands`. Triggers do not include the leading slash. `Method` is `"P"` for POST or `"G"` for GET; command URLs must be absolute HTTP or HTTPS URLs. Commands must be enabled on the server. Management requires `manage_own_slash_commands` for the team and `manage_others_slash_commands` for another creator's commands. `CreatorId` can be specified on creation when those permissions allow it.

`UpdateCommandAsync` replaces writable settings; read a complete object with `GetCommandAsync` (Mattermost 5.22+) before changing it. The server preserves its token and creator; it does not persist plugin-provided structured autocomplete metadata for custom commands. `MoveCommandAsync` (Mattermost 5.22+) changes its team, `RegenerateCommandTokenAsync` returns a new secret and invalidates the old one, and `DeleteCommandAsync` removes the command. Treat command tokens as secrets. Listing with `customOnly: false` includes built-in/plugin commands and custom commands the current user can manage. Autocomplete listings may hide request URLs, methods, creators and tokens. Suggestions (Mattermost 5.24+) preserve input whitespace, include optional channel/thread context, and accept `"/"` for initial suggestions. Native autocomplete metadata and suggestion fields use PascalCase JSON names.

Execute a command as the current user:

```csharp
SlashCommandResponse response = await client.ExecuteCommandAsync(channelId, "/help",
    cancellationToken: cancellationToken);
```

Execution requires channel membership and `create_post` permission. Supply `teamId` for commands needing a team context in direct/group channels; ordinary channels determine their own team. `rootId` optionally places response posts in a thread. Arguments and their whitespace are sent unchanged. The server processes the response, so execution may create posts or invoke an external integration; do not post the returned text again automatically. Cancellation cannot undo an already executed command. `TriggerId` contains client trigger metadata when returned, and an empty server `response_type` is read as null. Existing integration responses still omit unset fields and serialize visibility as `"ephemeral"` or `"in_channel"`.

To implement the command's HTTP endpoint, use `SlashCommandRequestParser` to decode the URL-encoded POST body or GET query string and return a `SlashCommandResponse` as JSON.

For a POST command in an ASP.NET Core minimal API:

```csharp
using Mattermost.Helpers;
using Mattermost.Models.SlashCommands;
using System.IO;

app.MapPost("/mattermost/commands/weather", async (HttpRequest httpRequest) =>
{
    using StreamReader reader = new StreamReader(httpRequest.Body);
    string encodedParameters = await reader.ReadToEndAsync();
    SlashCommandRequest command = SlashCommandRequestParser.Parse(encodedParameters);

    // Validate the command token or Authorization header before processing the request.

    return Results.Json(new SlashCommandResponse
    {
        ResponseType = SlashCommandResponseType.InChannel,
        Text = $"Weather request: {command.Text}"
    });
});
```

For a GET command, pass the raw query string to the same parser:

```csharp
app.MapGet("/mattermost/commands/weather", (HttpRequest httpRequest) =>
{
    string encodedParameters = httpRequest.QueryString.Value ?? string.Empty;
    SlashCommandRequest command = SlashCommandRequestParser.Parse(encodedParameters);

    return Results.Json(new SlashCommandResponse
    {
        ResponseType = SlashCommandResponseType.Ephemeral,
        Text = $"Weather request: {command.Text}"
    });
});
```

`SlashCommandRequest.RootId` identifies the parent post when a command is invoked in a thread, while `UserMentions` and `ChannelMentions` map names in the command text to Mattermost identifiers. To return multiple immediate posts, add `SlashCommandResponseItem` values to `ExtraResponses`:

```csharp
return Results.Json(new SlashCommandResponse
{
    ResponseType = SlashCommandResponseType.InChannel,
    Text = "Weather report",
    ExtraResponses = new List<SlashCommandResponseItem>
    {
        new SlashCommandResponseItem
        {
            ResponseType = SlashCommandResponseType.Ephemeral,
            Text = "Only the command author can see this detail."
        }
    }
});
```

For work that completes after the initial request, send a `SlashCommandResponse` to the request's `ResponseUrl` using an application-managed `HttpClient`:

```csharp
await httpClient.PostAsJsonAsync(command.ResponseUrl, new SlashCommandResponse
{
    ResponseType = SlashCommandResponseType.InChannel,
    Text = "The delayed weather report is ready."
});
```

Validate the command token or authorization header before processing either request method. See Mattermost's [custom slash commands documentation](https://developers.mattermost.com/integrate/slash-commands/custom/) for command registration, request validation, and response behavior.

---

# Builders

## PostBuilder

```csharp
using Mattermost.Builders;
using Mattermost.Enums;

await new PostBuilder()
    .ToChannel(channelId)
    .AddText("@username Please acknowledge this incident.")
    .SetPriority(
        MessagePriority.Urgent,
        requestedAck: true,
        persistentNotifications: true)
    .SendMessageAsync(client);
```

## Markdown table builder

```csharp
using Mattermost.Builders;
using Mattermost.Models.Enums;

string table = new TableMarkdownBuilder(3, TableAlignment.Center)
    .AddHeader("Name", "Status", "Score")
    .AddRow("Build", "OK", 100)
    .AddRow("Tests", "OK", 100)
    .ToString();

await client.CreatePostAsync(channelId, table);
```

---

# Client settings

```csharp
IDictionary<string, string> config = await client.GetClientConfigAsync(
    authenticated: true, cancellationToken: cancellationToken);
IDictionary<string, string> license = await client.GetClientLicenseAsync(
    cancellationToken: cancellationToken);
long load = await client.GetLicenseLoadMetricAsync(cancellationToken);
```

Client configuration and license methods return the server's string-valued maps; boolean and numeric values remain strings, and available keys vary with server version, settings and permissions. Both methods default to an anonymous request without sending the saved token. Set `authenticated: true` to use the current account. Additional license information requires `read_license_information` permission. These methods neither change server configuration nor activate a license.

The license load metric requires authentication and Mattermost 10.8 or later. It is the monthly active non-bot, non-deleted user count divided by the licensed user limit, multiplied by 1000 and rounded; 1000 represents 100 percent. It is zero when no license or user limit is present. HTTP errors are propagated.

---

# Custom emoji management

```csharp
byte[] image = await File.ReadAllBytesAsync("build.png", cancellationToken);
Emoji emoji = await client.CreateEmojiAsync("build", image, "build.png", cancellationToken);
await client.DeleteEmojiAsync(emoji.Id, cancellationToken);
```

Creation uploads the image and the authenticated user's creator ID as multipart form data. Requires `EnableCustomEmoji` and `create_emojis` permission. The server validates image format, dimensions and upload size, lowercases names, and rejects duplicate or built-in emoji names. Deletion requires `delete_emojis`; deleting another user's emoji additionally requires `delete_others_emojis`. HTTP errors are propagated.

---

# Webhook management

```csharp
IncomingWebhook incoming = await client.CreateIncomingWebhookAsync(new IncomingWebhook
{
    ChannelId = channelId,
    DisplayName = "Build notifications",
    ChannelLocked = true
}, cancellationToken);

IList<IncomingWebhook> hooks = await client.GetIncomingWebhooksAsync(teamId,
    cancellationToken: cancellationToken);
incoming.Description = "Build status updates";
incoming = await client.UpdateIncomingWebhookAsync(incoming, cancellationToken);

OutgoingWebhook outgoing = await client.GetOutgoingWebhookAsync(outgoingWebhookId, cancellationToken);
outgoing.Description = "Updated description";
outgoing = await client.UpdateOutgoingWebhookAsync(outgoing, cancellationToken);
```

Webhook operations require the corresponding server setting and `manage_own_incoming_webhooks` or `manage_own_outgoing_webhooks` permission. Managing another owner's webhook requires the corresponding `manage_others_*` permission. Incoming hooks require access to their destination channel. Outgoing hooks target public channels; a team-wide hook has an empty `ChannelId` and requires trigger words. Callback URLs must be absolute HTTP(S) URLs. `OutgoingWebhookTriggerWhen` matches the first word exactly or by prefix. Username and icon overrides are subject to server settings, and incoming channel locking may be enforced by permissions.

Updates replace writable settings, so read the webhook first. For outgoing hooks, carry the existing `Token`: the server overwrites it, and the SDK rejects an empty token. `RegenerateOutgoingWebhookTokenAsync` returns the new configuration and token; update the receiver's token check accordingly. Creation ignores supplied IDs, tokens and timestamps; optional incoming `UserId` and outgoing `CreatorId` can assign another owner when permitted. Updates cannot change ownership or move a webhook to another team.

Incoming webhook identifiers grant access to their posting URLs; outgoing tokens authenticate callback payloads. Keep both secret. List methods return empty collections for JSON null or empty arrays. For outgoing lists, a channel filter takes precedence over a team filter. Deletion invalidates the webhook without deleting existing posts. HTTP errors are propagated.

---

# Synced drafts

```csharp
Draft? draft = await client.UpsertDraftAsync(new Draft
{
    ChannelId = channelId,
    Text = "Work in progress"
}, cancellationToken);

IList<Draft> drafts = await client.GetDraftsAsync(teamId, cancellationToken);
await client.DeleteDraftAsync(channelId, cancellationToken: cancellationToken);
await client.DeleteDraftAsync(channelId, threadId: rootPostId, cancellationToken: cancellationToken);
```

Synced drafts require the server's `AllowSyncedDrafts` setting and permission to create posts in the channel. All draft methods operate on the authenticated user. `RootId` is empty for a channel draft and is the root post ID for a reply draft. Saving replaces the complete draft; read it first to preserve attachments, properties and priority. Server-owned user IDs, timestamps and metadata are not sent. `Props` and `Priority` use the server's JSON property bags; priority properties are `priority`, `requested_ack` and `persistent_notifications`.

Saving empty `Text` deletes that draft and returns null. `GetDraftsAsync` returns an empty list when there are no drafts, including when the server sends JSON null. Deletion of an absent draft succeeds and does not delete posted messages. Disabled synced drafts return HTTP 501; HTTP errors are propagated.

---

# Followed threads

```csharp
await client.FollowThreadAsync("me", teamId, rootPostId, cancellationToken);
UserThread thread = await client.GetUserThreadAsync("me", teamId, rootPostId,
    extended: true, cancellationToken: cancellationToken);
UserThreadsResponse followed = await client.GetUserThreadsAsync("me", teamId,
    perPage: 60, excludeDirect: true, cancellationToken: cancellationToken);

thread = await client.UpdateThreadReadAsync("me", teamId, rootPostId,
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), cancellationToken);
thread = await client.MarkThreadUnreadAsync("me", teamId, rootPostId, replyPostId, cancellationToken);
await client.UnfollowThreadAsync("me", teamId, rootPostId, cancellationToken);
```

Thread identifiers are root post IDs. These operations require Mattermost 5.29 or later; marking unread from a post requires 6.7 or later. The user must be able to read the thread, and accessing another user's state requires `edit_other_users` permission. Thread metadata is created after the first reply.

`GetUserThreadsAsync` uses `beforeThreadId` or `afterThreadId` cursors, never both. `since` filters membership updates using Unix milliseconds. By default, direct and group message threads are included. `totalsOnly` returns counts without threads; `threadsOnly` returns threads with zero totals. These two flags are mutually exclusive. Empty thread and participant lists may be null. `MarkThreadUnreadAsync` also follows the thread; `MarkAllThreadsReadAsync` changes every followed thread's read state for the team. Unfollowing leaves posts intact. HTTP errors are propagated.

---

# Channel suggestions and upload discovery

```csharp
var channels = await client.AutocompleteChannelsAsync(teamId, "town", cancellationToken);
var searchFilters = await client.AutocompleteChannelsForSearchAsync(teamId, "town", cancellationToken);
var commonTeams = await client.GetChannelMembersCommonTeamsAsync(groupChannelId, cancellationToken);
var uploads = await client.GetUserUploadsAsync("me", cancellationToken);
```

Autocomplete accepts an empty name for initial suggestions, respects channel visibility and may include archived channels. Search suggestions include the current user's channels, including direct/group channels in that team context. Common-team lookup supports direct and group message channels, requires Mattermost 9.1 or later and is not available to guests.

`GetRecommendedChannelsAsync` returns attribute-based public-channel recommendations when Enterprise Advanced and attribute-based access control are enabled; it returns an empty list when that feature is unavailable. `GetUserUploadsAsync` (Mattermost 5.28+) lists only the current user's incomplete upload sessions. Administrators cannot use it to list someone else's uploads. Completed sessions are removed by the server. HTTP errors are propagated.

---

# API coverage

The public API is exposed through `IMattermostClient` and includes:

- authentication and logout;
- current user, users by id, username, or email;
- bulk user profiles and single or bulk presence statuses;
- team creation, metadata editing, search, lookup, membership reads and addition, statistics, and unread counts;
- create, update, delete, read, and list posts, plus synchronized drafts;
- thread posts, followed threads, thread read state, team-wide and cross-team search, saved posts, and unread-boundary reads;
- channel lookup, creation, archiving, membership reads and changes, statistics, unread counts, and member timezones;
- direct and group channels;
- file upload, download, streaming, metadata, previews, thumbnails, and public links;
- profile images, custom emoji metadata and images, role metadata, and user preferences;
- server health, supported timezones, client settings, client license information and license load metrics;
- incoming and outgoing webhook management, including outgoing token regeneration;
- Calls plugin channel state, active call checks, and host call termination;
- WebSocket events for messages, status changes, connection changes, and raw events.

See [`IMattermostClient`](https://github.com/bvdcode/Mattermost.NET/blob/main/Sources/Mattermost/IMattermostClient.cs) for the full list of implemented methods.

Missing a Mattermost API method? Please open an issue with the exact Mattermost endpoint or scenario you need:

https://github.com/bvdcode/Mattermost.NET/issues/new?template=Blank+issue

---

# Target framework

Mattermost.NET targets both `.NET Standard 2.0` and `.NET Standard 2.1`.

---

# License

Distributed under the MIT License. See [LICENSE.md](https://github.com/bvdcode/Mattermost.NET/blob/main/LICENSE.md) for more information.

# Contact

[E-Mail](mailto:github-mattermost-net@belov.us)
