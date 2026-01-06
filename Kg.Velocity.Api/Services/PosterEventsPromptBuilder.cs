using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.Api.Services;

public class PosterEventsPromptBuilder(PromptStore prompts)
{
    private const string PosterEventsPromptPath = "Prompts/poster-events.md";

    public string BuildPrompt(TripComputationResult trip, string summary)
    {
        var template = prompts.GetPrompt(PosterEventsPromptPath);

        return PromptRenderer.Render(template, new Dictionary<string, string>
        {
            ["Destination"] = trip.Destination,
            ["DistanceLightYears"] = trip.DistanceLightYears.ToString("N4"),
            ["DistanceMiles"] = trip.DistanceMiles.ToString("N0"),
            ["SpeedName"] = trip.SpeedName,
            ["PercentageOfLightSpeed"] = trip.PercentageOfLightSpeed.ToString("N6"),
            ["EarthTimeFormatted"] = trip.EarthTimeFormatted,
            ["ShipTimeFormatted"] = trip.ShipTimeFormatted,
            ["TimeDifference"] = trip.TimeDifferenceFormatted,
//            ["Summary"] = summary,
        });
    }
}
