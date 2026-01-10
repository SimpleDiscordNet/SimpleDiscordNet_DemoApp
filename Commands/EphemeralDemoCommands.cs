using SimpleDiscordNet;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates the ephemeral parameter functionality with various argument types.
/// Usage: /ephemeral demo
/// </summary>
[SlashCommandGroup("ephemeral", "Demonstrates ephemeral (private) vs public responses")]
public sealed class EphemeralDemoCommands
{
    /// <summary>
    /// Demonstrates ephemeral responses with multiple string arguments and a boolean option.
    /// When ephemeral is true, only the command user can see the response.
    /// When ephemeral is false (default), everyone in the channel can see the response.
    /// </summary>
    [SlashCommand("demo", "Send a message with optional ephemeral flag")]
    public async Task DemoAsync(
        InteractionContext ctx,
        [CommandOption("message", "The message to send")] string message,
        [CommandOption("title", "Optional title for the embed")] string? title = null,
        [CommandOption("ephemeral", "Make the response visible only to you (default: false)")] bool? ephemeral = null)
    {
        bool isEphemeral = ephemeral ?? false;

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(title ?? "Ephemeral Demo")
            .WithDescription(message)
            .WithColor(isEphemeral ? DiscordColor.Yellow : DiscordColor.Green)
            .AddField("Visibility", isEphemeral ? "🔒 Private (only you can see this)" : "🌍 Public (everyone can see this)", inline: false)
            .AddField("Ephemeral Parameter", isEphemeral.ToString(), inline: true);

        if (!string.IsNullOrWhiteSpace(title))
        {
            embed.AddField("Title Provided", title, inline: true);
        }

        await ctx.RespondAsync("Here's your message:", embed, ephemeral: isEphemeral);
    }

    /// <summary>
    /// Demonstrates ephemeral responses with numeric arguments.
    /// </summary>
    [SlashCommand("calculate", "Perform a simple calculation and choose visibility")]
    public async Task CalculateAsync(
        InteractionContext ctx,
        [CommandOption("number1", "First number")] double num1,
        [CommandOption("number2", "Second number")] double num2,
        [CommandOption("operation", "Operation to perform")] string? operation = null,
        [CommandOption("ephemeral", "Make the response visible only to you (default: true)")] bool? ephemeral = null)
    {
        string op = operation ?? "add";
        bool isEphemeral = ephemeral ?? true;

        double result = op.ToLower() switch
        {
            "add" or "+" => num1 + num2,
            "subtract" or "-" => num1 - num2,
            "multiply" or "*" => num1 * num2,
            "divide" or "/" => num2 != 0 ? num1 / num2 : double.NaN,
            _ => double.NaN
        };

        string resultText = double.IsNaN(result)
            ? "❌ Invalid operation or division by zero"
            : $"✅ Result: **{result}**";

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("🧮 Calculator")
            .WithDescription(resultText)
            .AddField("Expression", $"{num1} {op} {num2}", inline: false)
            .AddField("Visibility", isEphemeral ? "🔒 Private" : "🌍 Public", inline: true)
            .WithColor(isEphemeral ? DiscordColor.Blue : DiscordColor.Purple);

        await ctx.RespondAsync("Calculation result:", embed, ephemeral: isEphemeral);
    }

    /// <summary>
    /// Demonstrates ephemeral with choice parameters.
    /// </summary>
    [SlashCommand("greet", "Send a greeting in different styles")]
    public async Task GreetAsync(
        InteractionContext ctx,
        [CommandOption("name", "Name to greet")] string name,
        [CommandOption("style", "Greeting style")]
        [CommandChoice("Formal", "formal")]
        [CommandChoice("Casual", "casual")]
        [CommandChoice("Enthusiastic", "enthusiastic")]
        string? style = null,
        [CommandOption("ephemeral", "Make the response visible only to you (default: false)")] bool? ephemeral = null)
    {
        string greetingStyle = style ?? "casual";
        bool isEphemeral = ephemeral ?? false;

        string greeting = greetingStyle.ToLower() switch
        {
            "formal" => $"Good day, {name}. It is a pleasure to make your acquaintance.",
            "casual" => $"Hey {name}, what's up?",
            "enthusiastic" => $"OMG HI {name.ToUpper()}!!! SO HAPPY TO SEE YOU!!! 🎉🎊✨",
            _ => $"Hello, {name}!"
        };

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("👋 Greeting")
            .WithDescription(greeting)
            .AddField("Style", greetingStyle, inline: true)
            .AddField("Visibility", isEphemeral ? "🔒 Private" : "🌍 Public", inline: true)
            .WithColor(DiscordColor.Teal);

        await ctx.RespondAsync("Here's your greeting:", embed, ephemeral: isEphemeral);
    }
}
