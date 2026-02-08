using Microsoft.Extensions.Caching.Memory;

namespace Kg.Velocity.Api.Services;

public class TravelLogEventsCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public TravelLogEventsCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>Caches log entries so the same content appears when the travel log SVG is rendered after evaluation.</summary>
    public void Store(string nonce, List<TravelLogEntry> entries)
    {
        var cacheKey = GetCacheKey(nonce);
        _cache.Set(cacheKey, entries, CacheDuration);
    }

    /// <summary>Retrieves cached entries for travel log rendering, returning null if expired or missing.</summary>
    public List<TravelLogEntry>? TryGet(string nonce)
    {
        var cacheKey = GetCacheKey(nonce);
        _cache.TryGetValue(cacheKey, out List<TravelLogEntry>? entries);
        return entries;
    }

    private static string GetCacheKey(string nonce) => $"travel_log_{nonce}";
}

public class TravelLogEntry
{
    public string Text { get; set; } = string.Empty;
}
