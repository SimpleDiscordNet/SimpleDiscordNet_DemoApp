// Sample app for SimpleDiscordNet
// --------------------------------
// This program demonstrates:
// - Creating and starting a Discord bot using SimpleDiscordNet
// - Slash commands (ungrouped and grouped with subcommands)
// - Components (buttons, selects) and modals with handlers
// - Sending messages and embeds (both via slash replies and direct channel send)
// - Ambient data access via DiscordContext with [DiscordContext] attribute
// - Event handling (DMs, connection, etc.)
// - Development mode for instant command syncing
//
// How to run:
// 1) Create a discord_token.txt file (see discord_token.txt.example) with:
//    - DISCORD_TOKEN    : your bot token
//    - DEV_GUILD_ID     : a guild ID where you have installed the bot (used for instant dev sync)
//    - DEMO_CHANNEL_ID  : optional channel ID to demonstrate direct channel sending
// 2) Start the app. On first run (in development mode), slash commands are synced immediately to DEV_GUILD_ID.
// 3) In Discord, try:
//    - /hello
//    - /demo text, /demo embed
//    - /ambient info
//    - /components show, /components select, /components modal
//    - /messages text, /messages embed, /messages complex
//    - /permissions, /roles list, /roles demo
//    - /channels list, /channels info, /channels types

using Microsoft.Extensions.Logging;
using SimpleDiscordNet;
using SimpleDiscordNet.Context;
using SimpleDiscordNet.Events;
using SimpleDiscordNet_DemoApp.Services;
using SimpleDiscordNet_DemoApp.Internal;

// Commands moved to folder: Commands\*.cs

/// <summary>
/// Program entry point for the SimpleDiscordNet demo application.
/// <para>
/// Responsibilities:
/// - Builds and starts the Discord bot using SimpleDiscordNet.
/// - Registers example slash command modules.
/// - Optionally syncs commands to a development guild prior to connecting.
/// - Demonstrates forwarding SimpleDiscordNet logs into Microsoft.Extensions.Logging with a console sink.
/// - Optionally sends a direct channel message after startup to demonstrate REST sending.
/// </para>
/// </summary>
public sealed class Program
{
    /// <summary>
    /// Application entry point. Configures logging, builds the bot, wires events to ILogger,
    /// registers demo commands, starts the gateway connection, and blocks until Ctrl+C.
    /// </summary>
    public static async Task Main()
    {
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

        // Read configuration from discord_token.txt
        ConfigurationReader config = new();
        string configFile = "discord_token.txt";

        if (!config.Load(configFile))
        {
            logger.LogError("Configuration file 'discord_token.txt' not found. Please create it based on discord_token.txt.example");
            return;
        }

        string? token = config.GetValue("DISCORD_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogError("DISCORD_TOKEN not found in discord_token.txt. Please set your bot token.");
            return;
        }

        string? devGuildId = config.GetValue("DEV_GUILD_ID");
        string? demoChannelId = config.GetValue("DEMO_CHANNEL_ID");

        // Build the bot
        DiscordBot bot = DiscordBot.NewBuilder()
            .WithToken(token)
            // Intents: request only what you need. For Members ambient data, include GuildMembers.
            .WithIntents(DiscordIntents.Guilds | DiscordIntents.GuildMembers | DiscordIntents.DirectMessages | DiscordIntents.GuildMessages)
            // Preload guilds, channels, and members on start to populate DiscordContext cache
            .WithPreloadOnStart(guilds: true, channels: true, members: true)
            // Enable dev mode so slash commands sync instantly to your dev guild before connecting
            .WithDevelopmentMode(true)
            // Conditionally add development guild without introducing a temporary builder variable
            .If(!string.IsNullOrWhiteSpace(devGuildId), b => b.WithDevelopmentGuild(devGuildId!))
            .Build();

        // Forward SimpleDiscordNet native logs into Microsoft ILogger (console sink configured above)
        DiscordEvents.Log += (_, m) =>
        {
            string levelName = m.Level.ToString();
            LogLevel msLevel = levelName switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Information" or "Info" => LogLevel.Information,
                "Warning" or "Warn" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" or "Fatal" => LogLevel.Critical,
                _ => LogLevel.Information
            };

            // If the log carries an Exception (commonly for Error/Fatal), include it directly.
            if (m.Exception is not null)
            {
                logger.Log(msLevel, m.Exception, "{Message}", m.Message);
            }
            else
            {
                logger.Log(msLevel, "{Message}", m.Message);
            }
        };
        DiscordEvents.Connected += (_, __) => logger.LogInformation("Connected to Discord Gateway.");
        DiscordEvents.Error += (_, ex) => logger.LogError(ex, "SimpleDiscordNet reported an error");
        DiscordEvents.GuildAdded += (_, guildEvent) =>
        {
            var members = DiscordContext.GetMembersInGuild(guildEvent.Guild.Id);
            logger.LogInformation("GUILD_ADDED event fired for guild: {GuildId} ({GuildName}). Members in cache: {MemberCount}",
                guildEvent.Guild.Id, guildEvent.Guild.Name, members.Count());
        };
        DiscordEvents.GuildReady += (_, guildEvent) =>
        {
            var members = DiscordContext.GetMembersInGuild(guildEvent.Guild.Id);
            var channels = DiscordContext.GetChannelsInGuild(guildEvent.Guild.Id);
            var roles = DiscordContext.GetRolesInGuild(guildEvent.Guild.Id);
            logger.LogInformation("GUILD_READY event fired for guild: {GuildId} ({GuildName}). All data loaded - Members: {MemberCount}, Channels: {ChannelCount}, Roles: {RoleCount}",
                guildEvent.Guild.Id, guildEvent.Guild.Name, members.Count(), channels.Count(), roles.Count());
        };

        // DM's to the bot can be handled many ways, or we can just log it like this.
        DiscordEvents.DirectMessageReceived += (_, msgEvent) =>
        {
            logger.LogInformation($"Received From: {msgEvent.Message.Author.Username}\nMessage: {msgEvent.Message.Content}");
        };

        // Start the bot and keep the process alive until Ctrl+C
        CancellationTokenSource cts = new ();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        await bot.StartAsync(cts.Token);

        // Timer-driven ambient snapshot logger (diagnostic/demo)
        int intervalSeconds = 60;
        if (int.TryParse(Environment.GetEnvironmentVariable("AMBIENT_TICK_SECONDS"), out int s) && s > 0)
            intervalSeconds = s;

        AmbientSnapshotService ambientService = new ();
        Timer timer = new (_ =>
        {
            // Fire-and-forget; the work is small and purely logging
            _ = ambientService.RunOnceAsync(cts.Token);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));

        // Optional: demonstrate direct channel send via REST helper when a DEMO_CHANNEL_ID is provided
        if (!string.IsNullOrWhiteSpace(demoChannelId))
        {
            try
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("Direct Channel Send")
                    .WithDescription("This message was sent via bot.SendMessageAsync after startup.")
                    .WithColor(DiscordColor.Teal);

                await bot.SendMessageAsync(demoChannelId!, "Hello from the testing app!", embed, cts.Token);
                logger.LogInformation("Sent a demo message to channel {ChannelId}.", demoChannelId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send demo message to channel {ChannelId}", demoChannelId);
            }
        }

        // Block until Ctrl+C
        try { await Task.Delay(Timeout.Infinite, cts.Token); }
        catch (OperationCanceledException) { /* expected on Ctrl+C */ }

        // Clean up timer before stopping the bot
        await timer.DisposeAsync();
        await bot.StopAsync();
    }
}