using System.Net.Http.Json;
using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.UI.Services;

/// <summary>
/// Makes the trip calls to the API: the trip numbers, the AI content,
/// and downloading the finished images.
/// </summary>
public class TripEvaluationService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Sets up the service with the HTTP client used to reach the API.
    /// </summary>
    public TripEvaluationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Asks the API to work out the trip numbers (times, distance, arrival dates). No AI involved.
    /// Any failure is rethrown with a readable "Error connecting to API" message.
    /// </summary>
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

    /// <summary>
    /// Asks the API for the AI-written summary, plus the addresses of the poster and mission log images.
    /// Any failure is rethrown with a readable "Error connecting to API" message.
    /// </summary>
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

    /// <summary>
    /// Downloads an image from the given address. Despite the name, it's used for
    /// both the poster and the mission log.
    /// </summary>
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
