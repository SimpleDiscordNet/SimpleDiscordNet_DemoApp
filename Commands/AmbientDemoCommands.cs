using SimpleDiscordNet;
using SimpleDiscordNet.Attributes;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Context;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates ambient data access via DiscordContext using attributes.
/// Usage: /ambient info
/// </summary>
[DiscordContext]
[SlashCommandGroup("ambient", "Demonstrates ambient data access via DiscordContext")]
public sealed class AmbientDemoCommands
{
    /// <summary>
    /// Shows counts for cached guilds, channels, members, and users using ambient context data.
    /// Demonstrates the use of attributes to opt in to cached datasets and ephemeral replies.
    /// </summary>
    [SlashCommand("info", "Show cached counts for guilds/channels/members/users")]
    public async Task InfoAsync(InteractionContext ctx)
    {
        int guildCount = DiscordContext.Guilds.Count;
        int channelCount = DiscordContext.Channels.Count;
        int memberCount = DiscordContext.Members.Count;
        int userCount = DiscordContext.Users.Count;

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Ambient Data Snapshot")
            .WithDescription($"Guilds: {guildCount}\nChannels: {channelCount}\nMembers: {memberCount}\nUsers: {userCount}")
            .WithColor(DiscordColor.Purple);

        await ctx.RespondAsync("Ambient data (cached snapshot):", embed, ephemeral: true);
    }
}
