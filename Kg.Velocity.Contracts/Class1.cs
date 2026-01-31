namespace Kg.Velocity.Contracts.Trips;

/// Represents normalized perceptual weight signals derived from a journey,
/// expressing how different aspects of the trip should be emphasized
/// based on scale, time passage, and desynchronization effects.
public sealed record JourneyWeights(
    double Emotion,
    double Distance,
    double Awe,
    double TimeGoneBy,
    double Memories,
    double Patience,
    double Loneliness
);

/// <summary>
/// Inputs-only request. The API is responsible for computing all derived values.
/// </summary>
public sealed record TripEvaluateRequest(
    string Destination,
    string SpeedName,
    double SpeedMph,
    double DistanceMiles,
    DateTimeOffset StartTime,
    int? PersonaId = null
);

/// <summary>
/// Authoritative computed results for the trip, suitable for direct UI rendering.
/// </summary>
public sealed record TripComputationResult(
    string Destination,
    string SpeedName,
    double SpeedMph,
    double PercentageOfLightSpeed,
    double DistanceMiles,
    double DistanceLightYears,
    double EarthTimeSeconds,
    double ShipTimeSeconds,
    string EarthTimeFormatted,
    string ShipTimeFormatted,
    string TimeDifferenceFormatted,
    string DepartedEarthTime,
    string ArrivedEarthTime,
    string ArrivedShipTime,
    double? LorentzFactor,
    JourneyWeights Weights
);

public sealed record TripEvaluateResponse(
    TripComputationResult Trip,
    string Summary,
    int PersonaId,
    string PersonaName,
    string? PosterUrl = null
);

public sealed record TripContentResponse(
    string Summary,
    int PersonaId,
    string PersonaName,
    string? PosterUrl = null,
    string? TravelLogUrl = null
);
