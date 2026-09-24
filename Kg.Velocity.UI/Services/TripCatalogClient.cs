using System.Net.Http.Json;
using Kg.Velocity.Contracts.Catalogs;

namespace Kg.Velocity.UI.Services;

/// <summary>
/// Gets the list of destinations and speed presets from the API.
/// Each list is fetched once and then kept in memory for the rest of the session.
/// </summary>
public class TripCatalogClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private IReadOnlyList<DestinationDto>? _destinations;
    private IReadOnlyList<SpeedPresetDto>? _speedPresets;

    /// <summary>
    /// Returns the destinations. Adds a timestamp to the address so the browser
    /// always fetches a fresh copy rather than an old saved one.
    /// </summary>
    public async Task<IReadOnlyList<DestinationDto>> GetDestinationsAsync()
    {
        if (_destinations != null) return _destinations;

        var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _destinations = (await _httpClient.GetFromJsonAsync<List<DestinationDto>>($"/api/destinations?_={cacheBuster}"))
                        ?? [];
        return _destinations;
    }

    /// <summary>
    /// Returns the speed presets.
    /// </summary>
    public async Task<IReadOnlyList<SpeedPresetDto>> GetSpeedPresetsAsync()
    {
        if (_speedPresets != null) return _speedPresets;

        _speedPresets = (await _httpClient.GetFromJsonAsync<List<SpeedPresetDto>>("/api/speed-presets"))
                        ?? [];
        return _speedPresets;
    }
}
