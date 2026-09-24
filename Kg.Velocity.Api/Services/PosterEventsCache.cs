using Microsoft.Extensions.Caching.Memory;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Holds AI poster events for 10 minutes, keyed by a nonce (a one-time ID for each trip).
/// generate-content stores them; the /api/poster.svg request that follows reads them back.
/// </summary>
public class PosterEventsCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public PosterEventsCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>Caches events so the same content appears when the poster SVG is rendered after evaluation.</summary>
    public void Store(string nonce, List<JourneyEvent> events)
    {
        var cacheKey = GetCacheKey(nonce);
        _cache.Set(cacheKey, events, CacheDuration);
    }

    /// <summary>Retrieves cached events for poster rendering, returning null if expired or missing.</summary>
    public List<JourneyEvent>? TryGet(string nonce)
    {
        var cacheKey = GetCacheKey(nonce);
        _cache.TryGetValue(cacheKey, out List<JourneyEvent>? events);
        return events;
    }

    private static string GetCacheKey(string nonce) => $"poster_events_{nonce}";
}
