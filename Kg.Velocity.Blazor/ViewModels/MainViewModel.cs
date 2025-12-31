using Kg.Velocity.Blazor.Services;
using Kg.Velocity.Contracts.Catalogs;
using Kg.Velocity.Contracts.Trips;
using System.Collections.ObjectModel;

namespace Kg.Velocity.Blazor.ViewModels;

public class MainViewModel
{
    private readonly TripEvaluationService _tripEvaluationService;
    private readonly PersonaIdStore _personaIdStore;
    private readonly TripCatalogClient _catalogClient;
    private DateTimeOffset _startTime;
    private double _selectedSpeedMph;
    private int _evaluationRequestVersion;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel(
        TripEvaluationService tripEvaluationService,
        PersonaIdStore personaIdStore,
        TripCatalogClient catalogClient)
    {
        _tripEvaluationService = tripEvaluationService;
        _personaIdStore = personaIdStore;
        _catalogClient = catalogClient;
        _startTime = DateTimeOffset.Now;
        _selectedSpeedMph = 0;

        // Initialize display properties without triggering state change
        UpdatePropertiesWithoutNotification();
    }
    
    private void UpdatePropertiesWithoutNotification()
    {
        // Initialize properties to safe defaults for first render
        SpeedMph = 0;
        PercentageOfLightSpeed = 0;
        DistanceMiles = 0;
        DistanceLightYears = 0;
        EarthTimeElapsed = "00:00:00";
        ShipTimeElapsed = "00:00:00";
        TimeDifference = "0s";
        ArrivalDateString = "N/A";
        ArrivalShipDateString = "N/A";
    }

    public async Task InitializeAsync()
    {
        var destinations = await _catalogClient.GetDestinationsAsync();
        Destinations = new ObservableCollection<DestinationDto>(destinations);

        var presets = await _catalogClient.GetSpeedPresetsAsync();
        SpeedPresets = presets.ToList();

        SelectedDestination = null;
        NotifyStateChanged();
    }

    // Properties
    public bool HasCalculated { get; set; }
    public bool IsCalculatingTrip { get; set; }
    public double SpeedMph { get; set; }
    public double PercentageOfLightSpeed { get; set; }
    public double DistanceMiles { get; set; }
    public double DistanceLightYears { get; set; }
    public string EarthTimeElapsed { get; set; } = "00:00:00";
    public string ShipTimeElapsed { get; set; } = "00:00:00";
    public string ArrivalDateString { get; set; } = "N/A";
    public string ArrivalShipDateString { get; set; } = "N/A";
    public string TimeDifference { get; set; } = "0s";
    public string JourneySummary { get; set; } = "";
    public string PersonaName { get; set; } = "";
    public ObservableCollection<DestinationDto> Destinations { get; set; } = new();
    public List<SpeedPresetDto> SpeedPresets { get; set; } = [];
    
    private DestinationDto? _selectedDestination;
    public DestinationDto? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            if (value != null)
            {
                // Re-evaluate if speed is already set
                if (_selectedSpeedMph > 0)
                    _ = EvaluateTripAsync();
                else
                    ClearCalculations();
            }
            else
            {
                // Blank destination selected - clear calculations
                ClearCalculations();
            }
            NotifyStateChanged();
        }
    }

    public double? SelectedPresetSpeed
    {
        get
        {
            var match = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _selectedSpeedMph) < 0.001);
            return match?.SpeedMph;
        }
        set
        {
            if (value.HasValue && value.Value > 0)
            {
                _selectedSpeedMph = value.Value;

                // Evaluate journey when speed is selected and destination exists
                if (SelectedDestination != null)
                    _ = EvaluateTripAsync();
                NotifyStateChanged();
            }
            else
            {
                // Blank speed selected - clear speed and calculations
                _selectedSpeedMph = 0;
                ClearCalculations();
            }
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private async Task EvaluateTripAsync()
    {
        if (SelectedDestination == null) return;
        if (_selectedSpeedMph <= 0) return;

        var requestVersion = ++_evaluationRequestVersion;

        var speedPreset = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _selectedSpeedMph) < 0.001);
        string speedName = speedPreset?.Name ?? $"{_selectedSpeedMph:N0} mph";

        HasCalculated = true;
        IsCalculatingTrip = true;
        JourneySummary = "Calculating trip...";
        PersonaName = "";
        NotifyStateChanged();

        try
        {
            var personaId = await _personaIdStore.TryGetAsync();
            _startTime = DateTimeOffset.Now;

            var request = new TripEvaluateRequest(
                Destination: SelectedDestination.Name,
                SpeedName: speedName,
                SpeedMph: _selectedSpeedMph,
                DistanceMiles: SelectedDestination.DistanceMiles,
                StartTime: _startTime,
                PersonaId: personaId
            );

            var response = await _tripEvaluationService.EvaluateTripAsync(request);
            if (requestVersion != _evaluationRequestVersion) return;

            await _personaIdStore.TrySetAsync(response.PersonaId);

            // Render authoritative server-computed values
            SpeedMph = response.Trip.SpeedMph;
            PercentageOfLightSpeed = response.Trip.PercentageOfLightSpeed;
            DistanceMiles = response.Trip.DistanceMiles;
            DistanceLightYears = response.Trip.DistanceLightYears;
            EarthTimeElapsed = response.Trip.EarthTimeFormatted;
            ShipTimeElapsed = response.Trip.ShipTimeFormatted;
            TimeDifference = response.Trip.TimeDifferenceFormatted;
            ArrivalDateString = response.Trip.ArrivedEarthTime;
            ArrivalShipDateString = response.Trip.ArrivedShipTime;

            JourneySummary = response.Summary;
            PersonaName = response.PersonaName;
            HasCalculated = true;
            IsCalculatingTrip = false;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            if (requestVersion != _evaluationRequestVersion) return;

            IsCalculatingTrip = false;
            JourneySummary = ex.Message;
            PersonaName = "System";
            HasCalculated = true;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Clears all calculation results without changing dropdown selections.
    /// </summary>
    private void ClearCalculations()
    {
        HasCalculated = false;
        IsCalculatingTrip = false;
        _startTime = DateTimeOffset.Now;
        
        // Update display properties to reflect cleared state
        UpdatePropertiesWithoutNotification();
        JourneySummary = "";
        
        NotifyStateChanged();
    }
}
