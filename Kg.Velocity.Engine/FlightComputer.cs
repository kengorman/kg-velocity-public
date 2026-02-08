using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

public enum DurationFormat
{
    Verbose,
    Compact
}

public static class FlightComputer
{
    /// <summary>Converts raw seconds to human-readable format, scaling from seconds up to quadrillions of years for astronomical journeys.</summary>
    public static string FormatDuration(double totalSeconds, DurationFormat format = DurationFormat.Verbose)
    {
        if (double.IsInfinity(totalSeconds) || double.IsNaN(totalSeconds))
            return "N/A";

        if (totalSeconds < 0)
            return format == DurationFormat.Compact ? "0m" : "00:00:00";

        long years = (long)(totalSeconds / (365.25 * 24 * 3600));
        
        // Handle extremely long durations with simplified formatting
        if (years >= 1_000_000_000_000_000) // Quadrillion+
        {
            return $"{(years / 1_000_000_000_000_000.0):N2} quadrillion years";
        }
        else if (years >= 1_000_000_000_000) // Trillion+
        {
            return $"{(years / 1_000_000_000_000.0):N2} trillion years";
        }
        else if (years >= 1_000_000_000) // Billion+
        {
            return $"{(years / 1_000_000_000.0):N2} billion years";
        }
        else if (years >= 1_000_000) // Million+
        {
            return $"{(years / 1_000_000.0):N2} million years";
        }
        
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
                return days > 0 ? $"{years:N0}y {days}d" : $"{years:N0}y";
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
                return $"{years:N0}y {days}d {hours:D2}:{minutes:D2}:{seconds:D2}";
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

    /// <summary>Displays the time dilation effect - how much more time passed on Earth than aboard the ship.</summary>
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

    /// <summary>Projects arrival date from travel time, switching to "Year X Million" format when beyond DateTime limits.</summary>
    public static string FormatDateTime(DateTime baseDate, double secondsToAdd)
    {
        try 
        {
            double yearsToAdd = secondsToAdd / (365.2425 * 24 * 3600);
            int currentYear = baseDate.Year;
            
            if (currentYear + yearsToAdd > 9999)
            {
                // Deep Time formatting
                double targetYear = currentYear + yearsToAdd;
                
                if (targetYear >= 1_000_000)
                {
                    return $"Year {(targetYear / 1_000_000):N2} Million";
                }
                else
                {
                    return $"Year {targetYear:N0}";
                }
            }
            else
            {
                DateTime resultDate = baseDate.AddSeconds(secondsToAdd);
                return resultDate.ToString("MM/dd/yyyy h:mm:ss tt");
            }
        }
        catch
        {
            return "Far Future";
        }
    }
}

