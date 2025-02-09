[![GitHub](https://img.shields.io/github/license/bvdcode/Mattermost.NET)](https://github.com/bvdcode/Mattermost.NET/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/dt/Mattermost.NET?color=%239100ff)](https://www.nuget.org/packages/Mattermost.NET/)
[![Static Badge](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/Mattermost.NET)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bvdcode/Mattermost.NET/.github%2Fworkflows%2Fpublish-release.yml)](https://github.com/bvdcode/Mattermost.NET/actions)
[![NuGet version (Mattermost.NET)](https://img.shields.io/nuget/v/Mattermost.NET.svg?label=stable)](https://www.nuget.org/packages/Mattermost.NET/)
[![CodeFactor](https://www.codefactor.io/repository/github/bvdcode/Mattermost.NET/badge)](https://www.codefactor.io/repository/github/bvdcode/Mattermost.NET)
![GitHub repo size](https://img.shields.io/github/repo-size/bvdcode/Mattermost.NET)

<a id="readme-top"></a>

# Mattermost.NET

Ready-to-use **.NET Standard** library for convenient development of Mattermost bots.

---

# Installation

The library is available as a NuGet package. You can install it using the NuGet Package Manager or the `dotnet` CLI.

```bash
dotnet add package Mattermost.NET
```

---

# Usage

## Create a new bot

```csharp
using Mattermost.NET;
const string token = "37VlFKySIZn6gryA85cR1GKBQkjmfRZ6";
const string server = "https://mm.your-server.com"; // or https://community.mattermost.com by default
MattermostClient client = new(server);
```

## Authenticate the bot

```csharp
var botUser = await client.LoginAsync(token);
```

## Subscribe to post updates

```csharp
client.OnMessageReceived += ClientOnMessageReceived;

private static void ClientOnMessageReceived(object? sender, MessageEventArgs e)
{
    if (string.IsNullOrWhiteSpace(e.Message.Post.Text))
    {
        return;
    }
    e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello, World!");
}
```

## Start the bot updates

```csharp
await client.StartReceivingAsync();
```

> **Note:** The bot will automatically reconnect if the connection is lost. It's not required to call `StartReceivingAsync` if you don't want to receive updates through the WebSocket connection.

## Stop the bot

```csharp
await client.StopReceivingAsync();
```

---

# Client Methods

## Posts

### `SendMessageAsync:`

`string channelId` - The ID of the channel to send the message to.
`string message` - The message to send.
`string replyToPostId` - The ID of the post to reply to (optional).
`MessagePriority priority` - The priority of the message, default is `MessagePriority.Empty`.
`IEnumerable<string> files` - The files to upload, you have to upload files before sending the message.

Example:

```csharp
await client.SendMessageAsync("channel_id", "Hello, World!");
```

---

### `UpdatePostAsync:`

`string postId` - The ID of the post to update.
`string message` - The new message text.

Example:

```csharp
await client.UpdatePostAsync("post_id", "I changed my mind");
```

---

### `DeletePostAsync:`

`string postId` - The ID of the post to delete.

Example:

```csharp
await client.DeletePostAsync("post_id");
```

---

### `GetChannelMembersAsync`

Example:

```csharp
var members = await client.GetChannelMembersAsync("channel_id");
```

### `GetChannelPostsAsync`

```csharp
var posts = await client.GetChannelPostsAsync("channel_id");
```

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-mattermost-net@belov.us)
