using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

public static class SpeedPresets
{
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

