namespace Kg.Velocity.Contracts.Catalogs;

/// <summary>
/// A speed preset as the API sends it to the app, via <c>GET /api/speed-presets</c>.
/// </summary>
/// <remarks>
/// A copy of the Engine's <c>SpeedPreset</c> model; see it for what each field means.
/// </remarks>
public sealed record SpeedPresetDto(
    string Name,
    double SpeedMph,
    string Group
);
