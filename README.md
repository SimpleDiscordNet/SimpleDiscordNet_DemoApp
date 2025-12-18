### SimpleDiscordNet Demo App

This repository is a minimal end-to-end demo of building a Discord bot with `SimpleDiscordNet` and wiring all logs into `Microsoft.Extensions.Logging` with a console sink.

#### What this demo shows
- Establishing a bot using `SimpleDiscordNet` and a fluent builder.
- Forwarding SimpleDiscordNet's native logs (`DiscordEvents.Log` / `DiscordEvents.Error`) into `ILogger`.
- Configuring an `ILogger` console sink (`AddSimpleConsole`) with timestamps and single-line output.
- Environment variable configuration for token and guild IDs.
- Registering slash commands:
  - Ungrouped command: `/hello`
  - Grouped commands: `/demo text` and `/demo embed`
  - Ambient data command: `/ambient info`
  - Components & Modals: `/components show` (buttons), `/components select` (string select), and `/components modal` (opens a modal)
  - Message commands: `/messages text`, `/messages embed`, `/messages complex`
  - Permission checking: `/permissions`
  - Role management: `/roles list`, `/roles demo`
  - Channel queries: `/channels list`, `/channels info`, `/channels types`
- Accessing cached ambient data via `DiscordContext` with the `[DiscordContext]` attribute.
- Optional: sending a direct channel message after startup to demonstrate REST sending.
- AoT (Ahead-of-Time) compilation support for Release builds.

---

### How it works

#### 1) Logging pipeline
`Program.cs` creates a `LoggerFactory` that writes to the console:

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});
ILogger logger = loggerFactory.CreateLogger("SimpleDiscordNetDemo");
```

Then we bridge SimpleDiscordNet logs into that `ILogger`:

```csharp
DiscordEvents.Log += (_, m) =>
{
    LogLevel msLevel = m.Level.ToString() switch
    {
        "Trace" => LogLevel.Trace,
        "Debug" => LogLevel.Debug,
        "Information" or "Info" => LogLevel.Information,
        "Warning" or "Warn" => LogLevel.Warning,
        "Error" => LogLevel.Error,
        "Critical" or "Fatal" => LogLevel.Critical,
        _ => LogLevel.Information
    };

    if (m.Exception is not null)
        logger.Log(msLevel, m.Exception, "{Message}", m.Message);
    else
        logger.Log(msLevel, "{Message}", m.Message);
};

DiscordEvents.Error += (_, ex) => logger.LogError(ex, "SimpleDiscordNet reported an error");
DiscordEvents.Connected += (_, __) => logger.LogInformation("Connected to Discord Gateway.");
```

Result: all framework logs (including exceptions) appear in the console with timestamps and levels.

#### 2) Bot startup
The bot is created using the fluent builder and started with a cancellation token that listens for Ctrl+C:

```csharp
DiscordBot bot = DiscordBot.NewBuilder()
    .WithToken("<BOT_TOKEN>")
    .WithIntents(DiscordIntents.Guilds | DiscordIntents.GuildMembers)
    .WithPreloadOnStart(guilds: true, channels: true, members: false)
    .WithDevelopmentMode(true)
    .If(!string.IsNullOrWhiteSpace("<DEV_GUILD_ID>"), b => b.WithDevelopmentGuild("<DEV_GUILD_ID>"))
    .Build();

