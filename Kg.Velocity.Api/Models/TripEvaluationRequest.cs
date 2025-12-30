namespace Kg.Velocity.Api.Models;

public record TripEvaluationRequest(
    string Destination,
    string SpeedName,
    double SpeedMph,
    double DistanceMiles,
    double EarthTimeSeconds,
    double ShipTimeSeconds,
    string EarthTimeFormatted,
    string ShipTimeFormatted,
    string DepartedEarthTime,
    string ArrivedEarthTime,
    string ArrivedShipTime,
    string TimeDifference,
    int? PersonaId = null
);





