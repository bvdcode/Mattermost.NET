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

## Work with channels

```csharp
var channel = await client.GetChannelAsync(channelId);
var found = await client.FindChannelByNameAsync(teamId, "town-square");
var direct = await client.CreateDirectChannelAsync(userId);
```

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
    .AddText("Important message")
    .SetPriority(MessagePriority.Important)
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
- create, update, delete, read, and list posts;
- thread posts;
- channel lookup, creation, archiving, and membership changes;
- direct and group channels;
- file upload, download, streaming, and metadata;
- Calls plugin channel state;
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
