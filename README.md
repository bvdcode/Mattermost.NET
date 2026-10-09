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

## Get current user

```csharp
var me = await client.GetMeAsync();
Console.WriteLine(me.Username);
```

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

## Read user presence

```csharp
var presence = await client.GetUserStatusAsync("me", cancellationToken);
var statuses = await client.GetUsersStatusesByIdsAsync(userIds, cancellationToken);
Console.WriteLine(presence.Status);
```

These methods return `UserPresence` with a typed `UserStatus` (`Online`, `Offline`, `Away`, `DoNotDisturb`, or `OutOfOffice`), `IsManual`, `LastActivityAt` (Unix milliseconds), and `DoNotDisturbEndTime` (Unix seconds; zero means no expiry). Bulk status lookup requires actual user IDs, not `"me"`; match responses by `UserId`, not list position. Server errors are propagated, including a missing single-user status.

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
IList<Emoji> selected = await client.GetEmojisByNamesAsync(new[] { "party_parrot", "custom_rocket" }, cancellationToken);
IList<Emoji> matches = await client.SearchEmojisAsync("parrot", prefixOnly: false, cancellationToken: cancellationToken);
IList<Emoji> suggestions = await client.AutocompleteEmojisAsync("party_", cancellationToken);
```

These methods return metadata for custom emojis, not image contents or the built-in emoji catalog. They require authentication and custom emojis enabled on the server. `Emoji` contains `Id`, `Name`, `CreatorId`, and 64-bit creation, update, and deletion timestamps in Unix milliseconds.

Pages are zero-based with 1–200 entries; the default page size is 60. Name sorting, lookup by name, search, and autocomplete require Mattermost 4.7 or later. Bulk name lookup requires Mattermost 9.2 or later and accepts up to 200 distinct names. Duplicate names are removed; missing and built-in names are omitted from the response. Match results by `Name` or `Id`, not input position.

Names and search terms have surrounding whitespace and colons removed; case is preserved. Search matches anywhere in the name unless `prefixOnly: true` is set and returns up to 200 results. Autocomplete always matches a prefix and returns up to 100 results. Both are sorted by name. A missing single emoji produces an API error; a search without matches returns an empty list. HTTP errors, including disabled custom emojis or unsupported endpoints, are propagated.

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

## Custom slash commands

These types help you implement the HTTP endpoint for a custom slash command; they do not register the command in Mattermost. Configure the command's request URL and HTTP method in Mattermost, then use `SlashCommandRequestParser` to decode the URL-encoded POST body or GET query string and return a `SlashCommandResponse` as JSON.

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

# API coverage

The public API is exposed through `IMattermostClient` and includes:

- authentication and logout;
- current user, users by id, username, or email;
- bulk user profiles and single or bulk presence statuses;
- team lookup, membership reads, statistics, and unread counts;
- create, update, delete, read, and list posts;
- thread posts;
- channel lookup, creation, archiving, membership reads and changes, statistics, unread counts, and member timezones;
- direct and group channels;
- file upload, download, streaming, and metadata;
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
