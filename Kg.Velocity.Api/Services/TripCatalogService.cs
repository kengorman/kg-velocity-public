using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Engine.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Api.Services;

public class TripCatalogService
{
    public IReadOnlyList<SpeedPresetDto> GetSpeedPresets()
        => Kg.Velocity.Engine.SpeedPresets.All
            .Select(p => new SpeedPresetDto(p.Name, p.SpeedMph, p.Group))
            .ToList();

    public IReadOnlyList<DestinationDto> GetDestinations()
    {
        var destinations = new List<Destination>
        {
            // Solar System
            new() { Name = "The Moon", DistanceMiles = 238_855, Category = "Space", Tagline = "Our closest neighbor" },
            new() { Name = "Mercury", DistanceMiles = 56_000_000, Category = "Space", Tagline = "Swift messenger of the gods" },
            new() { Name = "The Sun", DistanceMiles = 93_000_000, Category = "Space", Tagline = "Heart of the solar system" },
            new() { Name = "Mars", DistanceMiles = 140_000_000, Category = "Space", Tagline = "The Red Planet" },
            new() { Name = "Jupiter", DistanceMiles = 484_000_000, Category = "Space", Tagline = "King of the planets" },
            new() { Name = "Saturn", DistanceMiles = 886_000_000, Category = "Space", Tagline = "The ringed giant" },
            new() { Name = "Pluto", DistanceMiles = 3_700_000_000, Category = "Space", Tagline = "The frozen frontier" },

            // Deep Space
            new() { Name = "Proxima Centauri", DistanceMiles = 4.24 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Nearest star beyond the Sun" },
            new() { Name = "Polaris (North Star)", DistanceMiles = 433 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Guiding light of navigators" },
            new() { Name = "Betelgeuse", DistanceMiles = 700 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "A dying red supergiant" },
            new() { Name = "Horsehead Nebula", DistanceMiles = 1_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Dark silhouette in Orion" },
            new() { Name = "Crab Nebula", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Remnant of a supernova" },
            new() { Name = "Pillars of Creation", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Stellar nursery in the Eagle" },
            new() { Name = "Milky Way (center)", DistanceMiles = 26_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Home of a supermassive black hole" },
            new() { Name = "Andromeda Galaxy", DistanceMiles = 2_537_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space", Tagline = "Our nearest galactic neighbor" }
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













