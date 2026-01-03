using System.Net.Http.Json;
using Kg.Velocity.Contracts.Catalogs;

namespace Kg.Velocity.Blazor.Services;

public class TripCatalogClient
{
    private readonly HttpClient _httpClient;
    private IReadOnlyList<DestinationDto>? _destinations;
    private IReadOnlyList<SpeedPresetDto>? _speedPresets;

    public TripCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DestinationDto>> GetDestinationsAsync()
    {
        if (_destinations != null) return _destinations;

        _destinations = (await _httpClient.GetFromJsonAsync<List<DestinationDto>>("/api/destinations"))
                        ?? [];
        return _destinations;
    }

    public async Task<IReadOnlyList<SpeedPresetDto>> GetSpeedPresetsAsync()
    {
        if (_speedPresets != null) return _speedPresets;

        _speedPresets = (await _httpClient.GetFromJsonAsync<List<SpeedPresetDto>>("/api/speed-presets"))
                        ?? [];
        return _speedPresets;
    }
}












