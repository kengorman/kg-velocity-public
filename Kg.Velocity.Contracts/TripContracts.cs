namespace Kg.Velocity.Contracts.Trips;

/// <summary>
/// Seven "journey weights" for a trip, as percentages that add up to 100.
/// Each one says how much the AI travel log should lean on that part of the trip.
/// </summary>
/// <remarks>
/// Worked out by <c>JourneyWeightCalculator</c> in the Engine. Only the travel log uses these;
/// the trip summary uses <c>JourneyInsightClassifier</c> instead.
/// </remarks>
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
/// What the app sends to the API for a trip: just the user's choices. The API works out everything else.
/// </summary>
/// <remarks>
/// Sent to both <c>POST /api/compute-trip</c> and <c>POST /api/generate-content</c>.
/// </remarks>
/// <param name="Destination">Destination name, e.g. "Mars".</param>
/// <param name="SpeedName">Speed preset name, e.g. "99% Light Speed".</param>
/// <param name="SpeedMph">Speed in miles per hour.</param>
/// <param name="DistanceMiles">Distance to the destination, in miles.</param>
/// <param name="StartTime">When the trip starts; used to work out the arrival dates.</param>
public sealed record TripEvaluateRequest(
    string Destination,
    string SpeedName,
    double SpeedMph,
    double DistanceMiles,
    DateTimeOffset StartTime
);

/// <summary>
/// The trip's physics results, worked out by the API, plus ready-to-show text versions.
/// Returned by <c>POST /api/compute-trip</c>.
/// </summary>
/// <param name="PercentageOfLightSpeed">Speed as a percentage of light speed; over 100 means faster than light.</param>
/// <param name="EarthTimeSeconds">How long the trip takes as seen from Earth.</param>
/// <param name="ShipTimeSeconds">How long the trip feels to the traveler; shorter than Earth time near light speed, and zero at or above it.</param>
/// <param name="TimeDifferenceFormatted">How much more time passed on Earth than on the ship, e.g. "3d 4h".</param>
/// <param name="DepartedEarthTime">Start date and time, as text.</param>
/// <param name="ArrivedEarthTime">Arrival date and time by Earth's clock, as text.</param>
/// <param name="ArrivedShipTime">Arrival date and time by the ship's clock, as text.</param>
/// <param name="LorentzFactor">How much the ship's clock slows down (1 = not at all). Null at light speed and
/// faster, where the real value would be infinite.</param>
/// <param name="Weights">Journey weights for the AI travel log.</param>
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

// Not used: this was the response for /api/evaluate-trip, which is commented out in the API's Program.cs.
// The app now calls /api/compute-trip (returns TripComputationResult) and /api/generate-content
// (returns TripContentResponse). Kept here for reference.
//
// public sealed record TripEvaluateResponse(
//     TripComputationResult Trip,
//     string Summary,
//     int PersonaId,
//     string PersonaName,
//     string? PosterUrl = null
// );

/// <summary>
/// The AI-generated content for a trip. Returned by <c>POST /api/generate-content</c>.
/// </summary>
/// <param name="Summary">The AI-written trip summary.</param>
/// <param name="PersonaId">Id of the narrator style used (currently always the default narrator).</param>
/// <param name="PersonaName">Name of the narrator style used, e.g. "Narrator".</param>
/// <param name="PosterUrl">Link to the trip poster image (SVG).</param>
/// <param name="TravelLogUrl">Link to the travel log image (SVG).</param>
public sealed record TripContentResponse(
    string Summary,
    int PersonaId,
    string PersonaName,
    string? PosterUrl = null,
    string? TravelLogUrl = null
);
