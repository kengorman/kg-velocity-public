namespace Kg.Velocity.Math;

/// <summary>
/// Core simulation engine that updates the spaceship state based on inputs and elapsed time.
/// </summary>
public class SimulationEngine
{
    private readonly SimulationState _state;

    public SimulationEngine(SimulationState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// Updates the simulation state based on user inputs and elapsed time.
    /// </summary>
    /// <param name="deltaSeconds">Time elapsed since last update in seconds.</param>
    /// <param name="increaseHeld">Whether acceleration key is held.</param>
    /// <param name="decreaseHeld">Whether deceleration key is held.</param>
    /// <param name="slowHeld">Whether slow modifier key is held.</param>
    public void Update(double deltaSeconds, bool increaseHeld, bool decreaseHeld, bool slowHeld)
    {
        if (deltaSeconds <= 0) return;

        // Update key hold durations
        _state.WHeldSeconds = increaseHeld ? _state.WHeldSeconds + deltaSeconds : 0.0;
        _state.XHeldSeconds = decreaseHeld ? _state.XHeldSeconds + deltaSeconds : 0.0;

        // Calculate acceleration/deceleration rate
        double netDeltaRate = CalculateAccelerationRate(increaseHeld, decreaseHeld, slowHeld);

        // Update speed
        _state.SpeedMph += netDeltaRate * deltaSeconds;
        
        // Ensure speed doesn't go negative
        if (_state.SpeedMph < 0)
            _state.SpeedMph = 0;

        // Recalculate times based on current distance and current speed
        // (assuming we've been traveling at this speed the whole time)
        if (_state.SpeedMph > 0 && _state.DistanceMiles > 0)
        {
            // Calculate Earth time: time = distance / speed
            double hoursElapsed = _state.DistanceMiles / _state.SpeedMph;
            _state.EarthTimeSeconds = hoursElapsed * 3600.0;

            // Calculate Ship time with time dilation at current speed
            double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);
            _state.ShipTimeSeconds = _state.EarthTimeSeconds / lorentzFactor;
        }
        else if (_state.SpeedMph <= 0)
        {
            // If speed is zero or negative, set times to zero
            _state.EarthTimeSeconds = 0.0;
            _state.ShipTimeSeconds = 0.0;
        }

        // Update distance traveled
        _state.DistanceMiles += _state.SpeedMph * (deltaSeconds / 3600.0);
        
        // Ensure distance doesn't go negative
        if (_state.DistanceMiles < 0.0)
            _state.DistanceMiles = 0.0;
    }

    /// <summary>
    /// Calculates the current acceleration rate based on key inputs.
    /// </summary>
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
}

