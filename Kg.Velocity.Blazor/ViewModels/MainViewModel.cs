using Kg.Velocity.Math;
using Kg.Velocity.Engine;
using Kg.Velocity.Engine.Models;
using System.Collections.ObjectModel;

namespace Kg.Velocity.Blazor.ViewModels;

public class MainViewModel
{
    private readonly SimulationState _state;
    private DateTime _startDateTime;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel()
    {
        _state = new SimulationState();
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
            new Destination { Name = "Voyager 1", DistanceMiles = 15_000_000_000, Category = "Space" },
            
            // Deep Space
            new Destination { Name = "Polaris (North Star)", DistanceMiles = 433 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Horsehead Nebula", DistanceMiles = 5_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
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
        JourneySummary = GenerateJourneySummary();
    }

    private string GenerateJourneySummary()
    {
        if (SelectedDestination == null) return "";
        
        double earthYears = _state.EarthTimeSeconds / (365.25 * 24 * 3600);
        double shipYears = _state.ShipTimeSeconds / (365.25 * 24 * 3600);
        double timeSavedYears = earthYears - shipYears;
        string destination = SelectedDestination.Name;
        
        // Get speed context
        var speedPreset = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _state.SpeedMph) < 0.001);
        string speedName = speedPreset?.Name ?? $"{SpeedMph:N0} mph";
        
        // Format time dilation note
        string timeDilationNote = GetTimeDilationNote(earthYears, shipYears, timeSavedYears);
        
        // Generate contextual summary based on duration
        string mainSummary;
        if (earthYears < 0.0001) // Less than an hour
        {
            mainSummary = $"A quick hop to {destination} at {speedName}. You'd barely have time to finish your coffee.";
        }
        else if (earthYears < 0.01) // Less than a few days
        {
            double hours = _state.EarthTimeSeconds / 3600;
            mainSummary = $"At {speedName}, you'd reach {destination} in about {hours:N0} hours.";
        }
        else if (earthYears < 1)
        {
            double days = _state.EarthTimeSeconds / (24 * 3600);
            mainSummary = $"Traveling at {speedName}, {destination} is {days:N0} days away.";
        }
        else if (earthYears < 80)
        {
            mainSummary = $"At {speedName}, reaching {destination} would take {earthYears:N1} years.";
        }
        else if (earthYears < 1000)
        {
            mainSummary = $"At {speedName}, this {earthYears:N0}-year journey means everyone you know would be gone long before you arrive at {destination}.";
        }
        else if (earthYears < 1_000_000)
        {
            mainSummary = $"Traveling to {destination} at {speedName} would take {earthYears:N0} years. Human civilization is only about 10,000 years old.";
        }
        else if (earthYears < 1_000_000_000)
        {
            double millionYears = earthYears / 1_000_000;
            mainSummary = $"At {speedName}, reaching {destination} takes {millionYears:N1} million years. Homo sapiens have only existed for 0.3 million years.";
        }
        else
        {
            double billionYears = earthYears / 1_000_000_000;
            mainSummary = $"This {billionYears:N1} billion year journey to {destination} exceeds the remaining lifespan of our Sun. Earth itself may not exist when you arrive.";
        }
        
        // Append time dilation note if there's any difference
        if (!string.IsNullOrEmpty(timeDilationNote))
        {
            return $"{mainSummary} {timeDilationNote}";
        }
        return mainSummary;
    }
    
    private string GetTimeDilationNote(double earthYears, double shipYears, double timeSavedYears)
    {
        double timeSavedSeconds = _state.EarthTimeSeconds - _state.ShipTimeSeconds;
        
        // No meaningful difference
        if (timeSavedSeconds < 0.001) return "";
        
        // Less than a second
        if (timeSavedSeconds < 1)
        {
            double ms = timeSavedSeconds * 1000;
            return $"Due to time dilation, you'd age {ms:F1}ms less than those on Earth.";
        }
        // Less than a minute
        else if (timeSavedSeconds < 60)
        {
            return $"Due to time dilation, you'd age {timeSavedSeconds:F1} seconds less than those on Earth.";
        }
        // Less than an hour
        else if (timeSavedSeconds < 3600)
        {
            double minutes = timeSavedSeconds / 60;
            return $"Due to time dilation, you'd age {minutes:F1} minutes less than those on Earth.";
        }
        // Less than a day
        else if (timeSavedSeconds < 86400)
        {
            double hours = timeSavedSeconds / 3600;
            return $"Due to time dilation, you'd age {hours:F1} hours less than those on Earth.";
        }
        // Less than a year
        else if (timeSavedYears < 1)
        {
            double days = timeSavedSeconds / 86400;
            return $"Due to time dilation, you'd age {days:F0} days less than those on Earth.";
        }
        // Years
        else if (timeSavedYears < 1000)
        {
            return $"Due to time dilation, you'd age only {shipYears:F1} years while {earthYears:F1} years pass on Earth.";
        }
        // Massive difference
        else
        {
            return $"Time dilation is extreme: you'd experience {ShipTimeElapsed} while Earth ages {EarthTimeElapsed}.";
        }
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
        
        NotifyStateChanged();
    }
}
