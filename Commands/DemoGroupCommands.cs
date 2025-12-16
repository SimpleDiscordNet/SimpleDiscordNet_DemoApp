using SimpleDiscordNet;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demo group with two subcommands: /demo text and /demo embed
/// </summary>
[SlashCommandGroup("demo", "Demo commands for text and embed replies")]
public sealed class DemoGroupCommands
{
    /// <summary>
    /// Sends a plain text reply to demonstrate a simple subcommand response.
    /// </summary>
    [SlashCommand("text", "Send a plain text reply")]
    public async Task TextAsync(InteractionContext ctx)
    {
        await ctx.RespondAsync("This is a plain text reply from /demo text.");
    }

    /// <summary>
    /// Sends an embed reply to demonstrate building and returning rich content.
    /// </summary>
    [SlashCommand("embed", "Send an embed reply")]
    public async Task EmbedAsync(InteractionContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Demo Embed")
            .WithDescription("This embed was sent from /demo embed.")
            .WithColor(DiscordColor.Blue);

        await ctx.RespondAsync("Here is an embed example:", embed);
    }
}
