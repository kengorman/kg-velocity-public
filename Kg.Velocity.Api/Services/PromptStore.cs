using System.Collections.Concurrent;

namespace Kg.Velocity.Api.Services;

public class PromptStore(IHostEnvironment env)
{
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetPrompt(string relativePath)
    {
        return _cache.GetOrAdd(relativePath, LoadPrompt);
    }

    private string LoadPrompt(string relativePath)
    {
        var fullPath = Path.Combine(env.ContentRootPath, relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Prompt file not found: {relativePath}", fullPath);

        return File.ReadAllText(fullPath);
    }
}
















