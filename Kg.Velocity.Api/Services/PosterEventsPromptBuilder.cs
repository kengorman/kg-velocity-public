using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.Api.Services;

public class PosterEventsPromptBuilder(PromptStore prompts)
{
    private const string PosterEventsPromptPath = "Prompts/poster-events.md";

    /// <summary>Injects trip data into the poster events prompt template, separating prompt content from code.</summary>
    public string BuildPrompt(TripComputationResult trip)
    {
        var template = prompts.GetPrompt(PosterEventsPromptPath);

        var w = trip.Weights;

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
            ["WeightEmotion"] = w.Emotion.ToString("F1"),
            ["WeightDistance"] = w.Distance.ToString("F1"),
            ["WeightAwe"] = w.Awe.ToString("F1"),
            ["WeightTimeGoneBy"] = w.TimeGoneBy.ToString("F1"),
            ["WeightMemories"] = w.Memories.ToString("F1"),
            ["WeightPatience"] = w.Patience.ToString("F1"),
            ["WeightLoneliness"] = w.Loneliness.ToString("F1"),
        });
    }
}
