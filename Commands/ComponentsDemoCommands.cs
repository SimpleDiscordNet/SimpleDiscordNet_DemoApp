using SimpleDiscordNet;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Primitives;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates the new Model/Component system (buttons, selects, modals)
/// and the AutoDefer attribute behaviors.
/// </summary>
[SlashCommandGroup("components", "Components & Modals demo using the new model system")]
public sealed class ComponentsDemoCommands
{
    /// <summary>
    /// Renders a message with interactive buttons.
    /// Clicks are handled by methods marked with [ComponentHandler].
    /// </summary>
    [SlashCommand("show", "Show a message with interactive buttons")]
    public async Task ShowAsync(InteractionContext ctx)
    {
        IComponent[] comps =
        [
            new Button("Ping", "components:ping", style: 1),    // Primary
            new Button("Open Modal", "components:openmodal", style: 2), // Secondary
            new Button("Danger", "components:danger", style: 4) // Danger
        ];

        var builder = new MessageBuilder()
            .WithContent("Component demo: click a button.")
            .WithComponents(comps);
        await ctx.RespondAsync(builder);
    }

    /// <summary>
    /// Shows a string select menu.
    /// </summary>
    [SlashCommand("select", "Show a string select menu")]
    public async Task SelectAsync(InteractionContext ctx)
    {
        IComponent[] comps =
        [
            new StringSelect(
                customId: "components:select",
                options:
                [
                    new SelectOption("Red", "red"),
                    new SelectOption("Green", "green"),
                    new SelectOption("Blue", "blue")
                ],
                placeholder: "Pick a color",
                min: 1,
                max: 1
            )
        ];

        var builder = new MessageBuilder()
            .WithContent("Select demo: choose a color.")
            .WithComponents(comps);
        await ctx.RespondAsync(builder);
    }

    /// <summary>
    /// Opens a modal from a slash command. Do not defer so modal can be the initial response.
    /// </summary>
    [SlashCommand("modal", "Open a modal with text inputs")]
    [NoDefer]
    public Task ModalAsync(InteractionContext ctx)
    {
        return ctx.OpenModalAsync(customId: "modal:feedback", title: "Feedback",
            new ActionRow(new TextInput("subject", "Subject", style: 1, required: true, maxLength: 100)),
            new ActionRow(new TextInput("message", "Message", style: 2, required: true, maxLength: 1000, placeholder: "Type your feedback here"))
        );
    }

    // --- Component handlers ---

    [ComponentHandler("components:ping")]
    public async Task OnPingAsync(InteractionContext ctx)
    {
        // For message component clicks, auto-defer is enabled by default; we update the original message.
        await ctx.UpdateMessageAsync("Pong! ✅");
    }

    [ComponentHandler("components:danger")]
    public async Task OnDangerAsync(InteractionContext ctx) => await ctx.UpdateMessageAsync("You clicked Danger! ⚠️");
    
    // Handle string select choice for colors
    [ComponentHandler("components:select")]
    public async Task OnSelectColorAsync(InteractionContext ctx)
    {
        string value = ctx.SelectedValues.FirstOrDefault() ?? "(none)";
        string label = value switch
        {
            "red" => "Red",
            "green" => "Green",
            "blue" => "Blue",
            _ => value
        };

        // Keep the select menu so users can change their selection again
        IComponent[] comps =
        [
            new StringSelect(
                customId: "components:select",
                options:
                [
                    new SelectOption("Red", "red"),
                    new SelectOption("Green", "green"),
                    new SelectOption("Blue", "blue")
                ],
                placeholder: "Pick a color",
                min: 1,
                max: 1
            )
        ];

        await ctx.UpdateMessageAsync($"Selected color: {label}", comps);
    }
    

    // To open a modal from a component click, do not defer; modals cannot be opened after deferral.
    [ComponentHandler("components:openmodal")]
    [NoDefer]
    public Task OnOpenModalAsync(InteractionContext ctx)
    {
        return ctx.OpenModalAsync(
            customId: "modal:feedback",
            title: "Feedback",
            new ActionRow(new TextInput("subject", "Subject", style: 1, required: true, maxLength: 100)),
            new ActionRow(new TextInput("message", "Message", style: 2, required: true, maxLength: 1000, placeholder: "Type your feedback here"))
        );
    }

    // Modal submission handler (customId must match the modal's customId)
    [ComponentHandler("modal:feedback")]
    public async Task OnFeedbackModalAsync(InteractionContext ctx)
    {
        string subject = ctx.Modal?.Inputs.FirstOrDefault(i => i.CustomId == "subject")?.Value ?? "(none)";
        string message = ctx.Modal?.Inputs.FirstOrDefault(i => i.CustomId == "message")?.Value ?? "(none)";

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle($"Feedback: {subject}")
            .WithDescription(message)
            .WithColor(DiscordColor.Yellow);

        await ctx.RespondAsync("Thanks for your feedback!", embed);
    }
}
