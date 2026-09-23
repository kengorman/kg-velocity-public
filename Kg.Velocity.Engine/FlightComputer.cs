using Kg.Velocity.Math;

namespace Kg.Velocity.Engine;

/// <summary>
/// How much detail <see cref="FlightComputer.FormatDuration"/> shows.
/// </summary>
public enum DurationFormat
{
    /// <summary>Full detail down to the second, e.g. "2y 153d 04:15:30".</summary>
    Verbose,

    /// <summary>Just the two largest units, e.g. "3d 4h" or "15m 30s".</summary>
    Compact
}

/// <summary>
/// Turns trip times (in seconds) into readable text: durations, the Earth-vs-ship time gap, and arrival dates.
/// </summary>
public static class FlightComputer
{
    /// <summary>
    /// Converts seconds into readable text, from "00:00:05" up to "2.54 million years" and beyond.
    /// </summary>
    /// <remarks>
    /// Returns "N/A" for infinite or invalid values, and zero ("0m" or "00:00:00") for negative values.
    /// A million years or more always uses the short "X million/billion/trillion/quadrillion years" form.
    /// </remarks>
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

    /// <summary>
    /// Returns how much more time passed on Earth than on the ship, as text, e.g. "3d 4h" or "0.52ms".
    /// </summary>
    /// <remarks>
    /// Gaps under one second are shown in milliseconds; anything under a microsecond is "0ms".
    /// </remarks>
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

    /// <summary>
    /// Adds travel time to a start date and returns the result as text, e.g. "03/14/2027 9:30:00 PM".
    /// </summary>
    /// <remarks>
    /// Dates past the year 9999 (the most <see cref="DateTime"/> can hold) are shown as just a year,
    /// e.g. "Year 12,345", or "Year 2.54 Million" from a million years on.
    /// </remarks>
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

