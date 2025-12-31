using Kg.Velocity.Api.Models;

namespace Kg.Velocity.Api.Services;

public class TripSummaryPromptBuilder(PromptStore prompts)
{
    private const string TripSummaryPromptPath = "Prompts/trip-summary.md";

    public string BuildPrompt(TripEvaluationRequest request, Persona persona)
    {
        var template = prompts.GetPrompt(TripSummaryPromptPath);

        return PromptRenderer.Render(template, new Dictionary<string, string>
        {
            ["PersonaDescription"] = persona.Description,
            ["Destination"] = request.Destination,
            ["SpeedName"] = request.SpeedName,
            ["SpeedMph"] = request.SpeedMph.ToString("N0"),
            ["DistanceMiles"] = request.DistanceMiles.ToString("N0"),
            ["EarthTimeFormatted"] = request.EarthTimeFormatted,
            ["ShipTimeFormatted"] = request.ShipTimeFormatted,
            ["TimeDifference"] = request.TimeDifference,
        });
    }
}



