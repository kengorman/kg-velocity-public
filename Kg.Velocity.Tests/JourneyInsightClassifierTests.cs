using Kg.Velocity.Api.Services;
using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Engine;
using Kg.Velocity.Math;

namespace Kg.Velocity.Tests;

// These are the example trips in the README, so the README stays true.
public class JourneyInsightClassifierTests
{
    private const double C = PhysicsConstants.SpeedOfLightMph;
    private const double MoonMiles = 238_855; // same as TripCatalogService
    private const double AndromedaMiles = 2_537_000 * PhysicsConstants.LightYearMiles;

    // Same steps the app takes: work out the trip, then classify it
    // the way TripSummaryPromptBuilder does (γ of 1 when faster than light).
    private static string Classify(double distanceMiles, double speedMph)
    {
        var trip = new TripComputationService().Compute(new TripEvaluateRequest(
            "Test", "Test speed", speedMph, distanceMiles, DateTimeOffset.Now));

        return JourneyInsightClassifier.Classify(
            trip.EarthTimeSeconds, trip.ShipTimeSeconds, speedMph, trip.LorentzFactor ?? 1.0);
    }

    [Theory]
    [InlineData(MoonMiles, 3, "duration")]                     // walking to the Moon: 9 years
    [InlineData(MoonMiles, C * 0.99, "speed and dilation")]    // about a second, and clocks split 7 to 1
    [InlineData(AndromedaMiles, C * 1000, "farewell")]         // instant for you, 2,537 years back home
    [InlineData(AndromedaMiles, C * 0.99, "farewell and scale")]
    [InlineData(MoonMiles, 65, "journey")]                     // a few months' drive: nothing extreme
    public void Classify_PicksTheExpectedStory(double distanceMiles, double speedMph, string expected)
    {
        Assert.Equal(expected, Classify(distanceMiles, speedMph));
    }

    [Fact]
    public void FasterThanLight_NeverScoresDilation()
    {
        var scores = JourneyInsightClassifier.GetScores(
            earthTimeSeconds: 1e9, shipTimeSeconds: 0, speedMph: C * 10, lorentzFactor: 1.0);

        Assert.Equal(0, scores["dilation"]);
    }

    [Fact]
    public void Duration_OnlyScores_AtHumanSpeeds()
    {
        var tenThousandYears = 10_000 * 365.25 * 24 * 3600;

        var walking = JourneyInsightClassifier.GetScores(tenThousandYears, tenThousandYears, 3, 1.0);
        var spacecraft = JourneyInsightClassifier.GetScores(tenThousandYears, tenThousandYears, 430_000, 1.0);

        Assert.Equal(100, walking["duration"]);
        Assert.Equal(0, spacecraft["duration"]);
    }
}
