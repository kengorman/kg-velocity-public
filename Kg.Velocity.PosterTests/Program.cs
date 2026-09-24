using System.ClientModel;
using System.Text;
using System.Text.Json;
using Kg.Velocity.Engine;
using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

// ── Config ──────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .AddUserSecrets("kg-velocity-api")
    .Build();

// Same settings as the API (see Kg.Velocity.Api/Services/ModelClientFactory.cs)
var endpoint = config["OpenAI:Endpoint"];
var model = config["OpenAI:Model"] is { Length: > 0 } m ? m : "gpt-5.2";
var maxTemperature = float.TryParse(config["OpenAI:MaxTemperature"], System.Globalization.NumberStyles.Float,
    System.Globalization.CultureInfo.InvariantCulture, out var max) ? max : float.MaxValue;

var chatClient = string.IsNullOrWhiteSpace(endpoint)
    ? new OpenAIClient(new ApiKeyCredential(config["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException("OpenAI:ApiKey not configured. Run: dotnet user-secrets set OpenAI:ApiKey <key>")))
        .GetChatClient(model)
    : new OpenAIClient(new ApiKeyCredential(config["OpenAI:ApiKey"] ?? "none"),
        new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
        .GetChatClient(model);

// ── Prompt template (read directly from file) ───────────────────────
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var promptTemplatePath = Path.Combine(solutionRoot, "Kg.Velocity.Api", "Prompts", "poster-events.md");
var promptTemplate = File.ReadAllText(promptTemplatePath);

// ── Destinations & Speeds (from the Engine, not hardcoded) ──────────
var destinations = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase)
{
    ["The Moon"]          = new("The Moon", 238_855, "Solar System", "Our closest neighbor"),
    ["Mars"]              = new("Mars", 140_000_000, "Solar System", "The Red Planet"),
    ["Pluto"]             = new("Pluto", 3_700_000_000, "Solar System", "The frozen frontier"),
    ["Proxima Centauri"]  = new("Proxima Centauri", 4.24 * PhysicsConstants.LightYearMiles, "Milky Way", "Nearest star beyond the Sun"),
    ["Crab Nebula"]       = new("Crab Nebula", 6_500 * PhysicsConstants.LightYearMiles, "Milky Way", "Remnant of a supernova"),
    ["Andromeda Galaxy"]  = new("Andromeda Galaxy", 2_537_000 * PhysicsConstants.LightYearMiles, "Extragalactic", "Our nearest galactic neighbor"),
};

var speeds = SpeedPresets.All.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);

// ── Scenarios: pick destination + speed by name ─────────────────────
var scenarios = new (string Destination, string Speed)[]
{
    ("Crab Nebula",      "2x Light Speed"),
    ("Crab Nebula",      "100x Light Speed"),
    ("Mars",             "Boeing 747 Speed"),
    ("Pluto",            "Car (Highway) Speed"),
    ("Andromeda Galaxy", "1,000x Light Speed"),
};

// ── Run ─────────────────────────────────────────────────────────────
foreach (var (destName, speedName) in scenarios)
{
    var dest = destinations[destName];
    var speed = speeds[speedName];

    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  >>> {dest.Name} @ {speed.Name}");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");

    // Physics
    double earthTimeSeconds = dest.DistanceMiles / speed.SpeedMph * 3600;
    double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(speed.SpeedMph);
    double shipTimeSeconds = earthTimeSeconds / lorentzFactor;

    string earthTimeFormatted = FlightComputer.FormatDuration(earthTimeSeconds);
    string shipTimeFormatted = FlightComputer.FormatDuration(shipTimeSeconds);
    string timeDifference = FlightComputer.CalculateTimeDifference(earthTimeSeconds, shipTimeSeconds);

    double percentOfLight = RelativisticPhysics.CalculatePercentageOfLightSpeed(speed.SpeedMph);
    double distanceLY = RelativisticPhysics.MilesToLightYears(dest.DistanceMiles);

    string insight = JourneyInsightClassifier.Classify(earthTimeSeconds, shipTimeSeconds, speed.SpeedMph, lorentzFactor);

    Console.WriteLine($"  Earth time: {earthTimeFormatted}");
    Console.WriteLine($"  Ship time:  {shipTimeFormatted}");
    Console.WriteLine($"  Insight:    {insight}");
    Console.WriteLine();

    // Build prompt
    var prompt = BuildPrompt(promptTemplate, dest.Name, speed.Name, percentOfLight,
        dest.DistanceMiles, distanceLY, earthTimeFormatted, shipTimeFormatted, timeDifference, insight);

    // Call OpenAI
    try
    {
        var messages = new List<ChatMessage> { new UserChatMessage(prompt) };
        var options = new ChatCompletionOptions { Temperature = System.Math.Min(0.9f, maxTemperature) };

        var completion = await chatClient.CompleteChatAsync(messages, options);
        var responseText = completion.Value.Content[0].Text;

        var events = ParseEvents(responseText);
        foreach (var evt in events)
        {
            Console.WriteLine($"  • {evt.Text}");
            if (!string.IsNullOrEmpty(evt.Description))
                Console.WriteLine($"    {evt.Description}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine();
}

// ── Helpers ──────────────────────────────────────────────────────────

static string BuildPrompt(string template, string destination, string speedName,
    double percentOfLight, double distanceMiles, double distanceLY,
    string earthTime, string shipTime, string timeDiff, string insight)
{
    var sb = new StringBuilder(template);
    sb.Replace("{{Destination}}", destination);
    sb.Replace("{{DepartedTime}}", DateTime.Now.ToString("MMMM d, yyyy h:mm tt"));
    sb.Replace("{{SpeedName}}", speedName);
    sb.Replace("{{PercentageOfLightSpeed}}", percentOfLight.ToString("N6"));
    sb.Replace("{{DistanceMiles}}", distanceMiles.ToString("N0"));
    sb.Replace("{{DistanceLightYears}}", distanceLY.ToString("N4"));
    sb.Replace("{{EarthTimeFormatted}}", earthTime);
    sb.Replace("{{ShipTimeFormatted}}", shipTime);
    sb.Replace("{{TimeDifference}}", timeDiff);
    sb.Replace("{{JourneyInsight}}", insight);
    return sb.ToString();
}

static List<(string Text, string? Description)> ParseEvents(string responseText)
{
    var json = responseText.Trim();
    if (json.StartsWith("```"))
    {
        var startIndex = json.IndexOf('{');
        var endIndex = json.LastIndexOf('}');
        if (startIndex >= 0 && endIndex > startIndex)
            json = json.Substring(startIndex, endIndex - startIndex + 1);
    }

    var doc = JsonDocument.Parse(json);
    var events = new List<(string Text, string? Description)>();

    foreach (var evt in doc.RootElement.GetProperty("events").EnumerateArray())
    {
        var text = evt.GetProperty("text").GetString() ?? "";
        var desc = evt.TryGetProperty("description", out var d) ? d.GetString() : null;
        events.Add((text, desc));
    }

    return events;
}
