using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

/// <summary>
/// The fixed list of speeds the user can choose from, from walking pace to 1,000 times the speed of light.
/// </summary>
/// <remarks>
/// <para>
/// Why: most people can't picture "99% of light speed", but they can picture walking, driving, or a jet.
/// Mixing real-world speeds with light-speed ones lets the user see how the same trip changes as speed
/// climbs, and pick contrasting trips (e.g. the Moon at walking pace vs. at 99% of light speed).
/// </para>
/// <para>
/// Where it's used: the API sends this list to the app through <c>GET /api/speed-presets</c>
/// (see <c>TripCatalogService</c>), and the app shows it in the speed carousel in this same order.
/// The poster test console app (Kg.Velocity.PosterTests) also looks up speeds here by name.
/// It lives in the Engine so every project shares one list.
/// </para>
/// </remarks>
public static class SpeedPresets
{
    /// <summary>All speed presets, slowest first. The order here is the order shown in the app.</summary>
    public static readonly List<SpeedPreset> All = new()
    {
        // Slow Pokes
        new("Walking Speed", 3, "— Slow Pokes —"),
        new("Car (Highway) Speed", 65, "— Slow Pokes —"),
        new("Boeing 747 Speed", 570, "— Slow Pokes —"),
        new("SR-71 Blackbird Speed", 2200, "— Slow Pokes —"),

        // Space
        new("ISS Orbit Speed", 17500, "— Space Vehicles —"),
        new("Apollo 10 Speed", 24791, "— Space Vehicles —"),
        new("Voyager 1 Speed", 38000, "— Space Vehicles —"),
        new("Parker Solar Probe Speed", 430000, "— Space Vehicles —"),

        // Relativistic
        new("1% Light Speed", PhysicsConstants.SpeedOfLightMph * 0.01, "— Light Speeds —"),
        new("10% Light Speed", PhysicsConstants.SpeedOfLightMph * 0.10, "— Light Speeds —"),
        new("50% Light Speed", PhysicsConstants.SpeedOfLightMph * 0.50, "— Light Speeds —"),
        new("90% Light Speed", PhysicsConstants.SpeedOfLightMph * 0.90, "— Light Speeds —"),
        new("99% Light Speed", PhysicsConstants.SpeedOfLightMph * 0.99, "— Light Speeds —"),
        new("Light Speed (c)", PhysicsConstants.SpeedOfLightMph, "— Light Speeds —"),

        // FTL
        new("2x Light Speed", PhysicsConstants.SpeedOfLightMph * 2.0, "— Faster than Light —"),
        new("10x Light Speed", PhysicsConstants.SpeedOfLightMph * 10.0, "— Faster than Light —"),
        new("100x Light Speed", PhysicsConstants.SpeedOfLightMph * 100.0, "— Faster than Light —"),
        new("1,000x Light Speed", PhysicsConstants.SpeedOfLightMph * 1000.0, "— Faster than Light —")
    };
}

