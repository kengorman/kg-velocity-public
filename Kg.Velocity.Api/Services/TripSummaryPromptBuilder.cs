using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Engine;
using Kg.Velocity.Math;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Fills in the trip summary prompt (Prompts/trip-summary.md) with this trip's details and its journey insight
/// (what the trip is really about), from JourneyInsightClassifier.
/// </summary>
public class TripSummaryPromptBuilder(PromptStore prompts)
{
    private const string TripSummaryPromptPath = "Prompts/trip-summary.md";

    /// <summary>Injects trip data into the summary prompt template, separating prompt content from code.</summary>
    public string BuildPrompt(TripComputationResult trip)
    {
        var template = prompts.GetPrompt(TripSummaryPromptPath);

        // Compute the journey insight to guide narrative focus
        var lorentzFactor = trip.LorentzFactor ?? 1.0;
        var insight = JourneyInsightClassifier.Classify(
            trip.EarthTimeSeconds,
            trip.ShipTimeSeconds,
            trip.SpeedMph,
            lorentzFactor);

        return PromptRenderer.Render(template, new Dictionary<string, string>
        {
            ["Destination"] = trip.Destination,
            ["SpeedName"] = trip.SpeedName,
            ["SpeedMph"] = trip.SpeedMph.ToString("N0"),
            ["DistanceMiles"] = trip.DistanceMiles.ToString("N0"),
            ["EarthTimeFormatted"] = trip.EarthTimeFormatted,
            ["ShipTimeFormatted"] = trip.ShipTimeFormatted,
            ["TimeDifference"] = trip.TimeDifferenceFormatted,
            ["JourneyInsight"] = insight,
        });
    }
}
