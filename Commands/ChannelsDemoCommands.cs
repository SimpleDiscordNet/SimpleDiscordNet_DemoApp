using SimpleDiscordNet;
using SimpleDiscordNet.Attributes;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Context;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates channel management and querying features.
/// Usage: /channels list, /channels info, /channels types
/// </summary>
[DiscordContext]
[SlashCommandGroup("channels", "Channel management demo commands")]
public sealed class ChannelsDemoCommands
{
    /// <summary>
    /// Lists all channels in the current guild.
    /// </summary>
    [SlashCommand("list", "List all channels in this guild")]
    public async Task ListAsync(InteractionContext ctx)
    {
        string? guildId = ctx.Event.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).", ephemeral: true);
            return;
        }

        try
        {
            var channels = DiscordContext.GetChannelsInGuild(guildId).Take(15).ToList();

            if (channels.Count == 0)
            {
                await ctx.RespondAsync("❌ No channels found in this guild.", ephemeral: true);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"📺 Channels in {channels[0].GuildName} (First 15)")
                .WithColor(DiscordColor.Blue);

            foreach (var channel in channels)
            {
                string typeIcon = channel.Type switch
                {
                    0 => "💬",  // Text
                    2 => "🔊",  // Voice
                    4 => "📁",  // Category
                    5 => "📢",  // Announcement
                    13 => "🎙️", // Stage
                    _ => "❓"
                };

                embed.AddField(
                    $"{typeIcon} {channel.Name}",
                    $"Type: {channel.Type} | ID: {channel.Id}",
                    inline: true
                );
            }

            await ctx.RespondAsync("Here are the channels:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error listing channels: {ex.Message}", ephemeral: true);
        }
    }

    /// <summary>
    /// Shows information about the current channel.
    /// </summary>
    [SlashCommand("info", "Show information about the current channel")]
    public async Task InfoAsync(InteractionContext ctx)
    {
        string? channelId = ctx.ChannelId;

        if (string.IsNullOrWhiteSpace(channelId))
        {
            await ctx.RespondAsync("❌ Could not determine channel ID.", ephemeral: true);
            return;
        }

        try
        {
            var channel = DiscordContext.GetChannel(channelId);

            if (channel == null)
            {
                await ctx.RespondAsync("❌ Could not find channel information.", ephemeral: true);
                return;
            }

            string typeDescription = channel.Type switch
            {
                0 => "Text Channel",
                2 => "Voice Channel",
                4 => "Category",
                5 => "Announcement Channel",
                10 => "Announcement Thread",
                11 => "Public Thread",
                12 => "Private Thread",
                13 => "Stage Channel",
                _ => $"Unknown ({channel.Type})"
            };

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"📺 Channel Information")
                .AddField("Name", channel.Name, inline: true)
                .AddField("Type", typeDescription, inline: true)
                .AddField("ID", channel.Id, inline: true)
                .AddField("Guild", channel.GuildName, inline: true)
                .AddField("Guild ID", channel.GuildId, inline: true)
                .WithColor(DiscordColor.Teal);

            await ctx.RespondAsync("Here are the channels:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error getting channel info: {ex.Message}", ephemeral: true);
        }
    }

    /// <summary>
    /// Shows a breakdown of channel types in the guild.
    /// </summary>
    [SlashCommand("types", "Show channel type breakdown for this guild")]
    public async Task TypesAsync(InteractionContext ctx)
    {
        string? guildId = ctx.Event.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).", ephemeral: true);
            return;
        }

        try
        {
            var allChannels = DiscordContext.GetChannelsInGuild(guildId);
            var categories = DiscordContext.GetCategoriesInGuild(guildId);
            var textChannels = DiscordContext.TextChannels.Where(c => c.GuildId == guildId).ToList();
            var voiceChannels = DiscordContext.VoiceChannels.Where(c => c.GuildId == guildId).ToList();
            var threads = DiscordContext.Threads.Where(c => c.GuildId == guildId).ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("📊 Channel Type Breakdown")
                .AddField("📁 Categories", categories.Count.ToString(), inline: true)
                .AddField("💬 Text Channels", textChannels.Count.ToString(), inline: true)
                .AddField("🔊 Voice Channels", voiceChannels.Count.ToString(), inline: true)
                .AddField("🧵 Threads", threads.Count.ToString(), inline: true)
                .AddField("📺 Total Channels", allChannels.Count.ToString(), inline: true)
                .WithColor(DiscordColor.Green);

            await ctx.RespondAsync("Here are the channels:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error analyzing channel types: {ex.Message}", ephemeral: true);
        }
    }
}
