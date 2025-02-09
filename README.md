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

# Getting Started

## Installation

The library is available as a NuGet package. You can install it using the NuGet Package Manager or the `dotnet` CLI.

```bash
dotnet add package Mattermost.NET
```

## Usage

### Create a new bot

```csharp
using Mattermost.NET;
const string token = "37VlFKySIZn6gryA85cR1GKBQkjmfRZ6";
const string server = "https://mm.your-server.com"; // or https://community.mattermost.com by default
MattermostClient client = new(server);

var botUser = await client.LoginAsync(token);
```

### Subscribe to post updates

```csharp
client.OnMessageReceived += Client_OnMessageReceived;

private static void Client_OnMessageReceived(object? sender, MessageEventArgs e)
{
    if (string.IsNullOrWhiteSpace(e.Message.Post.Text))
    {
        return;
    }
    e.Client.SendMessageAsync(e.Message.Post.ChannelId, "Hello, World!");
}
```

### Start the bot

```csharp
await client.StartReceivingAsync();
```

### Stop the bot

```csharp
await client.StopReceivingAsync();
```

## Client Methods

### `SendMessageAsync`

```csharp
await client.SendMessageAsync("channel_id", "Hello, World!");
```

### `GetChannelMembersAsync`

```csharp
var members = await client.GetChannelMembersAsync("channel_id");
```

### `GetChannelPostsAsync`

```csharp
var posts = await client.GetChannelPostsAsync("channel_id");
```

`...` and more methods will be documented soon.

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-mattermost-net@belov.us)
