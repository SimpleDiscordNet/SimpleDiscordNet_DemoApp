using SimpleDiscordNet;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Context;
using SimpleDiscordNet.Primitives;

namespace SimpleDiscordNet_DemoApp.Commands;

/// <summary>
/// Demonstrates permissions and role management features.
/// Usage: /permissions demo, /roles demo, /roles list
/// </summary>
public sealed class PermissionsRolesDemoCommands
{
    /// <summary>
    /// Demonstrates checking permissions for roles and members.
    /// </summary>
    [SlashCommand("permissions", "Check permissions for the current user")]
    public async Task PermissionsAsync(InteractionContext ctx)
    {
        string? guildId = ctx.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).");
            return;
        }

        try
        {
            // Get the member who invoked the command
            ulong userId = ctx.UserId;
            var member = DiscordContext.GetMember(ulong.Parse(guildId), userId);

            if (member == null)
            {
                await ctx.RespondAsync("❌ Could not find member data.");
                return;
            }

            // Check various permissions
            var roles = DiscordContext.GetRolesInGuild(ulong.Parse(guildId));
            var memberRoles = roles.Where(r => member.Roles?.Contains(r.Id) == true).ToList();

            bool isAdmin = memberRoles.Any(r => r.IsAdministrator);
            bool canManageChannels = memberRoles.Any(r => r.HasPermission(PermissionFlags.ManageChannels));
            bool canManageRoles = memberRoles.Any(r => r.HasPermission(PermissionFlags.ManageRoles));
            bool canKick = memberRoles.Any(r => r.HasPermission(PermissionFlags.KickMembers));
            bool canBan = memberRoles.Any(r => r.HasPermission(PermissionFlags.BanMembers));

            string displayName = member.DisplayName;

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🔐 Permissions for {displayName}")
                .WithDescription("Checking permissions based on your roles:")
                .AddField("Administrator", isAdmin ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Manage Channels", canManageChannels ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Manage Roles", canManageRoles ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Kick Members", canKick ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Ban Members", canBan ? "✅ Yes" : "❌ No", inline: true)
                .AddField("Role Count", memberRoles.Count.ToString(), inline: true)
                .WithColor(isAdmin ? DiscordColor.Yellow : DiscordColor.Blue);

            await ctx.RespondAsync("Here are your permissions:", embed);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error checking permissions: {ex.Message}");
        }
    }

}

/// <summary>
/// Demonstrates role management features.
/// Usage: /roles list, /roles demo
/// </summary>
[SlashCommandGroup("roles", "Role management demo commands")]
public sealed class RolesDemoCommands
{
    /// <summary>
    /// Lists all roles in the current guild with their permissions.
    /// </summary>
    [SlashCommand("list", "List all roles in this guild")]
    public async Task ListRolesAsync(InteractionContext ctx)
    {
        string? guildId = ctx.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).");
            return;
        }

        try
        {
            var roles = DiscordContext.GetRolesInGuild(ulong.Parse(guildId))
                .OrderByDescending(r => r.Position)
                .Take(10)
                .ToList();

            if (roles.Count == 0)
            {
                await ctx.RespondAsync("❌ No roles found in this guild.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🎭 Roles in this Guild (Top 10)")
                .WithColor(DiscordColor.Purple);

            foreach (var role in roles)
            {
                string permInfo = role.IsAdministrator
                    ? "👑 Administrator"
                    : $"Perms: {role.Permissions}";

                embed.AddField(
                    role.Name,
                    $"ID: {role.Id}\n{permInfo}",
                    inline: true
                );
            }

            await ctx.RespondAsync("Here are the roles in this guild:", embed);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error listing roles: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates role checking functionality.
    /// </summary>
    [SlashCommand("demo", "Check your roles in this guild")]
    public async Task RolesDemoAsync(InteractionContext ctx)
    {
        string? guildId = ctx.GuildId;

        if (string.IsNullOrWhiteSpace(guildId))
        {
            await ctx.RespondAsync("❌ This command can only be used in a guild (server).");
            return;
        }

        try
        {
            ulong guildIdUlong = ulong.Parse(guildId);
            ulong userId = ctx.UserId;

            // Debug: Check total members in cache
            var allMembers = DiscordContext.GetMembersInGuild(guildIdUlong);
            int totalMembersInCache = allMembers.Count();

            var member = DiscordContext.GetMember(guildIdUlong, userId);

            if (member == null)
            {
                await ctx.RespondAsync($"❌ Could not find member data for user {userId} in guild {guildId}.\nTotal members in cache: {totalMembersInCache}");
                return;
            }

            var allRoles = DiscordContext.GetRolesInGuild(guildIdUlong);
            var memberRoleIds = member.Roles ?? Array.Empty<ulong>();

            // Check if the guild object has roles populated
            var guild = DiscordContext.GetGuild(guildIdUlong);
            int rolesInGuildObject = guild?.Roles?.Length ?? 0;

            string displayName = member.DisplayName;

            // Debug info
            string debugInfo = $"Member: {displayName}\nUser ID: {userId}\nMember Role IDs: [{string.Join(", ", memberRoleIds)}]\nRoles in DiscordContext: {allRoles.Count()}\nRoles in Guild object: {rolesInGuildObject}";

            var memberRoles = allRoles.Where(r => memberRoleIds.Contains(r.Id)).ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"🎭 Roles for {displayName}")
                .WithDescription($"You have {memberRoles.Count} role(s) in this guild:")
                .WithColor(DiscordColor.Green)
                .AddField("Debug Info", $"```\n{debugInfo}\n```", inline: false);

            if (memberRoles.Count > 0)
            {
                foreach (var role in memberRoles.Take(10))
                {
                    embed.AddField(
                        role.Name,
                        $"ID: {role.Id}\nPosition: {role.Position}",
                        inline: true
                    );
                }

                if (memberRoles.Count > 10)
                {
                    embed.AddField("...", $"And {memberRoles.Count - 10} more roles", inline: false);
                }
            }
            else
            {
                embed.AddField("No Roles", "No roles found for this member (besides @everyone)", inline: false);
            }

            await ctx.RespondAsync("Here are your roles:", embed);
        }
        catch (Exception ex)
        {
            await ctx.RespondAsync($"❌ Error checking roles: {ex.Message}\n\nStack: {ex.StackTrace}");
        }
    }
}
