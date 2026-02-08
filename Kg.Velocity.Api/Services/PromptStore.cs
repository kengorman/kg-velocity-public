using System.Collections.Concurrent;
using System.Reflection;

namespace Kg.Velocity.Api.Services;

public class PromptStore
{
    private static readonly Assembly Assembly = typeof(PromptStore).Assembly;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Loads prompt templates from embedded resources, caching to avoid repeated I/O.</summary>
    public string GetPrompt(string relativePath)
    {
        return _cache.GetOrAdd(relativePath, LoadPrompt);
    }

    private static string LoadPrompt(string relativePath)
    {
        // Convert path like "Prompts/poster-events.md" to "Kg.Velocity.Api.Prompts.poster-events.md"
        var resourceName = "Kg.Velocity.Api." + relativePath.Replace('/', '.').Replace('\\', '.');
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded prompt not found: {relativePath}", resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
