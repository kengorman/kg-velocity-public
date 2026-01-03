using Microsoft.AspNetCore.Http;
using Scriban;
using Scriban.Runtime;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Kg.Velocity.Api.Services;

public class TripPosterService
{
    private const string TemplateResourceName = "Kg.Velocity.Api.Templates.trip-poster.svg.sbn";

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

    public static byte[] GeneratePoster(HttpRequest httpRequest)
    {
        try
        {
            // This is an intentionally bogus poster generator to prove wiring:
            // server generates bytes -> client displays/downloads.
            var destination = httpRequest.Query["destination"].ToString();
            var speed = httpRequest.Query["speed"].ToString();
            var nonce = httpRequest.Query["nonce"].ToString();

            var generatedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

            // Simple, deterministic-ish color variation based on nonce.
            var seed = 0;
            _ = int.TryParse(new string(nonce.Where(char.IsDigit).TakeLast(6).ToArray()), out seed);
            var accentHue = (seed % 40) + 15;
            var accent = $"hsl({accentHue}, 90%, 55%)";
            var accent2 = $"hsl({(accentHue + 180) % 360}, 80%, 60%)";

            static string Esc(string? s) =>
                string.IsNullOrEmpty(s) ? "" :
                s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                 .Replace("\"", "&quot;").Replace("'", "&apos;");

            // Geometry: bar starts at the top of a "rising Earth" semicircle at the bottom edge.
            const int canvasHeight = 1200;
            const int baseEarthRadius = (int)(canvasHeight * 0.05); // 5% of 1200 = 60
            const int earthDiameterScale = 3; // baseline sizing
            var earthRadius = baseEarthRadius * earthDiameterScale; // 180 (baseline)
            const int barHeight = 850;
            const int capOffset = 15;
            const int earthCx = 400;

            // Keep the top of the visible Earth dome fixed (do not raise/lower it),
            // but widen the visible base at the bottom edge by increasing the radius
            // and moving the center down accordingly.
            var baselineCapHeight = earthRadius / 2;           // 25% of baseline diameter
            var earthTopY = canvasHeight - baselineCapHeight;  // fixed top dome Y

            const double earthBaseWidenScale = 1.25;           // widen base without moving the top dome
            earthRadius = (int)System.Math.Round(earthRadius * earthBaseWidenScale);
            var earthCy = earthTopY + earthRadius;

            // Bar starts at the top of the visible Earth dome.
            var barBottomY = earthTopY;
            var barY = barBottomY - barHeight;
            var capTopCy = barY - capOffset;

            // Render via Scriban (template is embedded as a resource and parsed once).
            var template = ParsedTemplate.Value;

            var globals = new ScriptObject();
        // Pre-escape user-controlled text in C# (avoid custom function invocation in Scriban).
        globals.Add("destination_esc", Esc(destination));
        globals.Add("speed_esc", Esc(speed));
        globals.Add("nonce_esc", Esc(nonce));
            globals.Add("generated_at", generatedAt);
            globals.Add("accent", accent);
            globals.Add("accent2", accent2);
            globals.Add("bar_y", barY);
            globals.Add("bar_height", barHeight);
            globals.Add("cap_top_cy", capTopCy);
            globals.Add("earth_cx", earthCx);
            globals.Add("earth_cy", earthCy);
            globals.Add("earth_r", earthRadius);

            var context = new TemplateContext();
            context.PushGlobal(globals);
            var svg = template.Render(context);

            var bytes = Encoding.UTF8.GetBytes(svg);
            return bytes;
        }
        catch(Exception ex)
        {
            string s = ex.ToString();
            throw;
        }
    }
}
