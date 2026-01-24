using Kg.Velocity.Engine;
using Kg.Velocity.Math;

Console.WriteLine("=== Journey Insight Classifier Tests ===\n");

// Constants
const double LightSpeedMph = PhysicsConstants.SpeedOfLightMph;
const double SecondsPerYear = 365.25 * 24 * 3600;

// Distance constants (approximate)
const double MoonMiles = 238_900;
const double MarsMiles = 140_000_000;
const double PlutoMiles = 3_670_000_000; // ~3.67 billion miles
const double ProximaCentauriMiles = 4.24 * PhysicsConstants.LightYearMiles;
const double PolarisMiles = 433 * PhysicsConstants.LightYearMiles;
const double MilkyWayCenterMiles = 26_000 * PhysicsConstants.LightYearMiles;
const double AndromedaMiles = 2_500_000 * PhysicsConstants.LightYearMiles;

// Test scenarios - organized by distance, then speed slow-to-fast
var scenarios = new List<(string Name, double DistanceMiles, double SpeedMph, string SpeedName)>
{
    // ===== MOON (238,900 miles) =====
    ("Moon - walking (3 mph)", MoonMiles, 3, "walking"),
    ("Moon - bicycle (15 mph)", MoonMiles, 15, "bicycle"),
    ("Moon - car (60 mph)", MoonMiles, 60, "car"),
    ("Moon - airplane (500 mph)", MoonMiles, 500, "airplane"),
    ("Moon - SR-71 (2,200 mph)", MoonMiles, 2200, "SR-71"),
    ("Moon - ISS (17,500 mph)", MoonMiles, 17_500, "ISS orbital"),
    ("Moon - Parker Solar Probe (430,000 mph)", MoonMiles, 430_000, "Parker Solar Probe"),
    ("Moon - 1% c", MoonMiles, LightSpeedMph * 0.01, "1% c"),
    ("Moon - 10% c", MoonMiles, LightSpeedMph * 0.10, "10% c"),
    ("Moon - 50% c", MoonMiles, LightSpeedMph * 0.50, "50% c"),
    ("Moon - 99% c", MoonMiles, LightSpeedMph * 0.99, "99% c"),
    ("Moon - 10x c", MoonMiles, LightSpeedMph * 10, "10x c"),
    ("Moon - 100x c", MoonMiles, LightSpeedMph * 100, "100x c"),

    // ===== MARS (140 million miles) =====
    ("Mars - walking (3 mph)", MarsMiles, 3, "walking"),
    ("Mars - bicycle (15 mph)", MarsMiles, 15, "bicycle"),
    ("Mars - car (60 mph)", MarsMiles, 60, "car"),
    ("Mars - airplane (500 mph)", MarsMiles, 500, "airplane"),
    ("Mars - ISS (17,500 mph)", MarsMiles, 17_500, "ISS orbital"),
    ("Mars - Parker Solar Probe (430,000 mph)", MarsMiles, 430_000, "Parker Solar Probe"),
    ("Mars - 1% c", MarsMiles, LightSpeedMph * 0.01, "1% c"),
    ("Mars - 10% c", MarsMiles, LightSpeedMph * 0.10, "10% c"),
    ("Mars - 50% c", MarsMiles, LightSpeedMph * 0.50, "50% c"),
    ("Mars - 99% c", MarsMiles, LightSpeedMph * 0.99, "99% c"),
    ("Mars - 10x c", MarsMiles, LightSpeedMph * 10, "10x c"),
    ("Mars - 100x c", MarsMiles, LightSpeedMph * 100, "100x c"),

    // ===== PLUTO (3.67 billion miles) =====
    ("Pluto - walking (3 mph)", PlutoMiles, 3, "walking"),
    ("Pluto - bicycle (15 mph)", PlutoMiles, 15, "bicycle"),
    ("Pluto - airplane (500 mph)", PlutoMiles, 500, "airplane"),
    ("Pluto - ISS (17,500 mph)", PlutoMiles, 17_500, "ISS orbital"),
    ("Pluto - Parker Solar Probe (430,000 mph)", PlutoMiles, 430_000, "Parker Solar Probe"),
    ("Pluto - 1% c", PlutoMiles, LightSpeedMph * 0.01, "1% c"),
    ("Pluto - 10% c", PlutoMiles, LightSpeedMph * 0.10, "10% c"),
    ("Pluto - 50% c", PlutoMiles, LightSpeedMph * 0.50, "50% c"),
    ("Pluto - 99% c", PlutoMiles, LightSpeedMph * 0.99, "99% c"),
    ("Pluto - 10x c", PlutoMiles, LightSpeedMph * 10, "10x c"),

    // ===== PROXIMA CENTAURI (4.24 light years) =====
    ("Proxima Centauri - walking (3 mph)", ProximaCentauriMiles, 3, "walking"),
    ("Proxima Centauri - bicycle (15 mph)", ProximaCentauriMiles, 15, "bicycle"),
    ("Proxima Centauri - airplane (500 mph)", ProximaCentauriMiles, 500, "airplane"),
    ("Proxima Centauri - ISS (17,500 mph)", ProximaCentauriMiles, 17_500, "ISS orbital"),
    ("Proxima Centauri - 1% c", ProximaCentauriMiles, LightSpeedMph * 0.01, "1% c"),
    ("Proxima Centauri - 10% c", ProximaCentauriMiles, LightSpeedMph * 0.10, "10% c"),
    ("Proxima Centauri - 50% c", ProximaCentauriMiles, LightSpeedMph * 0.50, "50% c"),
    ("Proxima Centauri - 90% c", ProximaCentauriMiles, LightSpeedMph * 0.90, "90% c"),
    ("Proxima Centauri - 99% c", ProximaCentauriMiles, LightSpeedMph * 0.99, "99% c"),
    ("Proxima Centauri - 99.9% c", ProximaCentauriMiles, LightSpeedMph * 0.999, "99.9% c"),
    ("Proxima Centauri - 10x c", ProximaCentauriMiles, LightSpeedMph * 10, "10x c"),
    ("Proxima Centauri - 100x c", ProximaCentauriMiles, LightSpeedMph * 100, "100x c"),

    // ===== POLARIS (433 light years) =====
    ("Polaris - walking (3 mph)", PolarisMiles, 3, "walking"),
    ("Polaris - bicycle (15 mph)", PolarisMiles, 15, "bicycle"),
    ("Polaris - 1% c", PolarisMiles, LightSpeedMph * 0.01, "1% c"),
    ("Polaris - 50% c", PolarisMiles, LightSpeedMph * 0.50, "50% c"),
    ("Polaris - 99% c", PolarisMiles, LightSpeedMph * 0.99, "99% c"),
    ("Polaris - 99.99% c", PolarisMiles, LightSpeedMph * 0.9999, "99.99% c"),
    ("Polaris - 10x c", PolarisMiles, LightSpeedMph * 10, "10x c"),
    ("Polaris - 100x c", PolarisMiles, LightSpeedMph * 100, "100x c"),

    // ===== MILKY WAY CENTER (26,000 light years) =====
    ("Milky Way Center - walking (3 mph)", MilkyWayCenterMiles, 3, "walking"),
    ("Milky Way Center - 1% c", MilkyWayCenterMiles, LightSpeedMph * 0.01, "1% c"),
    ("Milky Way Center - 50% c", MilkyWayCenterMiles, LightSpeedMph * 0.50, "50% c"),
    ("Milky Way Center - 99% c", MilkyWayCenterMiles, LightSpeedMph * 0.99, "99% c"),
    ("Milky Way Center - 99.99% c", MilkyWayCenterMiles, LightSpeedMph * 0.9999, "99.99% c"),
    ("Milky Way Center - 10x c", MilkyWayCenterMiles, LightSpeedMph * 10, "10x c"),
    ("Milky Way Center - 100x c", MilkyWayCenterMiles, LightSpeedMph * 100, "100x c"),
    ("Milky Way Center - 1000x c", MilkyWayCenterMiles, LightSpeedMph * 1000, "1000x c"),

    // ===== ANDROMEDA (2.5 million light years) =====
    ("Andromeda - walking (3 mph)", AndromedaMiles, 3, "walking"),
    ("Andromeda - 1% c", AndromedaMiles, LightSpeedMph * 0.01, "1% c"),
    ("Andromeda - 50% c", AndromedaMiles, LightSpeedMph * 0.50, "50% c"),
    ("Andromeda - 99% c", AndromedaMiles, LightSpeedMph * 0.99, "99% c"),
    ("Andromeda - 99.99% c", AndromedaMiles, LightSpeedMph * 0.9999, "99.99% c"),
    ("Andromeda - 99.9999% c", AndromedaMiles, LightSpeedMph * 0.999999, "99.9999% c"),
    ("Andromeda - 10x c", AndromedaMiles, LightSpeedMph * 10, "10x c"),
    ("Andromeda - 100x c", AndromedaMiles, LightSpeedMph * 100, "100x c"),
    ("Andromeda - 1000x c", AndromedaMiles, LightSpeedMph * 1000, "1000x c"),
};

