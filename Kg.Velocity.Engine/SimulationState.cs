using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

/// <summary>
/// Represents the current state of the spaceship simulation.
/// </summary>
public class SimulationState
{
    /// <summary>
    /// Current speed in miles per hour.
    /// </summary>
    public double SpeedMph { get; set; }

    /// <summary>
    /// Total distance traveled in miles.
    /// </summary>
    public double DistanceMiles { get; set; }

    /// <summary>
    /// Elapsed time on Earth in seconds.
    /// </summary>
    public double EarthTimeSeconds { get; set; }

    /// <summary>
    /// Elapsed time on the ship in seconds (proper time).
    /// </summary>
    public double ShipTimeSeconds { get; set; }

    /// <summary>
    /// Target distance in miles.
    /// </summary>
    public double TargetDistanceMiles { get; set; }

    public SimulationState()
    {
        TargetDistanceMiles = PhysicsConstants.TargetDistanceLightYears * PhysicsConstants.LightYearMiles;
    }
    
    /// <summary>
    /// Updates the target distance for a new destination.
    /// </summary>
    /// <param name="newTargetMiles">New target distance in miles.</param>
    public void UpdateTargetDistance(double newTargetMiles)
    {
        TargetDistanceMiles = newTargetMiles;
    }
}

