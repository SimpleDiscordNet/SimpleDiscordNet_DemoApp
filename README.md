### SimpleDiscordNet Demo App

This repository is a minimal end-to-end demo of building a Discord bot with `SimpleDiscordNet` and wiring all logs into `Microsoft.Extensions.Logging` with a console sink.

#### What this demo shows
- Establishing a bot using `SimpleDiscordNet` and a fluent builder.
- Forwarding SimpleDiscordNet’s native logs (`DiscordEvents.Log` / `DiscordEvents.Error`) into `ILogger`.
- Configuring an `ILogger` console sink (`AddSimpleConsole`) with timestamps and single-line output.
- Registering slash commands:
  - Ungrouped command: `/hello`
  - Grouped commands: `/demo text` and `/demo embed`
  - Ambient data command: `/ambient info`
  - Components & Modals: `/components show` (buttons), `/components select` (string select), and `/components modal` (opens a modal)
- Accessing cached ambient data via `DiscordContext` with attributes (`[DiscordGuilds]`, `[DiscordChannels]`, `[DiscordMembers]`, `[DiscordUsers]`).
- Optional: sending a direct channel message after startup to demonstrate REST sending.
 - Using `AutoDefer` attributes to control interaction deferral for slash, component, and modal handlers.

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
- `AmbientDemoCommands` demonstrates ambient data with `/ambient info`, using attributes to opt-in to cached datasets and replying ephemerally with counts.
- `ComponentsDemoCommands` demonstrates message components, modals, and `AutoDefer`:
  - `/components show` sends three buttons (Ping, Open Modal, Danger). Clicking updates the original message; the "Open Modal" button uses a handler with `[AutoDefer(false)]` to open a modal immediately.
  - `/components select` sends a string select menu; choosing an option updates the original message.
  - `/components modal` (slash) opens a modal with two inputs; the slash handler uses `[AutoDefer(false)]` because modals cannot be opened after deferral.

#### 4) Ambient snapshot service (diagnostic)
`AmbientSnapshotService` periodically prints a compact line with raw and de-duplicated counts of cached guilds/channels/members/users. The interval defaults to 60s and can be overridden with the `AMBIENT_TICK_SECONDS` environment variable.

---

### Running the demo

1. Open the solution and restore/build.
2. Set the following values in `Program.cs`:
   - `token`: your bot token
   - `devGuildId`: a guild ID where your bot is installed (for instant dev sync)
   - `demoChannelId` (optional): a channel ID to demonstrate the direct send after startup

   Important: keep your real bot token secret and never commit it.

3. Run the app. On first run (in development mode), slash commands are synced to your dev guild. In Discord, try:
   - `/hello`
   - `/demo text`
   - `/demo embed`
   - `/ambient info`
   - `/components show`
   - `/components select`
   - `/components modal`

4. Observe logs in the console with timestamps and levels, including any forwarded exceptions.

Optional: set `AMBIENT_TICK_SECONDS` to a positive integer to change how often the ambient snapshot line is printed.

---

### Notes
- The logging bridge is isolated to `Program.cs`. If SimpleDiscordNet later exposes a direct logger sink API, you can replace the event-based wiring without changing your `ILogger` pipeline.
- You can tweak logging verbosity with `SetMinimumLevel(...)` or by adding category filters.
- This demo intentionally keeps infrastructure minimal to focus on bot startup, commands, ambient data, and logging.
