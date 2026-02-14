using Kg.Velocity.Contracts.Trips;
using Kg.Velocity.Engine;

namespace Kg.Velocity.Api.Services;

public class PosterEventsPromptBuilder(PromptStore prompts)
{
    private const string PosterEventsPromptPath = "Prompts/poster-events.md";

    /// <summary>Injects trip data into the poster events prompt template, separating prompt content from code.</summary>
    public string BuildPrompt(TripComputationResult trip)
    {
        var template = prompts.GetPrompt(PosterEventsPromptPath);

        var lorentzFactor = trip.LorentzFactor ?? 1.0;
        var insight = JourneyInsightClassifier.Classify(
            trip.EarthTimeSeconds,
            trip.ShipTimeSeconds,
            trip.SpeedMph,
            lorentzFactor);

        return PromptRenderer.Render(template, new Dictionary<string, string>
        {
            ["Destination"] = trip.Destination,
            ["DepartedTime"] = trip.DepartedEarthTime,
            ["DistanceLightYears"] = trip.DistanceLightYears.ToString("N4"),
            ["DistanceMiles"] = trip.DistanceMiles.ToString("N0"),
            ["SpeedName"] = trip.SpeedName,
            ["PercentageOfLightSpeed"] = trip.PercentageOfLightSpeed.ToString("N6"),
            ["EarthTimeFormatted"] = trip.EarthTimeFormatted,
            ["ShipTimeFormatted"] = trip.ShipTimeFormatted,
            ["TimeDifference"] = trip.TimeDifferenceFormatted,
            ["JourneyInsight"] = insight,
        });
    }
}
