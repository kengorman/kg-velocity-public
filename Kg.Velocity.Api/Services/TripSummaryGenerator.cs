using Kg.Velocity.Api.Models;

namespace Kg.Velocity.Api.Services;

public static class TripSummaryGenerator
{
    public static string Generate(TripEvaluationRequest request)
    {
        var timestamp = DateTime.UtcNow.ToString("HH:mm:ss");
        double earthYears = request.EarthTimeSeconds / (365.25 * 24 * 3600);
        double shipYears = request.ShipTimeSeconds / (365.25 * 24 * 3600);
        double timeSavedYears = earthYears - shipYears;
        string destination = request.Destination;
        string speedName = request.SpeedName;

        string timeDilationNote = GetTimeDilationNote(
            earthYears, shipYears, timeSavedYears,
            request.EarthTimeSeconds, request.ShipTimeSeconds,
            request.EarthTimeFormatted, request.ShipTimeFormatted);

        string mainSummary;
        if (earthYears < 0.0001)
        {
            mainSummary = $"A quick hop to {destination} at {speedName}. You'd barely have time to finish your coffee.";
        }
        else if (earthYears < 0.01)
        {
            double hours = request.EarthTimeSeconds / 3600;
            mainSummary = $"At {speedName}, you'd reach {destination} in about {hours:N0} hours.";
        }
        else if (earthYears < 1)
        {
            double days = request.EarthTimeSeconds / (24 * 3600);
            mainSummary = $"Traveling at {speedName}, {destination} is {days:N0} days away.";
        }
        else if (earthYears < 80)
        {
            mainSummary = $"At {speedName}, reaching {destination} would take {earthYears:N1} years.";
        }
        else if (earthYears < 1000)
        {
            mainSummary = $"At {speedName}, this {earthYears:N0}-year journey means everyone you know would be gone long before you arrive at {destination}.";
        }
        else if (earthYears < 1_000_000)
        {
            mainSummary = $"Traveling to {destination} at {speedName} would take {earthYears:N0} years. Human civilization is only about 10,000 years old.";
        }
        else if (earthYears < 1_000_000_000)
        {
            double millionYears = earthYears / 1_000_000;
            mainSummary = $"At {speedName}, reaching {destination} takes {millionYears:N1} million years. Homo sapiens have only existed for 0.3 million years.";
        }
        else
        {
            double billionYears = earthYears / 1_000_000_000;
            mainSummary = $"This {billionYears:N1} billion year journey to {destination} exceeds the remaining lifespan of our Sun. Earth itself may not exist when you arrive.";
        }

        var summary = !string.IsNullOrEmpty(timeDilationNote)
            ? $"{mainSummary} {timeDilationNote}"
            : mainSummary;
        
        return $"[{timestamp}] {summary}";
    }

    private static string GetTimeDilationNote(
        double earthYears, double shipYears, double timeSavedYears,
        double earthTimeSeconds, double shipTimeSeconds,
        string earthTimeFormatted, string shipTimeFormatted)
    {
        double timeSavedSeconds = earthTimeSeconds - shipTimeSeconds;

        if (timeSavedSeconds < 0.001) return "";

        if (timeSavedSeconds < 1)
        {
            double ms = timeSavedSeconds * 1000;
            return $"Due to time dilation, you'd age {ms:F1}ms less than those on Earth.";
        }
        else if (timeSavedSeconds < 60)
        {
            return $"Due to time dilation, you'd age {timeSavedSeconds:F1} seconds less than those on Earth.";
        }
        else if (timeSavedSeconds < 3600)
        {
            double minutes = timeSavedSeconds / 60;
            return $"Due to time dilation, you'd age {minutes:F1} minutes less than those on Earth.";
        }
        else if (timeSavedSeconds < 86400)
        {
            double hours = timeSavedSeconds / 3600;
            return $"Due to time dilation, you'd age {hours:F1} hours less than those on Earth.";
        }
        else if (timeSavedYears < 1)
        {
            double days = timeSavedSeconds / 86400;
            return $"Due to time dilation, you'd age {days:F0} days less than those on Earth.";
        }
        else if (timeSavedYears < 1000)
        {
            return $"Due to time dilation, you'd age only {shipYears:F1} years while {earthYears:F1} years pass on Earth.";
        }
        else
        {
            return $"Time dilation is extreme: you'd experience {shipTimeFormatted} while Earth ages {earthTimeFormatted}.";
        }
    }
}

