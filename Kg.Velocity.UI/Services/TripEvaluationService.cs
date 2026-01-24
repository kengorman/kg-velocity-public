using System.Net.Http.Json;
using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.UI.Services;

public class TripEvaluationService
{
    private readonly HttpClient _httpClient;

    public TripEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TripComputationResult> ComputeTripAsync(TripEvaluateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/compute-trip", request);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<TripComputationResult>())
                   ?? throw new InvalidOperationException("Empty response from API.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error connecting to API: {ex.Message}", ex);
        }
    }

    public async Task<TripContentResponse> GenerateContentAsync(TripEvaluateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/generate-content", request);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<TripContentResponse>())
                   ?? throw new InvalidOperationException("Empty response from API.");
        }
        catch (Exception ex)
        {
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
