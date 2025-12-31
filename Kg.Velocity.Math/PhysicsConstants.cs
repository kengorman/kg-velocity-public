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
    /// Default: Saturn - approximately 0.00015 light years (average distance ~886 million miles).
    /// Note: Saturn's distance varies from ~746 million miles (closest) to ~1.03 billion miles (farthest).
    /// </summary>
    public const double TargetDistanceLightYears = 0.00015;
}

