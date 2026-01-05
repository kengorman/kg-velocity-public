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
        // Keep the same destination catalog the client previously had.
        var destinations = new List<Destination>
        {
            // Solar System
            new() { Name = "The Moon", DistanceMiles = 238_855, Category = "Space" },
            new() { Name = "Mercury", DistanceMiles = 56_000_000, Category = "Space" },
            new() { Name = "The Sun", DistanceMiles = 93_000_000, Category = "Space" },
            new() { Name = "Mars", DistanceMiles = 140_000_000, Category = "Space" },
            new() { Name = "Jupiter", DistanceMiles = 484_000_000, Category = "Space" },
            new() { Name = "Saturn", DistanceMiles = 886_000_000, Category = "Space" },
            new() { Name = "Uranus", DistanceMiles = 1_800_000_000, Category = "Space" },
            new() { Name = "Pluto", DistanceMiles = 3_700_000_000, Category = "Space" },

            // Deep Space
            new() { Name = "Proxima Centauri", DistanceMiles = 4.24 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Polaris (North Star)", DistanceMiles = 433 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Betelgeuse", DistanceMiles = 700 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Horsehead Nebula", DistanceMiles = 1_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Crab Nebula", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Pillars of Creation", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Milky Way (center)", DistanceMiles = 26_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new() { Name = "Andromeda Galaxy", DistanceMiles = 2_537_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space" }
        };

        return destinations
            .Select(d => new DestinationDto(d.Name, d.DistanceMiles, d.Category, d.DisplayName))
            .ToList();
    }
}













