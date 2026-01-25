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

    public void Store(string nonce, List<TravelLogEntry> entries)
    {
        var cacheKey = GetCacheKey(nonce);
        _cache.Set(cacheKey, entries, CacheDuration);
    }

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
