using Scriban;
using Scriban.Runtime;
using System.Text;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Draws the travel log image (SVG) for /api/travel-log.svg, using the Templates/travel-log.svg.sbn template
/// and the cached AI entries. Uses fixed entries if the cache has expired.
/// </summary>
public class TravelLogService(TravelLogEventsCache eventsCache)
{
    private const string TemplateResourceName = "Kg.Velocity.Api.Templates.travel-log.svg.sbn";
    private readonly TravelLogEventsCache _eventsCache = eventsCache;

    private static string LoadTemplateText()
    {
        var asm = typeof(TravelLogService).Assembly;
        using var stream = asm.GetManifestResourceStream(TemplateResourceName);
        if (stream is null)
        {
            var known = string.Join(", ", asm.GetManifestResourceNames().OrderBy(n => n));
            throw new InvalidOperationException(
                $"Embedded resource not found: {TemplateResourceName}. Known resources: {known}");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        return reader.ReadToEnd();
    }

    private static Template LoadAndParseTemplate()
    {
        var templateText = LoadTemplateText();
        var template = Template.Parse(templateText, TemplateResourceName);
        if (template.HasErrors)
        {
            var errors = string.Join("; ", template.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException($"SVG template parse error(s): {errors}");
        }

        return template;
    }

    private static readonly Lazy<Template> ParsedTemplate =
        new(LoadAndParseTemplate, isThreadSafe: true);

    private static List<TravelLogEntry> GetFallbackEntries()
    {
        return new List<TravelLogEntry>
        {
            new() { Text = "Ship systems nominal. The void stretches endlessly ahead." },
            new() { Text = "Time passes differently here. Earth feels like a distant memory." },
            new() { Text = "Stars shift colors as we approach relativistic speeds." },
            new() { Text = "The silence of space is both peaceful and unnerving." },
            new() { Text = "Destination growing closer. A new world awaits." }
        };
    }

    /// <summary>Renders the travel log SVG using cached AI-generated entries and Scriban templating.</summary>
    public byte[] GenerateTravelLog(HttpRequest httpRequest)
    {
        var destination = httpRequest.Query["destination"].ToString();
        var speedName = httpRequest.Query["speed"].ToString();
        var departureDate = httpRequest.Query["departure"].ToString();
        var arrivalDate = httpRequest.Query["arrival"].ToString();
        var nonce = httpRequest.Query["nonce"].ToString();

        static string Esc(string? s) =>
            string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
             .Replace("\"", "&quot;").Replace("'", "&apos;");

        // Get entries from cache, fall back if not found
        var entries = _eventsCache.TryGet(nonce) ?? GetFallbackEntries();

        var template = ParsedTemplate.Value;

        var globals = new ScriptObject
        {
            { "destination", Esc(destination) },
            { "speed_name", Esc(speedName) },
            { "departure_date", Esc(departureDate) },
            { "arrival_date", Esc(arrivalDate) }
        };

        var scriptEvents = new ScriptArray();
        foreach (var entry in entries)
        {
            scriptEvents.Add(new ScriptObject { { "text", Esc(entry.Text) } });
        }
        globals.Add("events", scriptEvents);

        var context = new TemplateContext();
        context.PushGlobal(globals);
        var svg = template.Render(context);

        return Encoding.UTF8.GetBytes(svg);
    }
}
