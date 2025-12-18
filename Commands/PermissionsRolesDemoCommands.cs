using SimpleDiscordNet;
using SimpleDiscordNet.Attributes;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Context;
using SimpleDiscordNet.Primitives;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates permissions and role management features.
/// Usage: /permissions demo, /roles demo, /roles list
/// </summary>
[DiscordContext]
public sealed class PermissionsRolesDemoCommands
{
    /// <summary>
    /// Demonstrates checking permissions for roles and members.
    /// </summary>
    [SlashCommand("permissions", "Check permissions for the current user")]
    public async Task PermissionsAsync(InteractionContext ctx)
    {
        string? guildId = ctx.Event.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).", ephemeral: true);
            return;
        }

        try
        {
            // Get the member who invoked the command
            var member = DiscordContext.GetMember(guildId, ctx.User?.Id ?? "");

            if (member == null)
            {
                await ctx.RespondAsync("❌ Could not find member data.", ephemeral: true);
                return;
            }

            // Check various permissions
            var roles = DiscordContext.GetRolesInGuild(guildId);
            var memberRoles = roles.Where(r => member.Member.Roles?.Contains(r.Role.Id) == true).ToList();

            bool isAdmin = memberRoles.Any(r => r.IsAdministrator);
            bool canManageChannels = memberRoles.Any(r => r.HasPermission(PermissionFlags.ManageChannels));
            bool canManageRoles = memberRoles.Any(r => r.HasPermission(PermissionFlags.ManageRoles));
            bool canKick = memberRoles.Any(r => r.HasPermission(PermissionFlags.KickMembers));
            bool canBan = memberRoles.Any(r => r.HasPermission(PermissionFlags.BanMembers));

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🔐 Permissions for {member.DisplayName}")
                .WithDescription("Checking permissions based on your roles:")
                .AddField("Administrator", isAdmin ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Manage Channels", canManageChannels ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Manage Roles", canManageRoles ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Kick Members", canKick ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Ban Members", canBan ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Role Count", memberRoles.Count.ToString(), inline: true)
                .WithColor(isAdmin ? DiscordColor.Yellow : DiscordColor.Blue);

            await ctx.RespondAsync("Here are your permissions:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error checking permissions: {ex.Message}", ephemeral: true);
        }
    }

}

/// <summary>
/// Demonstrates role management features.
/// Usage: /roles list, /roles demo
/// </summary>
[DiscordContext]
[SlashCommandGroup("roles", "Role management demo commands")]
public sealed class RolesDemoCommands
{
    /// <summary>
    /// Lists all roles in the current guild with their permissions.
    /// </summary>
    [SlashCommand("list", "List all roles in this guild")]
    public async Task ListRolesAsync(InteractionContext ctx)
    {
        string? guildId = ctx.Event.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).", ephemeral: true);
            return;
        }

        try
        {
            var roles = DiscordContext.GetRolesInGuild(guildId)
                .OrderByDescending(r => r.Role.Position)
                .Take(10)
                .ToList();

            if (roles.Count == 0)
            {
                await ctx.RespondAsync("❌ No roles found in this guild.", ephemeral: true);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🎭 Roles in this Guild (Top 10)")
                .WithColor(DiscordColor.Purple);

            foreach (var role in roles)
            {
                string permInfo = role.IsAdministrator
                    ? "👑 Administrator"
                    : $"Perms: {role.Role.Permissions}";

                embed.AddField(
                    role.Role.Name,
                    $"ID: {role.Role.Id}\n{permInfo}",
                    inline: true
                );
            }

            await ctx.RespondAsync("Here are the roles in this guild:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error listing roles: {ex.Message}", ephemeral: true);
        }
    }

    /// <summary>
    /// Demonstrates role checking functionality.
    /// </summary>
    [SlashCommand("demo", "Check your roles in this guild")]
    public async Task RolesDemoAsync(InteractionContext ctx)
    {
        string? guildId = ctx.Event.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).", ephemeral: true);
            return;
        }

        try
        {
            var member = DiscordContext.GetMember(guildId, ctx.User?.Id ?? "");

            if (member == null)
            {
                await ctx.RespondAsync("❌ Could not find member data.", ephemeral: true);
                return;
            }

            var allRoles = DiscordContext.GetRolesInGuild(guildId);
            var memberRoleIds = member.Member.Roles ?? Array.Empty<string>();
            var memberRoles = allRoles.Where(r => memberRoleIds.Contains(r.Role.Id)).ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🎭 Roles for {member.DisplayName}")
                .WithDescription($"You have {memberRoles.Count} role(s) in this guild:")
                .WithColor(DiscordColor.Green);

            foreach (var role in memberRoles.Take(10))
            {
                embed.AddField(
                    role.Role.Name,
                    $"Position: {role.Role.Position}",
                    inline: true
                );
            }

            if (memberRoles.Count > 10)
            {
                embed.AddField("...", $"And {memberRoles.Count - 10} more roles", inline: false);
            }

            await ctx.RespondAsync("Here are your roles:", embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error checking roles: {ex.Message}", ephemeral: true);
        }
    }
}
