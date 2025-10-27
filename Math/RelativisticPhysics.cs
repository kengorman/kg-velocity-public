namespace Kg.Velocity.Math;

/// <summary>
/// Provides calculations for special relativity effects.
/// </summary>
public static class RelativisticPhysics
{
    /// <summary>
    /// Calculates the Lorentz factor (gamma) for a given speed.
    /// γ = 1 / √(1 - v²/c²)
    /// </summary>
    /// <param name="speedMph">Current speed in miles per hour.</param>
    /// <returns>The Lorentz factor. Returns infinity if speed >= speed of light.</returns>
    public static double CalculateLorentzFactor(double speedMph)
    {
        double vOverC = speedMph / PhysicsConstants.SpeedOfLightMph;
        
        if (vOverC >= 1.0)
            return double.PositiveInfinity;
        
        return 1.0 / System.Math.Sqrt(1.0 - vOverC * vOverC);
    }

    /// <summary>
    /// Calculates the speed as a percentage of the speed of light.
    /// </summary>
    /// <param name="speedMph">Current speed in miles per hour.</param>
    /// <returns>Percentage of light speed (0-100+).</returns>
    public static double CalculatePercentageOfLightSpeed(double speedMph)
    {
        return (speedMph / PhysicsConstants.SpeedOfLightMph) * 100.0;
    }

    /// <summary>
    /// Converts miles to light years.
    /// </summary>
    /// <param name="miles">Distance in miles.</param>
    /// <returns>Distance in light years.</returns>
    public static double MilesToLightYears(double miles)
    {
        return miles / PhysicsConstants.LightYearMiles;
    }

    /// <summary>
    /// Calculates the ship's clock rate relative to Earth's clock.
    /// Returns 1/γ, which represents how much slower the ship's clock ticks.
    /// </summary>
    /// <param name="lorentzFactor">The Lorentz factor (gamma).</param>
    /// <returns>Clock rate as a fraction of Earth's clock rate.</returns>
    public static double CalculateShipClockRate(double lorentzFactor)
    {
        if (double.IsInfinity(lorentzFactor))
            return 0.0;
        
        return 1.0 / lorentzFactor;
    }
}

