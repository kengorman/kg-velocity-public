namespace Kg.Velocity.UI.Utilities;

/// <summary>
/// Turns large numbers into short, readable text for the screen.
/// </summary>
public static class Formatting
{
    /// <summary>
    /// Formats a distance in miles, shortened with k, M, B or T (e.g. "238.9k miles").
    /// Returns a dash if the distance is zero or less.
    /// </summary>
    public static string FormatDistance(double miles)
    {
        if (miles <= 0)
            return "—";
        if (miles < 1_000)
            return $"{miles:N0} miles";
        if (miles < 1_000_000)
            return $"{miles / 1_000:N1}k miles";
        if (miles < 1_000_000_000)
            return $"{miles / 1_000_000:N1}M miles";
        if (miles < 1_000_000_000_000)
            return $"{miles / 1_000_000_000:N2}B miles";
        return $"{miles / 1_000_000_000_000:N2}T miles";
    }

    /// <summary>
    /// Formats a speed in mph, shortened with k, M or B (e.g. "670.6M mph").
    /// </summary>
    public static string FormatSpeed(double mph)
    {
        if (mph < 1_000)
            return $"{mph:N0} mph";
        if (mph < 1_000_000)
            return $"{mph / 1_000:N1}k mph";
        if (mph < 1_000_000_000)
            return $"{mph / 1_000_000:N1}M mph";
        return $"{mph / 1_000_000_000:N2}B mph";
    }
}
