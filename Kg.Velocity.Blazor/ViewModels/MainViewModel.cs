using Kg.Velocity.Math;
using Kg.Velocity.Shared.Models;
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
        EstimatedTimeOfArrival = "N/A";
        TimeDifference = "0s";
        JourneySummary = "Select a destination\nAdjust the ship's speed using W (accelerate) and X (decelerate).\nPress Q to launch.";
        AverageSpeedMph = 0;
        AverageSpeedPercentLight = 0;
    }

    private void InitializeDestinations()
    {
        Destinations =
        [
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
        ];

        SelectedDestination = null;
    }

    // Properties
    public bool IsAccelerating { get; set; }
    public bool IsDecelerating { get; set; }
    public bool IsLaunched { get; set; }
    public bool ShowNoDestinationWarning { get; set; }
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
    public bool DestinationReached { get; set; }
    public double TargetDistanceLightYears { get; set; } = PhysicsConstants.TargetDistanceLightYears;
    public string StartingLocation { get; set; } = "New York, USA";
    public ObservableCollection<Destination> Destinations { get; set; } = new();
    
    private Destination? _selectedDestination;
    public Destination? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            if (value != null)
            {
                ShowNoDestinationWarning = false;
                ResetSimulation(value.DistanceMiles);
                NotifyStateChanged();
            }
        }
    }
    
    public double JourneyProgressPercentage { get; set; }
    public string EstimatedTimeOfArrival { get; set; } = "N/A";
    public string TimeDifference { get; set; } = "0s";
    public string JourneySummary { get; set; } = "";
    public double AverageSpeedMph { get; set; }
    public double AverageSpeedPercentLight { get; set; }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private double CalculateAccelerationRate(bool increaseHeld, bool decreaseHeld, bool slowHeld)
    {
        double netDeltaRate = 0.0;

        if (increaseHeld)
        {
            netDeltaRate += 1.0 * System.Math.Exp(PhysicsConstants.ExponentialGrowthRatePerSecond * _state.WHeldSeconds);
        }

        if (decreaseHeld)
        {
            netDeltaRate -= 1.0 * System.Math.Exp(PhysicsConstants.ExponentialGrowthRatePerSecond * _state.XHeldSeconds);
        }

        if (slowHeld && netDeltaRate != 0.0)
        {
            netDeltaRate *= PhysicsConstants.SlowModeModifier;
        }

        return netDeltaRate;
    }

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
            ShowNoDestinationWarning = true;
            NotifyStateChanged();
            return;
        }
        
        ShowNoDestinationWarning = false;
        
        if (_state.SpeedMph <= 0)
        {
            _state.SpeedMph = 1.0;
        }
        
        IsLaunched = true;
        
        _timeSinceLastAverageUpdate = 1.0;
        _timeSinceLastTimeDiffUpdate = 1.0;
        
        NotifyStateChanged();
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
        else if (!IsLaunched)
        {
            _state.WHeldSeconds = _increaseHeld ? _state.WHeldSeconds + deltaSeconds : 0.0;
            _state.XHeldSeconds = _decreaseHeld ? _state.XHeldSeconds + deltaSeconds : 0.0;

            double netDeltaRate = CalculateAccelerationRate(_increaseHeld, _decreaseHeld, _slowHeld);
            _state.SpeedMph += netDeltaRate * deltaSeconds;
            
            if (_state.SpeedMph < 0)
                _state.SpeedMph = 0;
            
            _state.DistanceMiles = 0;
            _state.EarthTimeSeconds = 0;
            _state.ShipTimeSeconds = 0;
        }
        else
        {
            _engine.Update(deltaSeconds, _increaseHeld, _decreaseHeld, _slowHeld);
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
        EarthTimeElapsed = FormatDuration(_state.EarthTimeSeconds);
        ShipTimeElapsed = FormatDuration(_state.ShipTimeSeconds);
        EarthDateTime = _startDateTime.AddSeconds(_state.EarthTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        ShipDateTime = _startDateTime.AddSeconds(_state.ShipTimeSeconds).ToString("MM/dd/yyyy HH:mm:ss.fff");
        DestinationReached = _state.DestinationReached;
        
        JourneyProgressPercentage = _state.TargetDistanceMiles > 0 
            ? (_state.DistanceMiles / _state.TargetDistanceMiles) * 100.0 
            : 0.0;
        if (JourneyProgressPercentage > 100.0) 
            JourneyProgressPercentage = 100.0;

        _timeSinceLastAverageUpdate += deltaSeconds;
        if (_timeSinceLastAverageUpdate >= 1.0)
        {
            if (_state.EarthTimeSeconds > 0)
            {
                AverageSpeedMph = _state.DistanceMiles / (_state.EarthTimeSeconds / 3600.0);
                AverageSpeedPercentLight = (AverageSpeedMph / PhysicsConstants.SpeedOfLightMph) * 100.0;
            }
            else
            {
                AverageSpeedMph = 0;
                AverageSpeedPercentLight = 0;
            }
            _timeSinceLastAverageUpdate = 0.0;
        }

        EstimatedTimeOfArrival = CalculateETA();

        _timeSinceLastTimeDiffUpdate += deltaSeconds;
        if (_timeSinceLastTimeDiffUpdate >= 1.0)
        {
            TimeDifference = CalculateTimeDifference();
            _timeSinceLastTimeDiffUpdate = 0.0;
        }

        JourneySummary = GenerateJourneySummary();
        
        NotifyStateChanged();
    }

    private string CalculateTimeDifference()
    {
        double diffSeconds = _state.EarthTimeSeconds - _state.ShipTimeSeconds;
        
        if (diffSeconds < 0.000001)
            return "0ms";

        if (diffSeconds >= 365.25 * 24 * 3600)
        {
            double years = diffSeconds / (365.25 * 24 * 3600);
            return $"{years:F2}y";
        }
        else if (diffSeconds >= 24 * 3600)
        {
            double days = diffSeconds / (24 * 3600);
            return $"{days:F2}d";
        }
        else if (diffSeconds >= 3600)
        {
            double hours = diffSeconds / 3600;
            return $"{hours:F2}h";
        }
        else if (diffSeconds >= 60)
        {
            double minutes = diffSeconds / 60;
            return $"{minutes:F2}m";
        }
        else if (diffSeconds >= 1)
        {
            return $"{diffSeconds:F3}s";
        }
        else
        {
            double milliseconds = diffSeconds * 1000;
            return $"{milliseconds:F2}ms";
        }
    }

    private string CalculateETA()
    {
        if (_state.SpeedMph <= 0 || _state.RemainingDistanceMiles <= 0)
            return "N/A";

        double remainingHours = _state.RemainingDistanceMiles / _state.SpeedMph;
        double remainingSeconds = remainingHours * 3600;

        int years = (int)(remainingSeconds / (365.25 * 24 * 3600));
        remainingSeconds -= years * (365.25 * 24 * 3600);

        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds -= days * (24 * 3600);

        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds -= hours * 3600;

        int minutes = (int)(remainingSeconds / 60);

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
        if (SelectedDestination == null)
        {
            return "Select a destination\nAdjust the ship's speed using W (accelerate) and X (decelerate).\nPress Q to launch.";
        }

        string destinationName = SelectedDestination.Name;
        string speedText = $"{SpeedMph:N0} mph";
        string percentLight = $"{PercentageOfLightSpeed:F6}%";

        if (!IsLaunched)
        {
            return $"Preparing to travel to {destinationName}.\nInitial speed: {speedText} ({percentLight} c).\nPress Q to launch.";
        }
        else
        {
            string etaText = EstimatedTimeOfArrival != "N/A" ? $"Arrival in {EstimatedTimeOfArrival}." : "Arrival time unknown.";
            string timeDiffText = TimeDifference != "0s" && TimeDifference != "0ms" 
                ? $"Your ship's clock is {TimeDifference} slower than Earth time." 
                : "No time dilation.";
            
            string avgSpeedText = AverageSpeedMph > 0 
                ? $"Average speed: {AverageSpeedMph:N0} mph ({AverageSpeedPercentLight:F6}% c)." 
                : "";

            if (DestinationReached)
            {
                var lines = new List<string>
                {
                    $"Arrived at {destinationName}!",
                    $"Final speed: {speedText} ({percentLight} c)."
                };
                
                if (!string.IsNullOrEmpty(avgSpeedText))
                    lines.Add(avgSpeedText);
                    
                lines.Add(timeDiffText);
                lines.Add("Press A to reset.");
                
                return string.Join("\n", lines);
            }
            else
            {
                var lines = new List<string>
                {
                    $"Travelling to {destinationName} at {speedText} ({percentLight} c)."
                };
                
                if (!string.IsNullOrEmpty(avgSpeedText))
                    lines.Add(avgSpeedText);
                    
                lines.Add(etaText);
                lines.Add(timeDiffText);
                
                return string.Join("\n", lines);
            }
        }
    }

    private static string FormatDuration(double totalSeconds)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds))
            return "N/A";

        if (totalSeconds < 0)
            return "00:00:00";

        int years = (int)(totalSeconds / (365.25 * 24 * 3600));
        double remainingSeconds = totalSeconds - (years * 365.25 * 24 * 3600);

        int months = (int)(remainingSeconds / (30.44 * 24 * 3600));
        remainingSeconds -= months * (30.44 * 24 * 3600);

        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds -= days * (24 * 3600);

        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds -= hours * 3600;

        int minutes = (int)(remainingSeconds / 60);
        remainingSeconds -= minutes * 60;

        int seconds = (int)remainingSeconds;

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