await bot.StartAsync(cts.Token);
```

The `.WithDevelopmentMode(true)` plus `.WithDevelopmentGuild(...)` ensures your slash commands sync immediately to your dev guild at startup, which is ideal for iteration.

#### 3) Commands
- `BasicCommands` exposes `/hello`, returning a simple embed.
- `DemoGroupCommands` provides `/demo text` and `/demo embed` to illustrate plain vs embed responses.
- `AmbientDemoCommands` demonstrates ambient data with `/ambient info`, using the `[DiscordContext]` attribute to access cached datasets and replying ephemerally with counts.
- `ComponentsDemoCommands` demonstrates message components and modals:
  - `/components show` sends three buttons (Ping, Open Modal, Danger). Clicking updates the original message; the "Open Modal" button opens a modal immediately.
  - `/components select` sends a string select menu; choosing an option updates the original message with the selection.
  - `/components modal` (slash) opens a modal with two text inputs for collecting feedback.
- `MessagesDemoCommands` showcases message and embed features:
  - `/messages text` sends a simple text response.
  - `/messages embed` sends a message with a basic embed.
  - `/messages complex` demonstrates advanced embed features with formatting, inline fields, and links.
- `PermissionsRolesDemoCommands` demonstrates permission and role checking:
  - `/permissions` checks your permissions in the current guild based on your roles.
- `RolesDemoCommands` provides role management demonstrations:
  - `/roles list` lists all roles in the guild with their permissions.
  - `/roles demo` shows your assigned roles in the current guild.
- `ChannelsDemoCommands` demonstrates channel querying:
  - `/channels list` lists all channels in the guild with type icons.
  - `/channels info` shows detailed information about the current channel.
  - `/channels types` provides a breakdown of channel types in the guild (categories, text, voice, threads).

#### 4) Ambient snapshot service (diagnostic)
`AmbientSnapshotService` periodically prints a compact line with raw and de-duplicated counts of cached guilds/channels/members/users. The interval defaults to 60s and can be overridden with the `AMBIENT_TICK_SECONDS` environment variable.

---

### Running the demo

1. Open the solution and restore/build.

2. Create a `discord_token.txt` file in the project root directory based on `discord_token.txt.example`:
   ```
   # Discord Bot Configuration
   # Copy this file to discord_token.txt and fill in your values

   DISCORD_TOKEN=your_bot_token_here
   DEV_GUILD_ID=your_dev_guild_id_here
   DEMO_CHANNEL_ID=optional_channel_id_here
   ```

   Required values:
   - `DISCORD_TOKEN`: your bot token (required)
   - `DEV_GUILD_ID`: a guild ID where your bot is installed for instant dev sync (required)

   Optional values:
   - `DEMO_CHANNEL_ID`: a channel ID to demonstrate direct channel sending after startup
   - Environment variable `AMBIENT_TICK_SECONDS`: interval in seconds for ambient snapshot logging (defaults to 60)

   **Important:** Keep your real bot token secret and never commit `discord_token.txt` to version control. The file is already in `.gitignore`.

3. Run the app:
   ```bash
   dotnet run --project SimpleDiscordNet_DemoApp.csproj
   ```

4. On first run (in development mode), slash commands are synced instantly to your dev guild. In Discord, try:
   - `/hello` - Basic greeting with embed
   - `/demo text` and `/demo embed` - Simple text and embed responses
   - `/ambient info` - View cached guild/channel/member/user counts
   - `/components show`, `/components select`, `/components modal` - Interactive components and modals
   - `/messages text`, `/messages embed`, `/messages complex` - Various message formats
   - `/permissions` - Check your permissions
   - `/roles list` and `/roles demo` - View roles
   - `/channels list`, `/channels info`, `/channels types` - Channel information

5. Observe logs in the console with timestamps and levels, including any forwarded exceptions.

### Building for Production (Native AoT)

Native AoT (Ahead-of-Time) compilation produces a single native executable with fast startup and small size. This requires C++ build tools to be installed.

#### Prerequisites: Install Visual Studio Build Tools

Native AoT compilation requires the Visual Studio C++ toolchain. Install it via command line:

**Using winget (Windows Package Manager):**
```powershell
winget install Microsoft.VisualStudio.2022.BuildTools --silent --override "--wait --quiet --add Microsoft.VisualStudio.Workload.VCTools --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 --add Microsoft.VisualStudio.Component.Windows11SDK.22621 --includeRecommended"
```

**Alternative: Direct download and install:**
```powershell
# Download the installer
$installerUrl = "https://aka.ms/vs/17/release/vs_buildtools.exe"
$installerPath = "$env:TEMP\vs_buildtools.exe"
Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath

# Install with C++ workload
Start-Process -FilePath $installerPath -ArgumentList `
  "--quiet", "--wait", "--norestart", `
  "--add", "Microsoft.VisualStudio.Workload.VCTools", `
  "--add", "Microsoft.VisualStudio.Component.VC.Tools.x86.x64", `
  "--add", "Microsoft.VisualStudio.Component.Windows11SDK.22621", `
  "--includeRecommended" `
  -Wait -NoNewWindow
```

**Verify installation:**
```powershell
# Check if C++ compiler is available
where.exe cl.exe

# Check if linker is available
where.exe link.exe
```

#### Build Native AoT Binary

Once the C++ build tools are installed:

```bash
dotnet publish --configuration Release
```

The output will be a single native executable in `bin\Release\net10.0\win-x64\publish\`

**Project AoT Configuration:**
The project is configured with Native AoT settings in Release mode:
- `PublishAot=true` - Enables Native AoT compilation
- `SelfContained=true` - Produces standalone executable (required for AoT)
- `PublishTrimmed=true` - Trims unused code
- `InvariantGlobalization=true` - Reduces globalization overhead
- `PublishSingleFile=true` - Produces single executable file
- Warnings as errors for IL trimming and AoT compatibility

---

### Notes
- The logging bridge is isolated to `Program.cs`. SimpleDiscordNet logs are forwarded to `Microsoft.Extensions.Logging` via event handlers.
- You can tweak logging verbosity with `SetMinimumLevel(...)` or by adding category filters.
- This demo showcases the most common features of SimpleDiscordNet including slash commands, components, modals, ambient data access, and event handling.
- The project targets .NET 10.0 and is fully compatible with Native AoT compilation for optimal performance.
- All command handlers use only `InteractionContext` methods for responses - no direct bot instance access is required in command classes.
