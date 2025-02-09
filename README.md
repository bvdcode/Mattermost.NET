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

### `CreatePostAsync` - Send message to specified channel using channel identifier.

`string channelId` - The ID of the channel to send the message to.
`string message` - The message to send.
`string replyToPostId` - The ID of the post to reply to (optional).
`MessagePriority priority` - The priority of the message, default is `MessagePriority.Empty`.
`IEnumerable<string> files` - The files to upload, you have to upload files before sending the message.

Example:

```csharp
await client.CreatePostAsync("channel_id", "Hello, World!");
```

---

### `GetPostAsync` - Get post by identifier.

`string postId` - The ID of the post to get.

Example:

```csharp
var post = await client.GetPostAsync("post_id");
```

---

### `UpdatePostAsync` - Update message text for specified post identifier.

`string postId` - The ID of the post to update.
`string message` - The new message text.

Example:

```csharp
await client.UpdatePostAsync("post_id", "I changed my mind");
```

---

### `DeletePostAsync` - Delete post with specified post identifier.

`string postId` - The ID of the post to delete.

Example:

```csharp
await client.DeletePostAsync("post_id");
```

---

## Channels

### `CreateChannelAsync` - Create simple channel with specified users.

`string teamId` - The ID of the team to create the channel in.
`string name` - The name of the channel to create.
`string displayName` - The display name of the channel to create.
`ChannelType channelType` - The type of the channel to create.
`string purpose` - The purpose of the channel to create (optional).
`string header` - The header of the channel to create (optional).

Example:

```csharp
await client.CreateChannelAsync("team_id", "channel_name", "Channel display name", ChannelType.Public, "Channel purpose", "Channel header");
```

---

### `CreateGroupChannelAsync` - Create group channel with specified users.

`string[] userIds` - The IDs of the users to create the group channel with.

Example:

```csharp
await client.CreateGroupChannelAsync([ "user_id_1", "user_id_2" ]);
```

---

### `AddUserToChannelAsync` - Add user to specified channel.

`string channelId` - The ID of the channel to add the user to.
`string userId` - The ID of the user to add to the channel.

Example:

```csharp
await client.AddUserToChannelAsync("channel_id", "user_id");
```

---

### `DeleteUserFromChannelAsync` - Remove user from specified channel.

`string channelId` - The ID of the channel to remove the user from.
`string userId` - The ID of the user to remove from the channel.

Example:

```csharp
await client.DeleteUserFromChannelAsync("channel_id", "user_id");
```

---

### `FindChannelByNameAsync` - Find channel by name and team ID.

`string teamId` - The ID of the team to search in.
`string channelName` - The name of the channel to search for.

Example:

```csharp
Channel? channel = await client.FindChannelByNameAsync("team_id", "channel_name");
```

---

### `ArchiveChannelAsync` - Archive specified channel.

`string channelId` - The ID of the channel to archive.

Example:

```csharp
bool archived = await client.ArchiveChannelAsync("channel_id");
```

---

## Files

### `GetFileAsync` - Get file bytes by identifier.

`string fileId` - The ID of the file to get.

Example:

```csharp
byte[] file = await client.GetFileAsync("file_id");
```

---

### `GetFileDetailsAsync` - Get file details by identifier.

`string fileId` - The ID of the file to get.

Example:

```csharp
FileDetails fileDetails = await client.GetFileDetailsAsync("file_id");
```

---

### `UploadFileAsync` - Upload file to specified channel.

`string channelId` - The ID of the channel to upload the file to.
`string filePath` - The path to the file to upload.
`Action<int>? progressChanged` - The action to call with the upload progress.

Example:

```csharp
var callback = new Action<int>(progress => Console.WriteLine($"Upload progress: {progress}%"));
await client.UploadFileAsync("channel_id", "file_path", callback);
```

---

## Users

### `GetMeAsync` - Get current authenticated user information.

Example:

```csharp
User me = await client.GetMeAsync();
```

---

### `GetUserAsync` - Get user by identifier.

`string userId` - The ID of the user to get.

Example:

```csharp
User user = await client.GetUserAsync("user_id");
```

---

### `GetUserByUsernameAsync` - Get user by username.

`string username` - The username of the user to get.

Example:

```csharp
User user = await client.GetUserByUsernameAsync("username");
```

---

## Other

`SetChannelCallStateAsync` - Set call state for specified channel ('Calls' plugin required).

---

> If you are looking for another methods, please visit the [Mattermost API documentation](https://api.mattermost.com/) and create an issue in the [GitHub repository](https://github.com/bvdcode/Mattermost.NET/issues/new?template=Blank+issue) with what exact methods you need - I will add them as soon as possible.

---

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-mattermost-net@belov.us)
