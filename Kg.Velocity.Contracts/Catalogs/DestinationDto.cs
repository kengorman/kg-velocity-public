namespace Kg.Velocity.Contracts.Catalogs;

public sealed record DestinationDto(
    string Name,
    double DistanceMiles,
    string Category,
    string DisplayName,
    string Tagline,
    string? NasaSearchTerm = null
);













