using Kg.Velocity.Math;
using Kg.Velocity.Engine;
using Kg.Velocity.Engine.Models;
using Kg.Velocity.Blazor.Services;
using System.Collections.ObjectModel;

namespace Kg.Velocity.Blazor.ViewModels;

public class MainViewModel
{
    private readonly SimulationState _state;
    private readonly TripEvaluationService _tripEvaluationService;
    private DateTime _startDateTime;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel(TripEvaluationService tripEvaluationService)
    {
        _state = new SimulationState();
        _tripEvaluationService = tripEvaluationService;
        _startDateTime = DateTime.Now;

        InitializeDestinations();
        
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

    private void InitializeDestinations()
    {
        Destinations =
        [
            // Solar System
            new Destination { Name = "The Moon", DistanceMiles = 238_855, Category = "Space" },
            new Destination { Name = "Mercury", DistanceMiles = 56_000_000, Category = "Space" },
            new Destination { Name = "The Sun", DistanceMiles = 93_000_000, Category = "Space" },
            new Destination { Name = "Mars", DistanceMiles = 140_000_000, Category = "Space" },
            new Destination { Name = "Saturn", DistanceMiles = 886_000_000, Category = "Space" },
            new Destination { Name = "Uranus", DistanceMiles = 1_800_000_000, Category = "Space" },
            new Destination { Name = "Pluto", DistanceMiles = 3_700_000_000, Category = "Space" },
            
            // Deep Space
            new Destination { Name = "Proxima Centauri", DistanceMiles = 4.24 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Polaris (North Star)", DistanceMiles = 433 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Betelgeuse", DistanceMiles = 700 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Horsehead Nebula", DistanceMiles = 1_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Crab Nebula", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Pillars of Creation", DistanceMiles = 6_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Milky Way (center)", DistanceMiles = 26_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Andromeda Galaxy", DistanceMiles = 2_537_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space" }
        ];

        SelectedDestination = null;
    }

    // Properties
    public bool HasCalculated { get; set; }
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
    public ObservableCollection<Destination> Destinations { get; set; } = new();
    public List<SpeedPreset> SpeedPresets => Kg.Velocity.Engine.SpeedPresets.All;
    
    private Destination? _selectedDestination;
    public Destination? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            if (value != null)
            {
                _state.UpdateTargetDistance(value.DistanceMiles);
                
                // Recalculate if speed is already set
                if (_state.SpeedMph > 0)
                {
                    CalculateCompleteJourney();
                    HasCalculated = true;
                }
                else
                {
                    ClearCalculations();
                }
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
            var match = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _state.SpeedMph) < 0.001);
            return match?.SpeedMph;
        }
        set
        {
            if (value.HasValue && value.Value > 0)
            {
                _state.SpeedMph = value.Value;
                
                // Calculate journey when speed is selected and destination exists
                if (SelectedDestination != null)
                {
                    CalculateCompleteJourney();
                    HasCalculated = true;
                }
                NotifyStateChanged();
            }
            else
            {
                // Blank speed selected - clear speed and calculations
                _state.SpeedMph = 0;
                ClearCalculations();
            }
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private void CalculateCompleteJourney()
    {
        // Journey is complete - set distance to target
        _state.DistanceMiles = _state.TargetDistanceMiles;
        
        // Calculate time: Distance / Speed
        // Speed is in mph, so time in hours = distance / speed
        double hoursElapsed = _state.TargetDistanceMiles / _state.SpeedMph;
        _state.EarthTimeSeconds = hoursElapsed * 3600.0;
        
        // Calculate ship time using Lorentz factor
        double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);
        _state.ShipTimeSeconds = _state.EarthTimeSeconds / lorentzFactor;
        
        // Update all display properties
        UpdateDisplayProperties(lorentzFactor);
        
        // Fire async API call for summary
        _ = FetchJourneySummaryAsync();
    }
    
    private void UpdateDisplayProperties(double lorentzFactor)
    {
        SpeedMph = _state.SpeedMph;
        PercentageOfLightSpeed = RelativisticPhysics.CalculatePercentageOfLightSpeed(_state.SpeedMph);
        DistanceMiles = _state.DistanceMiles;
        DistanceLightYears = RelativisticPhysics.MilesToLightYears(_state.DistanceMiles);
        EarthTimeElapsed = FlightComputer.FormatDuration(_state.EarthTimeSeconds);
        ShipTimeElapsed = FlightComputer.FormatDuration(_state.ShipTimeSeconds);
        TimeDifference = FlightComputer.CalculateTimeDifference(_state.EarthTimeSeconds, _state.ShipTimeSeconds);
        ArrivalDateString = FlightComputer.FormatDateTime(_startDateTime, _state.EarthTimeSeconds);
        ArrivalShipDateString = FlightComputer.FormatDateTime(_startDateTime, _state.ShipTimeSeconds);
        
        // Show loading state immediately
        JourneySummary = "Generating trip summary...";
        PersonaName = "";
    }

    private async Task FetchJourneySummaryAsync()
    {
        if (SelectedDestination == null) return;

        var speedPreset = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _state.SpeedMph) < 0.001);
        string speedName = speedPreset?.Name ?? $"{SpeedMph:N0} mph";

        var request = new TripEvaluationRequest(
            Destination: SelectedDestination.Name,
            SpeedName: speedName,
            SpeedMph: _state.SpeedMph,
            DistanceMiles: _state.DistanceMiles,
            EarthTimeSeconds: _state.EarthTimeSeconds,
            ShipTimeSeconds: _state.ShipTimeSeconds,
            EarthTimeFormatted: EarthTimeElapsed,
            ShipTimeFormatted: ShipTimeElapsed,
            DepartedEarthTime: FlightComputer.FormatDateTime(_startDateTime, 0),
            ArrivedEarthTime: ArrivalDateString,
            ArrivedShipTime: ArrivalShipDateString,
            TimeDifference: TimeDifference
        );

        var (summary, personaName) = await _tripEvaluationService.EvaluateTripAsync(request);
        JourneySummary = summary;
        PersonaName = personaName;
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all calculation results without changing dropdown selections.
    /// </summary>
    private void ClearCalculations()
    {
        HasCalculated = false;
        
        _state.DistanceMiles = 0;
        _state.EarthTimeSeconds = 0;
        _state.ShipTimeSeconds = 0;

        _startDateTime = DateTime.Now;
        
        // Update display properties to reflect cleared state
        UpdatePropertiesWithoutNotification();
        JourneySummary = "";
        
        NotifyStateChanged();
    }
}