foreach (var (name, distance, speed, speedName) in scenarios)
{
    // Calculate trip parameters
    double earthTimeSeconds = distance / speed * 3600; // distance/speed gives hours, convert to seconds

    // Calculate Lorentz factor (γ)
    double v = speed;
    double c = LightSpeedMph;
    double lorentzFactor;

    if (speed >= c)
    {
        // FTL: no time dilation in classical sense
        lorentzFactor = 1.0;
    }
    else
    {
        double vOverC = v / c;
        lorentzFactor = 1.0 / System.Math.Sqrt(1 - vOverC * vOverC);
    }

    // Ship time = Earth time / γ
    double shipTimeSeconds = earthTimeSeconds / lorentzFactor;

    // Get classification
    string insight = JourneyInsightClassifier.Classify(
        earthTimeSeconds,
        shipTimeSeconds,
        speed,
        lorentzFactor);

    // Get all scores for visibility
    var scores = JourneyInsightClassifier.GetScores(
        earthTimeSeconds,
        shipTimeSeconds,
        speed,
        lorentzFactor);

    // Format times
    double earthYears = earthTimeSeconds / SecondsPerYear;
    double shipYears = shipTimeSeconds / SecondsPerYear;

    string earthTimeStr = FormatTime(earthYears);
    string shipTimeStr = FormatTime(shipYears);

    // Output
    Console.WriteLine($">>> {name}");
    Console.WriteLine($"    Earth time: {earthTimeStr}");
    Console.WriteLine($"    Ship time:  {shipTimeStr}");
    Console.WriteLine($"    γ (Lorentz): {lorentzFactor:F2}");
    Console.WriteLine($"    Scores: {string.Join(", ", scores.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}={kv.Value:F0}"))}");
    Console.WriteLine($"    --> INSIGHT: {insight}");
    Console.WriteLine();
}

static string FormatTime(double years)
{
    if (years < 0.000001) return $"{years * SecondsPerYear:F2} seconds";
    if (years < 0.0001) return $"{years * SecondsPerYear / 60:F2} minutes";
    if (years < 0.01) return $"{years * SecondsPerYear / 3600:F2} hours";
    if (years < 1) return $"{years * 365.25:F1} days";
    if (years < 1000) return $"{years:F1} years";
    if (years < 1_000_000) return $"{years / 1000:F1} thousand years";
    if (years < 1_000_000_000) return $"{years / 1_000_000:F1} million years";
    if (years < 1_000_000_000_000) return $"{years / 1_000_000_000:F1} billion years";
    return $"{years / 1_000_000_000_000:F1} trillion years";
}
