using Kg.Velocity.Api.Services;
using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Math;

namespace Kg.Velocity.Tests;

public class TripComputationServiceTests
{
    private const double C = PhysicsConstants.SpeedOfLightMph;
    private const double SecondsPerYear = 365.25 * 24 * 3600;
    private const double MoonMiles = 238_855; // same as TripCatalogService

    private static TripComputationResult Compute(double distanceMiles, double speedMph) =>
        new TripComputationService().Compute(new TripEvaluateRequest(
            "Test", "Test speed", speedMph, distanceMiles,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void EarthTime_IsDistanceOverSpeed()
    {
        var trip = Compute(MoonMiles, 3);

        Assert.Equal(MoonMiles / 3 * 3600, trip.EarthTimeSeconds, 6);
        Assert.Equal(9.08, trip.EarthTimeSeconds / SecondsPerYear, 2); // walking to the Moon: about 9 years
    }

    [Fact]
    public void ShipTime_IsEarthTimeOverGamma_NearLightSpeed()
    {
        var trip = Compute(MoonMiles, C * 0.99);

        Assert.Equal(7.0888, trip.EarthTimeSeconds / trip.ShipTimeSeconds, 4);
        Assert.Equal(7.0888, trip.LorentzFactor!.Value, 4);
    }

    [Fact]
    public void ShipTime_MatchesEarthTime_AtWalkingSpeed()
    {
        var trip = Compute(MoonMiles, 3);

        Assert.Equal(trip.EarthTimeSeconds, trip.ShipTimeSeconds, 3);
    }

    [Fact]
    public void FasterThanLight_IsInstantForTraveler_ButEarthStillWaits()
    {
        var trip = Compute(PhysicsConstants.LightYearMiles, C * 2);

        Assert.Equal(0.0, trip.ShipTimeSeconds);
        Assert.Null(trip.LorentzFactor); // infinite, so not sent
        Assert.Equal(0.5, trip.EarthTimeSeconds / SecondsPerYear, 2); // one light-year at 2x light speed
    }
}
