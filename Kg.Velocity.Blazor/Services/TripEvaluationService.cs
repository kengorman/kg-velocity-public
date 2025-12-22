using System.Net.Http.Json;

namespace Kg.Velocity.Blazor.Services;

public record TripEvaluationRequest(
    string Destination,
    string SpeedName,
    double SpeedMph,
    double DistanceMiles,
    double EarthTimeSeconds,
    double ShipTimeSeconds,
    string EarthTimeFormatted,
    string ShipTimeFormatted,
    string TimeDifference
);

public record TripEvaluationResponse(string Summary);

public class TripEvaluationService
{
    private readonly HttpClient _httpClient;

    public TripEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> EvaluateTripAsync(TripEvaluationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/evaluate-trip", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TripEvaluationResponse>();
            return result?.Summary ?? "Unable to generate summary.";
        }
        catch (Exception ex)
        {
            return $"Error connecting to API: {ex.Message}";
        }
    }
}





