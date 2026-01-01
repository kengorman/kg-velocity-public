using System.Net.Http.Json;
using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.Blazor.Services;

public class TripEvaluationService
{
    private readonly HttpClient _httpClient;

    public TripEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TripEvaluateResponse> EvaluateTripAsync(TripEvaluateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/evaluate-trip", request);
            response.EnsureSuccessStatusCode();
            
            return (await response.Content.ReadFromJsonAsync<TripEvaluateResponse>())
                   ?? throw new InvalidOperationException("Empty response from API.");
        }
        catch (Exception ex)
        {
            // Keep a consistent error surface for callers.
            throw new InvalidOperationException($"Error connecting to API: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GetPosterBytesAsync(string posterUrl)
    {
        if (string.IsNullOrWhiteSpace(posterUrl))
            throw new ArgumentException("Poster URL is required.", nameof(posterUrl));

        try
        {
            return await _httpClient.GetByteArrayAsync(posterUrl);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error downloading poster: {ex.Message}", ex);
        }
    }
}





