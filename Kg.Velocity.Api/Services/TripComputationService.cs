using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Engine;
using Kg.Velocity.Math;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Works out the trip physics for /api/compute-trip: Earth time, ship time, and ready-to-show text.
/// No AI involved.
/// </summary>
public class TripComputationService
{
    /// <summary>Calculates all relativistic physics for a trip: time dilation, Lorentz factor, and formatted outputs.</summary>
    public TripComputationResult Compute(TripEvaluateRequest request)
    {
        // Keep the caller's wall-clock base time (including their offset) rather than converting to server-local time.
        var baseDate = request.StartTime.DateTime;

        // Earth time: Distance / Speed, with Speed in mph => hours => seconds
        double hoursElapsed = request.DistanceMiles / request.SpeedMph;
        double earthTimeSeconds = hoursElapsed * 3600.0;

        double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(request.SpeedMph);
        double shipTimeSeconds = earthTimeSeconds / lorentzFactor;

        double percentageOfLightSpeed = RelativisticPhysics.CalculatePercentageOfLightSpeed(request.SpeedMph);
        double distanceLightYears = RelativisticPhysics.MilesToLightYears(request.DistanceMiles);

        var weights = JourneyWeightCalculator.Compute(
            request.DistanceMiles,
            request.SpeedMph,
            shipTimeSeconds / 3600.0);

        string earthTimeFormatted = FlightComputer.FormatDuration(earthTimeSeconds);
        string shipTimeFormatted = FlightComputer.FormatDuration(shipTimeSeconds);
        string timeDifference = FlightComputer.CalculateTimeDifference(earthTimeSeconds, shipTimeSeconds);

        string departedEarthTime = FlightComputer.FormatDateTime(baseDate, 0);
        string arrivedEarthTime = FlightComputer.FormatDateTime(baseDate, earthTimeSeconds);
        string arrivedShipTime = FlightComputer.FormatDateTime(baseDate, shipTimeSeconds);

        return new TripComputationResult(
            Destination: request.Destination,
            SpeedName: request.SpeedName,
            SpeedMph: request.SpeedMph,
            PercentageOfLightSpeed: percentageOfLightSpeed,
            DistanceMiles: request.DistanceMiles,
            DistanceLightYears: distanceLightYears,
            EarthTimeSeconds: earthTimeSeconds,
            ShipTimeSeconds: shipTimeSeconds,
            EarthTimeFormatted: earthTimeFormatted,
            ShipTimeFormatted: shipTimeFormatted,
            TimeDifferenceFormatted: timeDifference,
            DepartedEarthTime: departedEarthTime,
            ArrivedEarthTime: arrivedEarthTime,
            ArrivedShipTime: arrivedShipTime,
            LorentzFactor: double.IsInfinity(lorentzFactor) ? null : lorentzFactor,
            Weights: weights
        );
    }
}















