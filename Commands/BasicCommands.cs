using SimpleDiscordNet;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Basic ungrouped command demo.
/// Usage: /hello
/// </summary>
public sealed class BasicCommands
{
    /// <summary>
    /// Responds with a friendly greeting and an example embed to demonstrate
    /// a simple ungrouped slash command.
    /// </summary>
    [SlashCommand("hello", "Say hello and send an example embed reply")]
    public async Task HelloAsync(InteractionContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Hello from SimpleDiscordNet")
            .WithDescription("This is an example embed from the /hello command.")
            .WithColor(DiscordColor.Green);

        await ctx.RespondAsync("Hello! 👋", embed);
    }
}
