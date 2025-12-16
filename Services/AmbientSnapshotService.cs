using SimpleDiscordNet.Attributes;
using SimpleDiscordNet.Context;
using SimpleDiscordNet.Entities;

namespace SimpleDiscordNet_DemoApp.Services;

/// <summary>
/// Periodic ambient snapshot logger. Demonstrates reading cached data via DiscordContext
/// and defensively deduplicating by stable IDs in case upstream sources produce duplicates.
/// </summary>
[DiscordContext]
public sealed class AmbientSnapshotService
{
    /// <summary>
    /// Reads ambient caches, deduplicates entities by ID, and writes a concise report to the console.
    /// Intended for timer-based diagnostics and demonstration only.
    /// </summary>
    public Task RunOnceAsync(CancellationToken ct = default)
    {
        // Distinct counts (defensive)
        int distinctGuilds = CountDistinct(DiscordContext.Guilds, static g => g.Id);
        int distinctChannels = CountDistinct(DiscordContext.Channels, static x => $"{x.Guild.Id}:{x.Channel.Id}");
        int distinctMembers = CountDistinct(DiscordContext.Members, static x => $"{x.Guild.Id}:{x.Member.User.Id}");
        int distinctUsers = CountDistinct(DiscordContext.Users, static x => $"{x.Guild.Id}:{x.User.Id}");

        // Raw counts
        int rawGuilds = DiscordContext.Guilds.Count;
        int rawChannels = DiscordContext.Channels.Count;
        int rawMembers = DiscordContext.Members.Count;
        int rawUsers = DiscordContext.Users.Count;

        // Log a compact line; include raw->distinct if they differ
        string FormatPair(int raw, int distinct) => raw == distinct ? raw.ToString() : $"{raw}→{distinct}";

        string tail = $" Guilds={FormatPair(rawGuilds, distinctGuilds)} | " +
                      $"Channels={FormatPair(rawChannels, distinctChannels)} | " +
                      $"Members={FormatPair(rawMembers, distinctMembers)} | " +
                      $"Users={FormatPair(rawUsers, distinctUsers)}";

        // Highlight the tag like an INFO log level in the console
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGreen; // typical color used for Information level
        Console.Write("[AmbientSnapshot]");
        Console.ForegroundColor = previousColor;
        Console.WriteLine(tail);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Counts the number of distinct items in <paramref name="source"/> by selecting a stable key with <paramref name="keySelector"/>.
    /// Uses a <see cref="HashSet{T}"/> to track seen keys.
    /// </summary>
    /// <typeparam name="T">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type used for deduplication.</typeparam>
    /// <param name="source">Input list to scan.</param>
    /// <param name="keySelector">Function to select a comparable key for each element.</param>
    /// <returns>The number of unique keys found in the source.</returns>
    private static int CountDistinct<T, TKey>(IReadOnlyList<T> source, Func<T, TKey> keySelector)
    {
        if (source.Count == 0) return 0;
        HashSet<TKey> set = [];
        for (int i = 0; i < source.Count; i++)
        {
            set.Add(keySelector(source[i]!));
        }
        return set.Count;
    }
}
