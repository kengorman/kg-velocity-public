using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Api.Services;

public class TripCatalogService
{
    /// <summary>Provides speed options for the UI, from walking pace to faster-than-light.</summary>
    public IReadOnlyList<SpeedPresetDto> GetSpeedPresets()
        => Kg.Velocity.Engine.SpeedPresets.All
            .Select(p => new SpeedPresetDto(p.Name, p.SpeedMph, p.Group))
            .ToList();

    /// <summary>Provides destination options spanning the Moon to Andromeda, each with real astronomical distances.</summary>
    public IReadOnlyList<DestinationDto> GetDestinations()
    {
        var destinations = new List<Destination>
        {
            // Solar System (Moon → Pluto)
            new("The Moon", 238_855, "Solar System", "Our closest neighbor"),
            new("Mercury", 56_000_000, "Solar System", "Swift messenger of the gods"),
            new("The Sun", 93_000_000, "Solar System", "Heart of the solar system"),
            new("Mars", 140_000_000, "Solar System", "The Red Planet"),
            new("Jupiter", 484_000_000, "Solar System", "King of the planets"),
            new("Saturn", 886_000_000, "Solar System", "The ringed giant"),
            new("Pluto", 3_700_000_000, "Solar System", "The frozen frontier"),

            // Milky Way (Proxima Centauri → Milky Way center)
            new("Proxima Centauri", 4.24 * PhysicsConstants.LightYearMiles, "Milky Way", "Nearest star beyond the Sun"),
            new("Polaris (North Star)", 433 * PhysicsConstants.LightYearMiles, "Milky Way", "Guiding light of navigators"),
            new("Betelgeuse", 700 * PhysicsConstants.LightYearMiles, "Milky Way", "A dying red supergiant"),
            new("Horsehead Nebula", 1_500 * PhysicsConstants.LightYearMiles, "Milky Way", "Dark silhouette in Orion"),
            new("Crab Nebula", 6_500 * PhysicsConstants.LightYearMiles, "Milky Way", "Remnant of a supernova"),
            new("Pillars of Creation", 6_500 * PhysicsConstants.LightYearMiles, "Milky Way", "Stellar nursery in the Eagle"),
            new("Milky Way (center)", 26_000 * PhysicsConstants.LightYearMiles, "Milky Way", "Home of a supermassive black hole"),

            // Extragalactic
            new("Andromeda Galaxy", 2_537_000 * PhysicsConstants.LightYearMiles, "Extragalactic", "Our nearest galactic neighbor"),
            new("Triangulum Galaxy", 2_730_000 * PhysicsConstants.LightYearMiles, "Extragalactic", "The third wheel of the Local Group"),
            new("Whirlpool Galaxy", 23_000_000 * PhysicsConstants.LightYearMiles, "Extragalactic", "A face-on spiral masterpiece"),
            new("Sombrero Galaxy", 29_000_000 * PhysicsConstants.LightYearMiles, "Extragalactic", "The hat at the edge of imagination")
        };

        return destinations
            .Select(d => new DestinationDto(
                d.Name,
                d.DistanceMiles,
                d.Category,
                d.DisplayName,
                d.Tagline))
            .ToList();
    }
}
