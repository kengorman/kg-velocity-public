using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Kg.Velocity.Math;

namespace Kg.Velocity.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SimulationState _state;
    private readonly SimulationEngine _engine;
    private readonly Stopwatch _stopwatch;
    private double _lastElapsedSeconds;
    private readonly DateTime _startDateTime;

    // Keyboard state
    private bool _increaseHeld;
    private bool _decreaseHeld;
    private bool _slowHeld;

    public MainWindowViewModel()
    {
        _state = new SimulationState();
        _engine = new SimulationEngine(_state);
        _stopwatch = Stopwatch.StartNew();
        _startDateTime = DateTime.Now;
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
    private string _startingLocation = "Earth";

    [ObservableProperty]
    private string _destinationLocation = "Saturn";

    [ObservableProperty]
    private double _journeyProgressPercentage; // 0.0 to 100.0 for display

    // Keyboard input methods
    public void SetIncreaseHeld(bool held) => _increaseHeld = held;
    public void SetDecreaseHeld(bool held) => _decreaseHeld = held;
    public void SetSlowHeld(bool held) => _slowHeld = held;

    // Main update loop
    public void Update()
    {
        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        double deltaSeconds = elapsedSeconds - _lastElapsedSeconds;
        _lastElapsedSeconds = elapsedSeconds;

        if (deltaSeconds <= 0)
            return;

        // Update simulation
        _engine.Update(deltaSeconds, _increaseHeld, _decreaseHeld, _slowHeld);

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
    }

    private static string FormatDuration(double totalSeconds)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds))
            return "N/A";

        var ts = TimeSpan.FromSeconds(totalSeconds);
        int days = ts.Days;
        return days > 0
            ? $"{days}d {ts:hh\\:mm\\:ss}"
            : ts.ToString("hh\\:mm\\:ss");
    }
}
