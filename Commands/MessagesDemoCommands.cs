using SimpleDiscordNet;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates message and embed features via interaction responses.
/// Usage: /messages text, /messages embed, /messages complex
/// </summary>
[SlashCommandGroup("messages", "Message and embed demo commands")]
public sealed class MessagesDemoCommands
{
    /// <summary>
    /// Sends a simple text response.
    /// </summary>
    [SlashCommand("text", "Send a simple text message")]
    public async Task TextAsync(InteractionContext ctx)
    {
        await ctx.RespondAsync("📝 This is a simple text message response.");
    }

    /// <summary>
    /// Sends a message with an embed.
    /// </summary>
    [SlashCommand("embed", "Send a message with an embed")]
    public async Task EmbedAsync(InteractionContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("📨 Embed Demo")
            .WithDescription("This is a rich embed message with multiple fields")
            .WithColor(DiscordColor.Blue)
            .AddField("Field 1", "Value 1", inline: true)
            .AddField("Field 2", "Value 2", inline: true)
            .AddField("Field 3", "Value 3", inline: false);

        await ctx.RespondAsync("Here's an embed:", embed);
    }

    /// <summary>
    /// Sends a complex embed with multiple features.
    /// </summary>
    [SlashCommand("complex", "Send a complex embed with multiple features")]
    public async Task ComplexAsync(InteractionContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("🎨 Complex Embed Demo")
            .WithDescription("This embed demonstrates multiple embed features available in SimpleDiscordNet")
            .WithColor(DiscordColor.Purple)
            .AddField("Text Formatting", "**Bold**, *italic*, __underline__, ~~strikethrough~~", inline: false)
            .AddField("Inline Field 1", "These fields", inline: true)
            .AddField("Inline Field 2", "appear side by side", inline: true)
            .AddField("Inline Field 3", "in a row", inline: true)
            .AddField("Links", "[Click here](https://discord.com)", inline: false)
            .AddField("Code Blocks", "`inline code` or ```multiline code```", inline: false);

        await ctx.RespondAsync("Complex embed with multiple features:", embed, ephemeral: false);
    }
}
