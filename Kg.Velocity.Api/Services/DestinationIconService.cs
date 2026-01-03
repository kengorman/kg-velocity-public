using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Kg.Velocity.Api.Services;

public partial class DestinationIconService
{
    private readonly IHostEnvironment _env;
    private readonly ConcurrentDictionary<string, (string innerContent, string viewBox)> _iconCache = new(StringComparer.OrdinalIgnoreCase);

    [GeneratedRegex(@"<svg[^>]*viewBox\s*=\s*""([^""]+)""[^>]*>(.*)</svg>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex SvgWrapperRegex();

    private static readonly Dictionary<string, string> DestinationToIconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["The Moon"] = "moon.svg",
        ["Mercury"] = "mercury.svg",
        ["The Sun"] = "sun.svg",
        ["Mars"] = "mars.svg",
        ["Saturn"] = "saturn.svg",
        ["Uranus"] = "uranus.svg",
        ["Pluto"] = "pluto.svg",
        ["Proxima Centauri"] = "proxima-centauri.svg",
        ["Polaris (North Star)"] = "polaris.svg",
        ["Betelgeuse"] = "betelgeuse.svg",
        ["Horsehead Nebula"] = "horsehead-nebula.svg",
        ["Crab Nebula"] = "crab-nebula.svg",
        ["Pillars of Creation"] = "pillars-of-creation.svg",
        ["Milky Way (center)"] = "milky-way.svg",
        ["Andromeda Galaxy"] = "andromeda.svg"
    };

    public DestinationIconService(IHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Gets the SVG inner content and viewBox for a destination icon.
    /// Returns the inner SVG content (without the outer svg tag) for embedding.
    /// </summary>
    public (string innerContent, string viewBox) GetIconSvgContent(string destination)
    {
        return _iconCache.GetOrAdd(destination, LoadIconContent);
    }

    private (string innerContent, string viewBox) LoadIconContent(string destination)
    {
        var iconFile = DestinationToIconMap.GetValueOrDefault(destination, "default.svg");
        var iconPath = Path.Combine(_env.ContentRootPath, "Templates", "Icons", iconFile);

        if (!File.Exists(iconPath))
        {
            // Fall back to default if specific icon not found
            iconPath = Path.Combine(_env.ContentRootPath, "Templates", "Icons", "default.svg");
        }

        if (!File.Exists(iconPath))
        {
            // Ultimate fallback - return empty
            return (string.Empty, "0 0 60 60");
        }

        var svgContent = File.ReadAllText(iconPath);

        // Extract viewBox and inner content using regex
        var match = SvgWrapperRegex().Match(svgContent);
        if (match.Success)
        {
            var viewBox = match.Groups[1].Value;
            var innerContent = match.Groups[2].Value.Trim();
            return (innerContent, viewBox);
        }

        // Fallback if regex doesn't match - return as-is with default viewBox
        return (svgContent, "0 0 60 60");
    }
}
