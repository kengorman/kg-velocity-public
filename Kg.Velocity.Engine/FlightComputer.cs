using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

public enum DurationFormat
{
    Verbose,
    Compact
}

public static class FlightComputer
{
    public static string FormatDuration(double totalSeconds, DurationFormat format = DurationFormat.Verbose)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds))
            return "N/A";

        if (totalSeconds < 0)
            return format == DurationFormat.Compact ? "0m" : "00:00:00";

        int years = (int)(totalSeconds / (365.25 * 24 * 3600));
        double remainingSeconds = totalSeconds - (years * 365.25 * 24 * 3600);

        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds -= days * (24 * 3600);

        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds -= hours * 3600;

        int minutes = (int)(remainingSeconds / 60);
        remainingSeconds -= minutes * 60;

        int seconds = (int)remainingSeconds;

        if (format == DurationFormat.Compact)
        {
            // Compact format: "2y 5d", "3d 4h", "5h 30m", "15m", "15m 30s", "30s"
            if (totalSeconds < 1)
                return "0m";
            
            if (years > 0)
                return days > 0 ? $"{years}y {days}d" : $"{years}y";
            else if (days > 0)
                return hours > 0 ? $"{days}d {hours}h" : $"{days}d";
            else if (hours > 0)
                return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
            else if (minutes > 0)
                return seconds > 0 ? $"{minutes}m {seconds}s" : $"{minutes}m";
            else
                return $"{seconds}s";
        }
        else
        {
            // Verbose format: "2y 153d 04:15:30"
            if (years > 0)
            {
                return $"{years}y {days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
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

    public static string CalculateTimeDifference(double earthTimeSeconds, double shipTimeSeconds)
    {
        double diffSeconds = earthTimeSeconds - shipTimeSeconds;
        
        if (diffSeconds < 0.000001)
            return "0ms";

        if (diffSeconds < 1)
        {
            double milliseconds = diffSeconds * 1000;
            return $"{milliseconds:F2}ms";
        }

        // Reuse FormatDuration for consistent output
        return FormatDuration(diffSeconds, DurationFormat.Compact);
    }

    public static string CalculateETA(double speedMph, double remainingDistanceMiles)
    {
        if (speedMph <= 0 || remainingDistanceMiles <= 0)
            return "N/A";

        double remainingHours = remainingDistanceMiles / speedMph;
        double remainingSeconds = remainingHours * 3600;

        // Reuse FormatDuration for consistent output
        return FormatDuration(remainingSeconds, DurationFormat.Compact);
    }

    public static double CalculateJourneyProgress(double currentDistanceMiles, double targetDistanceMiles)
    {
        if (targetDistanceMiles <= 0)
            return 0.0;

        double progress = (currentDistanceMiles / targetDistanceMiles) * 100.0;
        return System.Math.Min(progress, 100.0);
    }

    public static (double SpeedMph, double PercentLight) CalculateAverageSpeed(double distanceMiles, double earthTimeSeconds)
    {
        if (earthTimeSeconds <= 0)
            return (0.0, 0.0);

        double averageSpeedMph = distanceMiles / (earthTimeSeconds / 3600.0);
        double averageSpeedPercentLight = (averageSpeedMph / PhysicsConstants.SpeedOfLightMph) * 100.0;

        return (averageSpeedMph, averageSpeedPercentLight);
    }
}

