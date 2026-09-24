namespace Kg.Velocity.Contracts.Catalogs;

/// <summary>
/// A destination as the API sends it to the app, via <c>GET /api/destinations</c>.
/// </summary>
/// <remarks>
/// A copy of the Engine's <c>Destination</c> model; see it for what each field means.
/// </remarks>
public sealed record DestinationDto(
    string Name,
    double DistanceMiles,
    string Category,
    string DisplayName,
    string Tagline
);
