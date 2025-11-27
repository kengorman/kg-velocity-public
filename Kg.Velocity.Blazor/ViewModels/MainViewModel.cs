using Kg.Velocity.Math;
using Kg.Velocity.Engine;
using Kg.Velocity.Engine.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Kg.Velocity.Blazor.ViewModels;

public class MainViewModel
{
    private readonly SimulationState _state;
    private readonly SimulationEngine _engine;
    private readonly Stopwatch _stopwatch;
    private double _lastElapsedSeconds;
    private DateTime _startDateTime;
    private double _timeSinceLastAverageUpdate;
    private double _timeSinceLastTimeDiffUpdate;

    // Keyboard state
    private bool _increaseHeld;
    private bool _decreaseHeld;
    private bool _slowHeld;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    public MainViewModel()
    {
        _state = new SimulationState();
        _engine = new SimulationEngine(_state);
        _stopwatch = Stopwatch.StartNew();
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
        LorentzFactor = 1;
        ShipClockRate = 1;
        DistanceMiles = 0;
        DistanceLightYears = 0;
        RemainingMiles = 0;
        RemainingLightYears = 0;
        EarthTimeElapsed = "00:00:00";
        ShipTimeElapsed = "00:00:00";
        EarthDateTime = _startDateTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
        ShipDateTime = _startDateTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
        DestinationReached = false;
        JourneyProgressPercentage = 0;
        EstimatedTimeOfArrivalEarth = "N/A";
        EstimatedTimeOfArrivalShip = "N/A";
        TimeDifference = "0s";
        JourneySummary = "Select a destination\nAdjust the ship's speed using the Faster/Slower buttons.\nTap Launch to begin.";
        AverageSpeedMph = 0;
        AverageSpeedPercentLight = 0;
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
    public bool IsAccelerating { get; set; }
    public bool IsDecelerating { get; set; }
    public bool IsLaunched { get; set; }
    public double SpeedMph { get; set; }
    public double PercentageOfLightSpeed { get; set; }
    public double LorentzFactor { get; set; }
    public double ShipClockRate { get; set; }
    public double DistanceMiles { get; set; }
    public double DistanceLightYears { get; set; }
    public double RemainingMiles { get; set; }
    public double RemainingLightYears { get; set; }
    public string EarthTimeElapsed { get; set; } = "00:00:00";
    public string ShipTimeElapsed { get; set; } = "00:00:00";
    public string EarthDateTime { get; set; } = "";
    public string ShipDateTime { get; set; } = "";
    public string ArrivalDateString { get; set; } = "N/A";
    public string ArrivalShipDateString { get; set; } = "N/A";
    public bool DestinationReached { get; set; }
    public double TargetDistanceLightYears { get; set; } = PhysicsConstants.TargetDistanceLightYears;
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
                ResetSimulation(value.DistanceMiles);
                NotifyStateChanged();
            }
        }
    }
    
    public double JourneyProgressPercentage { get; set; }
    public string EstimatedTimeOfArrivalEarth { get; set; } = "N/A";
    public string EstimatedTimeOfArrivalShip { get; set; } = "N/A";
    public string TravelingFor { get; set; } = "0m";
    public string TimeDifference { get; set; } = "0s";
    public string JourneySummary { get; set; } = "";
    public double AverageSpeedMph { get; set; }
    public double AverageSpeedPercentLight { get; set; }

    public double? SelectedPresetSpeed
    {
        get
        {
            var match = SpeedPresets.FirstOrDefault(p => System.Math.Abs(p.SpeedMph - _state.SpeedMph) < 0.001);
            return match?.SpeedMph;
        }
        set
        {
            if (value.HasValue)
            {
                _state.SpeedMph = value.Value;
                _state.WHeldSeconds = 0;
                _state.XHeldSeconds = 0;
                
                // Auto-launch when speed is selected
                if (SelectedDestination != null)
                {
                    Launch();
                }
                else
                {
                    NotifyStateChanged();
                }
            }
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private void ResetSimulation(double newTargetMiles)
    {
        IsLaunched = false;
        
        _state.SpeedMph = 0;
        _state.DistanceMiles = 0;
        _state.EarthTimeSeconds = 0;
        _state.ShipTimeSeconds = 0;
        _state.WHeldSeconds = 0;
        _state.XHeldSeconds = 0;

        _state.UpdateTargetDistance(newTargetMiles);
        
        double newTargetLightYears = newTargetMiles / PhysicsConstants.LightYearMiles;
        TargetDistanceLightYears = newTargetLightYears;

        _stopwatch.Restart();
        _lastElapsedSeconds = 0;
        _startDateTime = DateTime.Now;
        _timeSinceLastAverageUpdate = 0.0;
        _timeSinceLastTimeDiffUpdate = 0.0;
        
        // Update display properties to reflect reset state
        UpdatePropertiesWithoutNotification();
    }

    public void SetIncreaseHeld(bool held)
    {
        if (SelectedDestination == null || _state.DestinationReached)
            return;
            
        _increaseHeld = held;
        IsAccelerating = held;
    }

    public void SetDecreaseHeld(bool held)
    {
        if (SelectedDestination == null || _state.DestinationReached)
            return;
            
        _decreaseHeld = held;
        IsDecelerating = held;
    }

    public void SetSlowHeld(bool held)
    {
        if (SelectedDestination == null || _state.DestinationReached)
            return;
            
        _slowHeld = held;
    }

    public void Launch()
    {
        if (SelectedDestination == null)
        {
            return;
        }
        
        if (_state.SpeedMph <= 0)
        {
            _state.SpeedMph = 1.0;
        }
        
        IsLaunched = true;
        
        // Instant calculation: Calculate entire journey
        CalculateCompleteJourney();
        
        NotifyStateChanged();
    }
    
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
        LorentzFactor = lorentzFactor;
        ShipClockRate = RelativisticPhysics.CalculateShipClockRate(lorentzFactor);
        DistanceMiles = _state.DistanceMiles;
        DistanceLightYears = RelativisticPhysics.MilesToLightYears(_state.DistanceMiles);
        RemainingMiles = _state.RemainingDistanceMiles;
        RemainingLightYears = RelativisticPhysics.MilesToLightYears(_state.RemainingDistanceMiles);
        EarthTimeElapsed = FlightComputer.FormatDuration(_state.EarthTimeSeconds);
        ShipTimeElapsed = FlightComputer.FormatDuration(_state.ShipTimeSeconds);
        EarthDateTime = FlightComputer.FormatDateTime(_startDateTime, _state.EarthTimeSeconds);
        ShipDateTime = FlightComputer.FormatDateTime(_startDateTime, _state.ShipTimeSeconds);
        DestinationReached = true;
        
        JourneyProgressPercentage = 100.0;
        
        var (avgMph, avgPercent) = FlightComputer.CalculateAverageSpeed(_state.DistanceMiles, _state.EarthTimeSeconds);
        AverageSpeedMph = avgMph;
        AverageSpeedPercentLight = avgPercent;
        
        // ETAs are N/A since we've arrived
        EstimatedTimeOfArrivalEarth = "N/A";
        EstimatedTimeOfArrivalShip = "N/A";
        
        TravelingFor = CalculateTravelingFor();
        TimeDifference = FlightComputer.CalculateTimeDifference(_state.EarthTimeSeconds, _state.ShipTimeSeconds);
        
        // Arrival dates show "Arrived"
        ArrivalDateString = "Arrived";
        ArrivalShipDateString = "Arrived";
        
        JourneySummary = GenerateJourneySummary();
    }

    public void Reset()
    {
        IsLaunched = false;
        
        _state.SpeedMph = 0;
        _state.DistanceMiles = 0;
        _state.EarthTimeSeconds = 0;
        _state.ShipTimeSeconds = 0;
        _state.WHeldSeconds = 0;
        _state.XHeldSeconds = 0;

        SelectedDestination = null;

        _stopwatch.Restart();
        _lastElapsedSeconds = 0;
        _startDateTime = DateTime.Now;
        _timeSinceLastAverageUpdate = 0.0;
        _timeSinceLastTimeDiffUpdate = 0.0;
        
        // Update display properties to reflect reset state
        UpdatePropertiesWithoutNotification();
        
        NotifyStateChanged();
    }

    public void UpdateFromClickPosition(double percentage)
    {
        // Can't click after reaching destination
        if (_state.DestinationReached)
            return;

        // Clamp percentage to valid range
        percentage = System.Math.Clamp(percentage, 0.0, 100.0);

        // Calculate new distance based on percentage
        double newDistance = (percentage / 100.0) * _state.TargetDistanceMiles;
        
        // Clamp to target (don't exceed destination)
        newDistance = System.Math.Min(newDistance, _state.TargetDistanceMiles);
        _state.DistanceMiles = newDistance;

        if (!IsLaunched)
        {
            // Pre-launch: keep time at 0
            _state.EarthTimeSeconds = 0.0;
            _state.ShipTimeSeconds = 0.0;
        }
        else
        {
            // During flight: calculate time based on distance traveled at current speed
            // This gives a rough approximation for scrubbing
            if (_state.SpeedMph > 0)
            {
                // Calculate Earth time: distance / speed
                double hoursElapsed = newDistance / _state.SpeedMph;
                _state.EarthTimeSeconds = hoursElapsed * 3600.0;
                
                // Calculate ship time using Lorentz factor
                double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);
                _state.ShipTimeSeconds = _state.EarthTimeSeconds / lorentzFactor;
            }
        }
        
        // RemainingDistanceMiles and DestinationReached are computed properties - no need to set them
        
        NotifyStateChanged();
    }

    public void Update()
    {
        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        double deltaSeconds = elapsedSeconds - _lastElapsedSeconds;
        _lastElapsedSeconds = elapsedSeconds;

        if (deltaSeconds <= 0)
            return;

        if (_state.DestinationReached)
        {
            // Do nothing - keep all values frozen until reset
        }
        else
        {
            _state.WHeldSeconds = _increaseHeld ? _state.WHeldSeconds + deltaSeconds : 0.0;
            _state.XHeldSeconds = _decreaseHeld ? _state.XHeldSeconds + deltaSeconds : 0.0;

            _engine.Update(deltaSeconds, _increaseHeld, _decreaseHeld, _slowHeld, IsLaunched);
        }

        double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);

        SpeedMph = _state.SpeedMph;
        PercentageOfLightSpeed = RelativisticPhysics.CalculatePercentageOfLightSpeed(_state.SpeedMph);
        LorentzFactor = lorentzFactor;
        ShipClockRate = RelativisticPhysics.CalculateShipClockRate(lorentzFactor);
        DistanceMiles = _state.DistanceMiles;
        DistanceLightYears = RelativisticPhysics.MilesToLightYears(_state.DistanceMiles);
        RemainingMiles = _state.RemainingDistanceMiles;
        RemainingLightYears = RelativisticPhysics.MilesToLightYears(_state.RemainingDistanceMiles);
        EarthTimeElapsed = FlightComputer.FormatDuration(_state.EarthTimeSeconds);
        ShipTimeElapsed = FlightComputer.FormatDuration(_state.ShipTimeSeconds);
        EarthDateTime = _startDateTime.AddSeconds(_state.EarthTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        ShipDateTime = _startDateTime.AddSeconds(_state.ShipTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        DestinationReached = _state.DestinationReached;
        
        JourneyProgressPercentage = FlightComputer.CalculateJourneyProgress(_state.DistanceMiles, _state.TargetDistanceMiles);

        _timeSinceLastAverageUpdate += deltaSeconds;
        if (_timeSinceLastAverageUpdate >= 1.0)
        {
            var (avgMph, avgPercent) = FlightComputer.CalculateAverageSpeed(_state.DistanceMiles, _state.EarthTimeSeconds);
            AverageSpeedMph = avgMph;
            AverageSpeedPercentLight = avgPercent;
            _timeSinceLastAverageUpdate = 0.0;
        }

        // Calculate ETAs (Durations)
        if (SpeedMph > 0 && RemainingMiles > 0)
        {
            // Earth Duration
            double secondsToArriveEarth = RemainingMiles / (SpeedMph / 3600.0);
            EstimatedTimeOfArrivalEarth = FlightComputer.FormatDuration(secondsToArriveEarth, DurationFormat.Verbose);

            // Ship Duration (Time Dilated)
            double secondsToArriveShip = secondsToArriveEarth / LorentzFactor;
            EstimatedTimeOfArrivalShip = FlightComputer.FormatDuration(secondsToArriveShip, DurationFormat.Verbose);
        }
        else
        {
            EstimatedTimeOfArrivalEarth = "N/A";
            EstimatedTimeOfArrivalShip = "N/A";
        }

        TravelingFor = CalculateTravelingFor();

        _timeSinceLastTimeDiffUpdate += deltaSeconds;
        if (_timeSinceLastTimeDiffUpdate >= 1.0)
        {
            TimeDifference = FlightComputer.CalculateTimeDifference(_state.EarthTimeSeconds, _state.ShipTimeSeconds);
            _timeSinceLastTimeDiffUpdate = 0.0;
        }

        // Calculate Arrival Dates
        if (IsLaunched && !DestinationReached && SpeedMph > 0 && RemainingMiles > 0)
        {
            // Earth Arrival
            double secondsToArrive = RemainingMiles / (SpeedMph / 3600.0);
            DateTime currentEarthTime = _startDateTime.AddSeconds(_state.EarthTimeSeconds);
            ArrivalDateString = FormatArrivalDate(currentEarthTime, secondsToArrive);

            // Ship Arrival (Projected based on current Lorentz Factor)
            double shipSecondsToArrive = secondsToArrive / LorentzFactor;
            DateTime currentShipTime = _startDateTime.AddSeconds(_state.ShipTimeSeconds);
            ArrivalShipDateString = FormatArrivalDate(currentShipTime, shipSecondsToArrive);
        }
        else if (DestinationReached)
        {
            ArrivalDateString = "Arrived";
            ArrivalShipDateString = "Arrived";
        }
        else
        {
            ArrivalDateString = "N/A";
            ArrivalShipDateString = "N/A";
        }

        JourneySummary = GenerateJourneySummary();
        
        NotifyStateChanged();
    }

    private string FormatArrivalDate(DateTime currentBaseDate, double secondsToAdd)
    {
        return FlightComputer.FormatDateTime(currentBaseDate, secondsToAdd);
    }

    private string CalculateTravelingFor()
    {
        return FlightComputer.FormatDuration(_state.EarthTimeSeconds, DurationFormat.Compact);
    }

    private string GenerateJourneySummary()
    {
        if (SelectedDestination == null)
        {
            return "Select destination\nSet speed with preset or 'Faster'\nTap Launch to begin";
        }

        string destinationName = SelectedDestination.Name;
        string speedText = $"{SpeedMph:N0} mph";
        string percentLight = $"{PercentageOfLightSpeed:F6}%";

        if (!IsLaunched)
        {
            return $"→ {destinationName}\nSpeed: {speedText} ({percentLight} c)\nTap Launch to begin";
        }
        else
        {
            string etaText = EstimatedTimeOfArrivalEarth != "N/A" ? $"ETA: {EstimatedTimeOfArrivalEarth}" : "";
            string timeDiffText = TimeDifference != "0s" && TimeDifference != "0ms" 
                ? $"ΔTime: -{TimeDifference}" 
                : "ΔTime: 0s";
            
            string avgSpeedText = AverageSpeedMph > 0 
                ? $"Avg: {AverageSpeedMph:N0} mph ({AverageSpeedPercentLight:F6}% c)" 
                : "";

            if (DestinationReached)
            {
                var lines = new List<string>
                {
                    $"Arrived at {destinationName}!",
                    $"Final: {speedText} ({percentLight} c)"
                };
                
                if (!string.IsNullOrEmpty(avgSpeedText))
                    lines.Add(avgSpeedText);
                    
                lines.Add(timeDiffText);
                lines.Add("Tap Reset to restart");
                
                return string.Join("\n", lines);
            }
            else
            {
                var lines = new List<string>
                {
                    $"→ {destinationName} @ {speedText} ({percentLight} c)"
                };
                
                if (!string.IsNullOrEmpty(avgSpeedText))
                    lines.Add(avgSpeedText);
                    
                lines.Add($"Duration: {TravelingFor}");
                
                if (!string.IsNullOrEmpty(etaText))
                    lines.Add(etaText);
                    
                lines.Add(timeDiffText);
                
                return string.Join("\n", lines);
            }
        }
    }
}
