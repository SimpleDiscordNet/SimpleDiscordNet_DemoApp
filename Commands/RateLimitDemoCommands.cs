using SimpleDiscordNet;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.Rest;

namespace SimpleDiscordNet_DemoApp.Commands;

[SlashCommandGroup("ratelimit", "Rate limit monitoring demo commands")]
public sealed class RateLimitDemoCommands
{
    private static readonly object _lock = new();
    private static bool _eventsWired = false;
    private static readonly List<string> _recentEvents = new();
    private const int MaxEvents = 10;

    static RateLimitDemoCommands()
    {
        WireEvents();
    }

    private static void WireEvents()
    {
        lock (_lock)
        {
            if (_eventsWired) return;
            _eventsWired = true;

            RateLimitEventManager.BucketUpdated += (_, e) =>
            {
                lock (_recentEvents)
                {
                    _recentEvents.Add($"[{e.Timestamp:HH:mm:ss}] Bucket {e.BucketId}: {e.Remaining}/{e.Limit} remaining");
                    if (_recentEvents.Count > MaxEvents) _recentEvents.RemoveAt(0);
                }
            };

            RateLimitEventManager.PreEmptiveWait += (_, e) =>
            {
                lock (_recentEvents)
                {
                    _recentEvents.Add($"[{e.Timestamp:HH:mm:ss}] PreEmptiveWait: {e.BucketId} waiting {e.WaitDuration.TotalSeconds:F1}s");
                    if (_recentEvents.Count > MaxEvents) _recentEvents.RemoveAt(0);
                }
            };

            RateLimitEventManager.RateLimitHit += (_, e) =>
            {
                lock (_recentEvents)
                {
                    _recentEvents.Add($"[{e.Timestamp:HH:mm:ss}] **429 HIT**: {e.BucketId} (retry after {e.RetryAfter.TotalSeconds:F1}s)");
                    if (_recentEvents.Count > MaxEvents) _recentEvents.RemoveAt(0);
                }
            };

            RateLimitEventManager.RequestQueued += (_, e) =>
            {
                lock (_recentEvents)
                {
                    _recentEvents.Add($"[{e.Timestamp:HH:mm:ss}] Queued: {e.Route} (position {e.QueuePosition}/{e.QueueLength})");
                    if (_recentEvents.Count > MaxEvents) _recentEvents.RemoveAt(0);
                }
            };
        }
    }

    [SlashCommand("status", "Show recent rate limit activity")]
    public async Task StatusAsync(InteractionContext ctx)
    {
        List<string> snapshot;
        lock (_recentEvents)
        {
            snapshot = new List<string>(_recentEvents);
        }

        string eventLog = snapshot.Count > 0
            ? string.Join("\n", snapshot)
            : "No rate limit events recorded yet.";

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("🚦 Rate Limit Activity")
            .WithDescription($"```\n{eventLog}\n```")
            .WithColor(DiscordColor.Blue)
            .AddField("Info", "Rate limit events are tracked automatically. Use commands to generate activity.", inline: false);

        await ctx.RespondAsync("Recent rate limit activity:", embed);
    }

    [SlashCommand("info", "Explains the rate limiting system")]
    public async Task InfoAsync(InteractionContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("🚦 SimpleDiscordNet Rate Limiting")
            .WithDescription("SimpleDiscordNet includes automatic rate limit handling to prevent 429 errors.")
            .WithColor(DiscordColor.Green)
            .AddField("Bucket Updates", "Tracks remaining requests per endpoint bucket", inline: false)
            .AddField("Pre-emptive Waits", "Automatically delays requests to avoid hitting limits", inline: false)
            .AddField("429 Handling", "Retries automatically when rate limited", inline: false)
            .AddField("Request Queuing", "Queues requests during global rate limits", inline: false)
            .AddField("Monitoring", "Use `/ratelimit status` to see recent activity", inline: false);

        await ctx.RespondAsync("Rate limiting information:", embed);
    }
}
