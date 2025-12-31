using System.Net.Http.Json;

namespace Kg.Velocity.Blazor.Services;

public record TimelineEvent(
    string Label,
    string? Description,
    string? EarthTime,
    string? ShipTime
);

public class TimelineResponse
{
    public List<TimelineEvent> Events { get; set; } = [];
}

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

public record TripEvaluationResponse(string Summary, TimelineResponse Timeline, int PersonaId, string PersonaName);

public class TripEvaluationService
{
    private readonly HttpClient _httpClient;

    public TripEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string Summary, string PersonaName, TimelineResponse Timeline, int PersonaId)> EvaluateTripAsync(TripEvaluationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/evaluate-trip", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TripEvaluationResponse>();
            return (
                result?.Summary ?? "Unable to generate summary.",
                result?.PersonaName ?? "",
                result?.Timeline ?? new TimelineResponse(),
                result?.PersonaId ?? 0
            );
        }
        catch (Exception ex)
        {
            return ($"Error connecting to API: {ex.Message}", "System", new TimelineResponse(), 0);
        }
    }
}





