namespace Kg.Velocity.UI.Utilities;

public static class Formatting
{
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
