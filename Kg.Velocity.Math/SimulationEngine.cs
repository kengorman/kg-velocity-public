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
        _state.XHeldSeconds = increaseHeld ? _state.XHeldSeconds + deltaSeconds : 0.0;
        _state.WHeldSeconds = decreaseHeld ? _state.WHeldSeconds + deltaSeconds : 0.0;

        // Calculate acceleration/deceleration rate
        double netDeltaRate = CalculateAccelerationRate(increaseHeld, decreaseHeld, slowHeld);

        // Update speed
        _state.SpeedMph += netDeltaRate * deltaSeconds;

        // Calculate relativistic effects
        double lorentzFactor = RelativisticPhysics.CalculateLorentzFactor(_state.SpeedMph);

        // Update time (Earth time and dilated ship time)
        _state.EarthTimeSeconds += deltaSeconds;
        _state.ShipTimeSeconds += deltaSeconds / lorentzFactor;

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
}

