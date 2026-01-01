using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;

namespace Kg.Velocity.Api.Services;

public class TripPosterService
{
    public static byte[] GeneratePoster(HttpRequest httpRequest)
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

        var svg = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg xmlns=""http://www.w3.org/2000/svg"" width=""800"" height=""1200"" viewBox=""0 0 800 1200"">
  <defs>
    <linearGradient id=""spine"" x1=""0"" x2=""0"" y1=""0"" y2=""1"">
      <stop offset=""0%"" stop-color=""{accent2}"" stop-opacity=""0.9""/>
      <stop offset=""100%"" stop-color=""{accent}"" stop-opacity=""0.95""/>
    </linearGradient>
    <filter id=""shadow"" x=""-20%"" y=""-20%"" width=""140%"" height=""140%"">
      <feDropShadow dx=""0"" dy=""8"" stdDeviation=""10"" flood-color=""#000"" flood-opacity=""0.35""/>
    </filter>
    <style>
      .title {{ font: 800 54px -apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,sans-serif; fill: #fbbf24; }}
      .subtitle {{ font: 500 26px -apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,sans-serif; fill: #cbd5e1; }}
      .meta {{ font: 600 18px ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,""Liberation Mono"",""Courier New"",monospace; fill: #94a3b8; }}
    </style>
  </defs>

  <rect x=""0"" y=""0"" width=""800"" height=""1200"" fill=""#0b1b3b""/>

  <text x=""70"" y=""130"" class=""title"">Absurd Travel Poster</text>
  <text x=""70"" y=""175"" class=""subtitle"">{Esc(destination)} · {Esc(speed)}</text>
  <text x=""70"" y=""215"" class=""meta"">Generated: {generatedAt} · nonce {Esc(nonce)}</text>

  <g filter=""url(#shadow)"">
    <rect x=""370"" y=""440"" width=""60"" height=""700"" rx=""30"" fill=""url(#spine)""/>
    <circle cx=""400"" cy=""425"" r=""46"" fill=""{accent2}""/>
    <circle cx=""400"" cy=""425"" r=""30"" fill=""#0b1b3b"" opacity=""0.85""/>
    <circle cx=""400"" cy=""1155"" r=""46"" fill=""{accent2}""/>
    <circle cx=""400"" cy=""1155"" r=""30"" fill=""#0b1b3b"" opacity=""0.85""/>
  </g>

  <text x=""70"" y=""1150"" class=""meta"">Server-generated mock SVG (bytes) · kg-velocity</text>
</svg>";

        var bytes = Encoding.UTF8.GetBytes(svg);
        return bytes;
    }
}
