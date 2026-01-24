using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.Api.Services;

public class TripSummaryPromptBuilder(PromptStore prompts)
{
    private const string TripSummaryPromptPath = "Prompts/trip-summary.md";

    public string BuildPrompt(TripComputationResult trip, Persona persona)
    {
        var template = prompts.GetPrompt(TripSummaryPromptPath);

        return PromptRenderer.Render(template, new Dictionary<string, string>
        {
            ["PersonaDescription"] = persona.Description,
            ["Destination"] = trip.Destination,
            ["SpeedName"] = trip.SpeedName,
            ["SpeedMph"] = trip.SpeedMph.ToString("N0"),
            ["DistanceMiles"] = trip.DistanceMiles.ToString("N0"),
            ["EarthTimeFormatted"] = trip.EarthTimeFormatted,
            ["ShipTimeFormatted"] = trip.ShipTimeFormatted,
            ["TimeDifference"] = trip.TimeDifferenceFormatted,
            ["JourneyFocus"] = trip.JourneyFocus,
        });
    }
}




