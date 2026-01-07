using Scriban;
using Scriban.Runtime;
using System.Globalization;
using System.Text;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// 
/// </summary>
public class TripPosterService(PosterEventsCache eventsCache, DestinationIconService iconService)
{
    private const string TemplateResourceName = "Kg.Velocity.Api.Templates.trip-poster.svg.sbn";
    private readonly PosterEventsCache _eventsCache = eventsCache;
    private readonly DestinationIconService _iconService = iconService;

    private static string LoadTemplateText()
    {
        var asm = typeof(TripPosterService).Assembly;
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

    // Cache the compiled template (parse/compile once). Output is still rendered per request.
    private static readonly Lazy<Template> ParsedTemplate =
        new(LoadAndParseTemplate, isThreadSafe: true);

    /// <summary>
    /// Generates fallback journey events when AI events aren't available.
    /// </summary>
    private static List<JourneyEvent> GenerateFallbackEvents(string destination, int seed)
    {
        var eventCount = (seed % 4) + 3; // 3-6 events

        var allEvents = new List<JourneyEvent>
        {
            new() { Text = "Gravity Well Bypass", Description = "Slingshot around Jupiter's moon Io" },
            new() { Text = "Kuiper Belt Navigation", Description = "Dodging frozen remnants" },
            new() { Text = "Oort Cloud Crossing", Description = "The sun's final whisper fades" },
            new() { Text = "Dark Matter Current", Description = "Riding invisible rivers between stars" },
            new() { Text = "Neutron Star Encounter", Description = "Time hiccups near the magnetic beast" },
            new() { Text = "Nebula Refueling Stop", Description = "Hydrogen harvest in stellar nursery" },
            new() { Text = "Wormhole Attempt", Description = "Shortcut through spacetime fabric" },
            new() { Text = "Quantum Tunnel Transit", Description = "Probability waves align perfectly" }
        };

        return allEvents.Take(eventCount).ToList();
    }

    public byte[] GeneratePoster(HttpRequest httpRequest)
    {
        try
        {
            var destination = httpRequest.Query["destination"].ToString();
            var speed = httpRequest.Query["speed"].ToString();
            var nonce = httpRequest.Query["nonce"].ToString();
            var earthTime = httpRequest.Query["earthTime"].ToString();
            var shipTime = httpRequest.Query["shipTime"].ToString();

            var generatedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

            // Simple, deterministic-ish color variation based on nonce.
            _ = int.TryParse(new string(nonce.Where(char.IsDigit).TakeLast(6).ToArray()), out int seed);
            var accentHue = (seed % 40) + 15;

            static string Esc(string? s) =>
                string.IsNullOrEmpty(s) ? "" :
                s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                 .Replace("\"", "&quot;").Replace("'", "&apos;");

            // Try to get AI-generated events from cache, fall back to mock events
            var events = _eventsCache.TryGet(nonce) ?? GenerateFallbackEvents(destination, seed);

            // Render via Scriban
            var template = ParsedTemplate.Value;

            var globals = new ScriptObject
            {
                { "destination_esc", Esc(destination) },
                { "speed_esc", Esc(speed) },
                { "nonce_esc", Esc(nonce) },
                { "generated_at", generatedAt },
                { "earth_time", Esc(earthTime) },
                { "ship_time", Esc(shipTime) },
                // Each unique nonce gets a different but reproducible color pair. Same nonce = same colors every time.
                { "accent_hue", accentHue }
            };
 
            // Load destination icon SVG content
            var (iconContent, iconViewBox) = _iconService.GetIconSvgContent(destination);
            globals.Add("destination_icon", iconContent);
            globals.Add("destination_icon_viewbox", iconViewBox);

            // Convert events to ScriptArray for Scriban iteration
            var scriptEvents = new ScriptArray();
            foreach (var evt in events)
            {
                var scriptEvent = new ScriptObject
                {
                    { "text", Esc(evt.Text) }
                };
                if (!string.IsNullOrEmpty(evt.Description))
                {
                    scriptEvent.Add("description", Esc(evt.Description));
                }
                scriptEvents.Add(scriptEvent);
            }
            globals.Add("events", scriptEvents);

            var context = new TemplateContext();
            context.PushGlobal(globals);
            var svg = template.Render(context);

            return Encoding.UTF8.GetBytes(svg);
        }
        catch (Exception ex)
        {
            string s = ex.ToString();
            throw;
        }
    }
}
