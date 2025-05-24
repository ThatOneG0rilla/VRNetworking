# Introduction
Steps:
1. Import the package.
2. Input your app id, and voice id
3. If you want backup servers, input those ids as well.
4. Use the Objects in the example scene.
5. Input all player settings that is needed, and change any settings you want.

The package offers a range of features, including:
- Large amount of debug
- Backup servers
- Many bools and settings for customisability
- Automatic naming when name is not set
- Syncs name to playfab automatically
- Logging user when they join, leave, connect and disconnect fron a room
- Improved playfab login
- Moderator item safety in playfab login

## Script Usage
### `SetCosmetic`
- Sets a players cosmetic
Example:
```csharp
NetworkManager.SetCosmetic("Head", "Hat");
```

### `SetPlayerColor`
- Sets a players color
Example:
```csharp
NetworkManager.SetPlayerColor(color);
```

### `SetPlayerName`
- Sets a players name
Example:
```csharp
NetworkManager.SetPlayerName(name);
```

### `JoinRandom`
- Joins a random room
Example:
```csharp
NetworkManager.JoinRandom();
```

### `JoinPrivate`
- Joins a private room
Example:
```csharp
NetworkManager.JoinPrivate(roomID);
```

# Info
I will be posting future versions in [Discord](https://discord.gg/gorillasdevhub). Join to get updates.
If you want to purchase VRNetworking Plus, you can buy it here:
