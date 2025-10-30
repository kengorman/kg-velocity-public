using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Kg.Velocity.Avalonia.Models;
using Kg.Velocity.Math;

namespace Kg.Velocity.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SimulationState _state;
    private readonly SimulationEngine _engine;
    private readonly Stopwatch _stopwatch;
    private double _lastElapsedSeconds;
    private DateTime _startDateTime;

    // Keyboard state
    private bool _increaseHeld;
    private bool _decreaseHeld;
    private bool _slowHeld;

    [ObservableProperty]
    private bool _isAccelerating;

    [ObservableProperty]
    private bool _isDecelerating;

    [ObservableProperty]
    private bool _isLaunched;

    public MainWindowViewModel()
    {
        _state = new SimulationState();
        _engine = new SimulationEngine(_state);
        _stopwatch = Stopwatch.StartNew();
        _startDateTime = DateTime.Now;

        // Initialize destinations
        InitializeDestinations();
    }

    private void InitializeDestinations()
    {
        Destinations = new ObservableCollection<Destination>
        {
            // Terrestrial
            new Destination { Name = "California (Los Angeles)", DistanceMiles = 2_800, Category = "Earth" },
            new Destination { Name = "Paris, France", DistanceMiles = 3_628, Category = "Earth" },
            new Destination { Name = "North Pole", DistanceMiles = 3_360, Category = "Earth" },
            new Destination { Name = "South Pole", DistanceMiles = 9_445, Category = "Earth" },
            new Destination { Name = "Sydney, Australia", DistanceMiles = 9_950, Category = "Earth" },
            
            // Solar System
            new Destination { Name = "The Moon", DistanceMiles = 238_855, Category = "Space" },
            new Destination { Name = "Mercury", DistanceMiles = 56_000_000, Category = "Space" },
            new Destination { Name = "The Sun", DistanceMiles = 93_000_000, Category = "Space" },
            new Destination { Name = "Mars", DistanceMiles = 140_000_000, Category = "Space" },
            new Destination { Name = "Saturn", DistanceMiles = 886_000_000, Category = "Space" },
            new Destination { Name = "Voyager 1", DistanceMiles = 15_000_000_000, Category = "Space" },
            
            // Deep Space
            new Destination { Name = "Horseshoe Nebula", DistanceMiles = 5_500 * PhysicsConstants.LightYearMiles, Category = "Deep Space" },
            new Destination { Name = "Andromeda Galaxy", DistanceMiles = 2_537_000 * PhysicsConstants.LightYearMiles, Category = "Deep Space" }
        };

        // Start with no destination selected
        SelectedDestination = null;
    }

    // Simulation update properties
    [ObservableProperty]
    private double _speedMph;

    [ObservableProperty]
    private double _percentageOfLightSpeed;

    [ObservableProperty]
    private double _lorentzFactor;

    [ObservableProperty]
    private double _shipClockRate;

    [ObservableProperty]
    private double _distanceMiles;

    [ObservableProperty]
    private double _distanceLightYears;

    [ObservableProperty]
    private double _remainingMiles;

    [ObservableProperty]
    private double _remainingLightYears;

    [ObservableProperty]
    private string _earthTimeElapsed = "00:00:00";

    [ObservableProperty]
    private string _shipTimeElapsed = "00:00:00";

    [ObservableProperty]
    private string _earthDateTime = "";

    [ObservableProperty]
    private string _shipDateTime = "";

    [ObservableProperty]
    private bool _destinationReached;

    [ObservableProperty]
    private double _targetDistanceLightYears = PhysicsConstants.TargetDistanceLightYears;

    // Journey configuration
    [ObservableProperty]
    private string _startingLocation = "New York, USA";

    public ObservableCollection<Destination> Destinations { get; private set; } = new();

    [ObservableProperty]
    private Destination? _selectedDestination;

    [ObservableProperty]
    private double _journeyProgressPercentage; // 0.0 to 100.0 for display

    [ObservableProperty]
    private string _estimatedTimeOfArrival = "N/A";

    [ObservableProperty]
    private string _timeDifference = "0s";

    [ObservableProperty]
    private string _journeySummary = "";

    partial void OnSelectedDestinationChanged(Destination? value)
    {
        if (value != null)
        {
            // Reset simulation when destination changes
            ResetSimulation(value.DistanceMiles);
        }
    }

    private double CalculateAccelerationRate(bool increaseHeld, bool decreaseHeld, bool slowHeld)
    {
        double netDeltaRate = 0.0;

        if (increaseHeld)
        {
            netDeltaRate += 1.0 * System.Math.Exp(PhysicsConstants.ExponentialGrowthRatePerSecond * _state.XHeldSeconds);
        }

        if (decreaseHeld)
        {
            netDeltaRate -= 1.0 * System.Math.Exp(PhysicsConstants.ExponentialGrowthRatePerSecond * _state.WHeldSeconds);
        }

        if (slowHeld && netDeltaRate != 0.0)
        {
            netDeltaRate *= PhysicsConstants.SlowModeModifier;
        }

        return netDeltaRate;
    }

    private void ResetSimulation(double newTargetMiles)
    {
        // Reset launch state (but NOT destination - that stays selected)
        IsLaunched = false;
        
        // Reset state
        _state.SpeedMph = 0;
        _state.DistanceMiles = 0;
        _state.EarthTimeSeconds = 0;
        _state.ShipTimeSeconds = 0;
        _state.XHeldSeconds = 0;
        _state.WHeldSeconds = 0;

        // Update target distance in state
        _state.UpdateTargetDistance(newTargetMiles);
        
        // Update display value
        double newTargetLightYears = newTargetMiles / PhysicsConstants.LightYearMiles;
        TargetDistanceLightYears = newTargetLightYears;

        // Reset stopwatch and start time
        _stopwatch.Restart();
        _lastElapsedSeconds = 0;
        _startDateTime = DateTime.Now;
    }

    // Keyboard input methods
    public void SetIncreaseHeld(bool held)
    {
        _increaseHeld = held;
        IsAccelerating = held;
    }

    public void SetDecreaseHeld(bool held)
    {
        _decreaseHeld = held;
        IsDecelerating = held;
    }

    public void SetSlowHeld(bool held) => _slowHeld = held;

    public void Launch()
    {
        // If speed is zero, set to 1 mph so the simulation can progress
        if (_state.SpeedMph <= 0)
        {
            _state.SpeedMph = 1.0;
        }
        
        IsLaunched = true;
    }

    public void Reset()
    {
        IsLaunched = false;
        
        // Reset state
        _state.SpeedMph = 0;
        _state.DistanceMiles = 0;
        _state.EarthTimeSeconds = 0;
        _state.ShipTimeSeconds = 0;
        _state.XHeldSeconds = 0;
        _state.WHeldSeconds = 0;

        // Clear destination selection
        SelectedDestination = null;

        // Reset stopwatch and start time
        _stopwatch.Restart();
        _lastElapsedSeconds = 0;
        _startDateTime = DateTime.Now;
    }

    /// <summary>
    /// Updates the simulation state based on a journey progress percentage (scrubbing).
    /// Recalculates distance and times based on current speed.
    /// </summary>
    /// <param name="percentage">Journey progress percentage (0-100).</param>
    public void UpdateFromDragPosition(double percentage)
    {
        // Clamp percentage to valid range
        percentage = System.Math.Clamp(percentage, 0.0, 100.0);

        // Calculate new distance based on percentage
        double newDistance = (percentage / 100.0) * _state.TargetDistanceMiles;
        _state.DistanceMiles = newDistance;

        // If speed is greater than 0, recalculate times based on distance traveled at current speed
        if (_state.SpeedMph > 0)
        {
            // Calculate Earth time: time = distance / speed
            double hoursElapsed = newDistance / _state.SpeedMph;
            double newEarthTimeSeconds = hoursElapsed * 3600.0;
            _state.EarthTimeSeconds = newEarthTimeSeconds;

            // Calculate Ship time with time dilation at current speed
            double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);
            double newShipTimeSeconds = newEarthTimeSeconds / lorentzFactor;
            _state.ShipTimeSeconds = newShipTimeSeconds;
        }
        // If speed is 0, don't change times (can't calculate time for zero velocity)
    }

    // Main update loop
    public void Update()
    {
        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        double deltaSeconds = elapsedSeconds - _lastElapsedSeconds;
        _lastElapsedSeconds = elapsedSeconds;

        if (deltaSeconds <= 0)
            return;

        // Before launch: only update speed, not distance or time
        if (!IsLaunched)
        {
            // Update key hold durations for acceleration
            _state.XHeldSeconds = _increaseHeld ? _state.XHeldSeconds + deltaSeconds : 0.0;
            _state.WHeldSeconds = _decreaseHeld ? _state.WHeldSeconds + deltaSeconds : 0.0;

            // Calculate and update speed
            double netDeltaRate = CalculateAccelerationRate(_increaseHeld, _decreaseHeld, _slowHeld);
            _state.SpeedMph += netDeltaRate * deltaSeconds;
            
            // Ensure speed doesn't go negative
            if (_state.SpeedMph < 0)
                _state.SpeedMph = 0;
            
            // Keep distance and times at zero
            _state.DistanceMiles = 0;
            _state.EarthTimeSeconds = 0;
            _state.ShipTimeSeconds = 0;
        }
        // After launch: run full simulation
        else if (!_state.DestinationReached)
        {
            // Update simulation
            _engine.Update(deltaSeconds, _increaseHeld, _decreaseHeld, _slowHeld);
        }

        // Calculate display values
        double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);

        // Update properties
        SpeedMph = _state.SpeedMph;
        PercentageOfLightSpeed = RelativisticPhysics.CalculatePercentageOfLightSpeed(_state.SpeedMph);
        LorentzFactor = lorentzFactor;
        ShipClockRate = RelativisticPhysics.CalculateShipClockRate(lorentzFactor);
        DistanceMiles = _state.DistanceMiles;
        DistanceLightYears = RelativisticPhysics.MilesToLightYears(_state.DistanceMiles);
        RemainingMiles = _state.RemainingDistanceMiles;
        RemainingLightYears = RelativisticPhysics.MilesToLightYears(_state.RemainingDistanceMiles);
        EarthTimeElapsed = FormatDuration(_state.EarthTimeSeconds);
        ShipTimeElapsed = FormatDuration(_state.ShipTimeSeconds);
        EarthDateTime = _startDateTime.AddSeconds(_state.EarthTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        ShipDateTime = _startDateTime.AddSeconds(_state.ShipTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        DestinationReached = _state.DestinationReached;
        
        // Calculate journey progress (0-100%)
        JourneyProgressPercentage = _state.TargetDistanceMiles > 0 
            ? (_state.DistanceMiles / _state.TargetDistanceMiles) * 100.0 
            : 0.0;
        if (JourneyProgressPercentage > 100.0) 
            JourneyProgressPercentage = 100.0;

        // Calculate ETA
        EstimatedTimeOfArrival = CalculateETA();

        // Calculate time difference
        TimeDifference = CalculateTimeDifference();

        // Update journey summary
        JourneySummary = GenerateJourneySummary();
    }

    private string CalculateTimeDifference()
    {
        double diffSeconds = _state.EarthTimeSeconds - _state.ShipTimeSeconds;
        
        if (diffSeconds < 0.000001)
            return "0ms";

        // Format based on magnitude
        if (diffSeconds >= 365.25 * 24 * 3600) // Years
        {
            double years = diffSeconds / (365.25 * 24 * 3600);
            return $"{years:F2}y";
        }
        else if (diffSeconds >= 24 * 3600) // Days
        {
            double days = diffSeconds / (24 * 3600);
            return $"{days:F2}d";
        }
        else if (diffSeconds >= 3600) // Hours
        {
            double hours = diffSeconds / 3600;
            return $"{hours:F2}h";
        }
        else if (diffSeconds >= 60) // Minutes
        {
            double minutes = diffSeconds / 60;
            return $"{minutes:F2}m";
        }
        else if (diffSeconds >= 1) // Seconds
        {
            return $"{diffSeconds:F3}s";
        }
        else // Milliseconds
        {
            double milliseconds = diffSeconds * 1000;
            return $"{milliseconds:F2}ms";
        }
    }

    private string CalculateETA()
    {
        if (_state.SpeedMph <= 0 || _state.RemainingDistanceMiles <= 0)
            return "N/A";

        // Calculate remaining time in hours
        double remainingHours = _state.RemainingDistanceMiles / _state.SpeedMph;
        double remainingSeconds = remainingHours * 3600;

        // Convert to time units
        int years = (int)(remainingSeconds / (365.25 * 24 * 3600));
        remainingSeconds -= years * (365.25 * 24 * 3600);

        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds -= days * (24 * 3600);

        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds -= hours * 3600;

        int minutes = (int)(remainingSeconds / 60);

        // Format based on magnitude
        if (years > 0)
        {
            return days > 0 ? $"{years}y {days}d" : $"{years}y";
        }
        else if (days > 0)
        {
            return hours > 0 ? $"{days}d {hours}h" : $"{days}d";
        }
        else if (hours > 0)
        {
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
        else
        {
            return $"{minutes}m";
        }
    }

    private string GenerateJourneySummary()
    {
        // Handle no destination selected
        if (SelectedDestination == null)
        {
            return "Select a destination and adjust your speed using W (accelerate) and X (decelerate). Press Q when ready to launch.";
        }

        string destinationName = SelectedDestination.Name;
        string speedText = $"{SpeedMph:N0} mph";
        string percentLight = $"{PercentageOfLightSpeed:F6}%";

        if (!IsLaunched)
        {
            // Pre-launch summary
            return $"Prepare to travel to {destinationName}. Current speed: {speedText} ({percentLight} the speed of light).";
        }
        else
        {
            // In-flight summary
            string etaText = EstimatedTimeOfArrival != "N/A" ? $"You will arrive in {EstimatedTimeOfArrival}." : "Arrival time unknown.";
            string timeDiffText = TimeDifference != "0s" && TimeDifference != "0ms" 
                ? $"Your ship's clock is {TimeDifference} slower than Earth time." 
                : "No time dilation yet.";

            if (DestinationReached)
            {
                return $"You have arrived at {destinationName}! Final speed: {speedText} ({percentLight} the speed of light). {timeDiffText}";
            }
            else
            {
                return $"You are travelling to {destinationName} at {speedText} ({percentLight} the speed of light). {etaText} {timeDiffText}";
            }
        }
    }

    private static string FormatDuration(double totalSeconds)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds))
            return "N/A";

        if (totalSeconds < 0)
            return "00:00:00";

        // Calculate time units
        int years = (int)(totalSeconds / (365.25 * 24 * 3600));
        double remainingSeconds = totalSeconds - (years * 365.25 * 24 * 3600);

        int months = (int)(remainingSeconds / (30.44 * 24 * 3600)); // Average month length
        remainingSeconds -= months * (30.44 * 24 * 3600);

        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds -= days * (24 * 3600);

        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds -= hours * 3600;

        int minutes = (int)(remainingSeconds / 60);
        remainingSeconds -= minutes * 60;

        int seconds = (int)remainingSeconds;

        // Format based on magnitude - show the two most significant units
        if (years > 0)
        {
            if (months > 0)
                return $"{years}y {months}mo {days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
            else if (days > 0)
                return $"{years}y {days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
            else
                return $"{years}y {hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else if (months > 0)
        {
            if (days > 0)
                return $"{months}mo {days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
            else
                return $"{months}mo {hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else if (days > 0)
        {
            return $"{days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
