using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

public static class SpeedPresets
{
    public static readonly List<SpeedPreset> All = new()
    {
        // Slow Pokes
        new SpeedPreset { Name = "Walking Speed", SpeedMph = 3, Group = "— Slow Pokes —" },
        new SpeedPreset { Name = "Car (Highway) Speed", SpeedMph = 65, Group = "— Slow Pokes —" },
        new SpeedPreset { Name = "Boeing 747 Speed", SpeedMph = 570, Group = "— Slow Pokes —" },
        new SpeedPreset { Name = "SR-71 Blackbird Speed", SpeedMph = 2200, Group = "— Slow Pokes —" },

        // Space
        new SpeedPreset { Name = "ISS Orbit Speed", SpeedMph = 17500, Group = "— Space Vehicles —" },
        new SpeedPreset { Name = "Apollo 10 Speed", SpeedMph = 24791, Group = "— Space Vehicles —" },
        new SpeedPreset { Name = "Voyager 1 Speed", SpeedMph = 38000, Group = "— Space Vehicles —" },
        new SpeedPreset { Name = "Parker Solar Probe Speed", SpeedMph = 430000, Group = "— Space Vehicles —" },

        // Relativistic
        new SpeedPreset { Name = "1% Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 0.01, Group = "— Light Speeds —" },
        new SpeedPreset { Name = "10% Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 0.10, Group = "— Light Speeds —" },
        new SpeedPreset { Name = "50% Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 0.50, Group = "— Light Speeds —" },
        new SpeedPreset { Name = "90% Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 0.90, Group = "— Light Speeds —" },
        new SpeedPreset { Name = "99% Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 0.99, Group = "— Light Speeds —" },
        new SpeedPreset { Name = "Light Speed (c)", SpeedMph = PhysicsConstants.SpeedOfLightMph, Group = "— Light Speeds —" },

        // FTL
        new SpeedPreset { Name = "2x Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 2.0, Group = "— Faster than Light —" },
        new SpeedPreset { Name = "10x Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 10.0, Group = "— Faster than Light —" },
        new SpeedPreset { Name = "100x Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 100.0, Group = "— Faster than Light —" },
        new SpeedPreset { Name = "1,000x Light Speed", SpeedMph = PhysicsConstants.SpeedOfLightMph * 1000.0, Group = "— Faster than Light —" }
    };
}

