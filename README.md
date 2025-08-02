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

> Mattermost.NET is a production-ready .NET Standard library for building bots and integrations for the Mattermost platform. It provides a clean, strongly-typed C# interface over the Mattermost API, with support for messaging, channel management, file uploads, and real-time WebSocket updates. The client handles authentication, reconnection, and offers async methods for most operations. Published on NuGet under the MIT license.

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
const string server = "https://mm.your-server.com"; // or https://community.mattermost.com by default
MattermostClient client = new(server);
```

## Authenticate the bot with credentials

```csharp
var botUser = await client.LoginAsync(username, password);
// Or you can use constructor if you have API key, ex. personal or bot token
// It will automatically authenticate the bot
const string token = "37VlFKySIZn6gryA85cR1GKBQkjmfRZ6";
MattermostClient client = new(server, token);
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

> The rest of the methods are implemented according to the Mattermost API. You can find them in [IMattermostClient](https://github.com/bvdcode/Mattermost.NET/blob/main/Sources/Mattermost/IMattermostClient.cs).

> If you are looking for another methods, please visit the [Mattermost API documentation](https://api.mattermost.com/) and create an issue in the [GitHub repository](https://github.com/bvdcode/Mattermost.NET/issues/new?template=Blank+issue) with what exact methods you need - I will add them as soon as possible.

---

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-mattermost-net@belov.us)
