namespace Kg.Velocity.Math;

/// <summary>
/// Physical constants for relativistic travel simulation.
/// </summary>
public static class PhysicsConstants
{
    /// <summary>
    /// Speed of light in miles per hour.
    /// </summary>
    public const double SpeedOfLightMph = 670_616_629.0;

    /// <summary>
    /// Number of miles in one light year.
    /// </summary>
    public const double LightYearMiles = 5.878625e12;

    /// <summary>
    /// Distance to target destination in light years.
    /// This value can be adjusted to simulate different journey lengths.
    /// Default: 7,100 light years.
    /// </summary>
    public const double TargetDistanceLightYears = 7_100.0;

    /// <summary>
    /// Exponential growth rate for acceleration per second.
    /// ~= ln(2)/2, which doubles speed approximately every 2 seconds when key is held.
    /// </summary>
    public const double ExponentialGrowthRatePerSecond = 0.346573590379;

    /// <summary>
    /// Modifier for slow mode - reduces acceleration rate by this factor.
    /// </summary>
    public const double SlowModeModifier = 0.5;
}

