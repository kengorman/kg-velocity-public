namespace Kg.Velocity.Engine;

public static class JourneyInsightClassifier
{
    private const double SecondsPerYear = 365.25 * 24 * 3600;

    /// <summary>Identifies the most compelling aspect of a trip (speed, dilation, scale, etc.) to guide AI-generated narrative focus.</summary>
    public static string Classify(
        double earthTimeSeconds,
        double shipTimeSeconds,
        double speedMph,
        double lorentzFactor)
    {
        double earthYears = earthTimeSeconds / SecondsPerYear;
        double shipYears = shipTimeSeconds / SecondsPerYear;
        double dilationRatio = shipTimeSeconds > 0
            ? earthTimeSeconds / shipTimeSeconds
            : 1;

        // Score each dimension (0-100)
        var scores = new Dictionary<string, double>
        {
            ["speed"] = ScoreSpeed(earthTimeSeconds),
            ["duration"] = ScoreDuration(earthYears, speedMph),
            ["dilation"] = ScoreDilation(dilationRatio, lorentzFactor),
            ["scale"] = ScoreScale(earthYears),
            ["farewell"] = ScoreFarewell(earthYears, shipYears)
        };

        var ranked = scores
            .OrderByDescending(kv => kv.Value)
            .ToList();

        var primary = ranked[0];
        var secondary = ranked[1];

        // If nothing scores high, generic fallback
        if (primary.Value < 10)
            return "journey";

        // Include secondary if it's strong enough on its own
        // and at least half as important as primary
        const double secondaryThreshold = 40;

        if (secondary.Value >= secondaryThreshold
            && secondary.Value >= primary.Value * 0.5)
        {
            return $"{primary.Key} and {secondary.Key}";
        }

        return primary.Key;
    }

    /// <summary>
    /// Returns all scores for debugging/testing purposes.
    /// </summary>
    public static Dictionary<string, double> GetScores(
        double earthTimeSeconds,
        double shipTimeSeconds,
        double speedMph,
        double lorentzFactor)
    {
        double earthYears = earthTimeSeconds / SecondsPerYear;
        double shipYears = shipTimeSeconds / SecondsPerYear;
        double dilationRatio = shipTimeSeconds > 0
            ? earthTimeSeconds / shipTimeSeconds
            : 1;

        return new Dictionary<string, double>
        {
            ["speed"] = ScoreSpeed(earthTimeSeconds),
            ["duration"] = ScoreDuration(earthYears, speedMph),
            ["dilation"] = ScoreDilation(dilationRatio, lorentzFactor),
            ["scale"] = ScoreScale(earthYears),
            ["farewell"] = ScoreFarewell(earthYears, shipYears)
        };
    }

    private static double ScoreSpeed(double earthTimeSeconds)
    {
        // Near-instant arrival is the story
        if (earthTimeSeconds < 1) return 100;
        if (earthTimeSeconds < 10) return 80;
        if (earthTimeSeconds < 60) return 50;
        if (earthTimeSeconds < 3600) return 20;
        return 0;
    }

    private static double ScoreDuration(double earthYears, double speedMph)
    {
        // Only scores if speed is human-relatable (walking to airplane)
        if (speedMph > 600) return 0;

        if (earthYears > 1000) return 100;
        if (earthYears > 100) return 80;
        if (earthYears > 10) return 60;
        if (earthYears > 1) return 40;
        return 0;
    }

    private static double ScoreDilation(double dilationRatio, double lorentzFactor)
    {
        // Only meaningful for sub-light (γ >= 1)
        if (lorentzFactor < 1.001) return 0;

        if (dilationRatio > 1000) return 100;
        if (dilationRatio > 100) return 80;
        if (dilationRatio > 10) return 60;
        if (dilationRatio > 2) return 40;
        return 0;
    }

    private static double ScoreScale(double earthYears)
    {
        // Time beyond human comprehension
        if (earthYears > 1e12) return 100;  // trillions
        if (earthYears > 1e9) return 80;    // billions
        if (earthYears > 1e6) return 60;    // millions
        if (earthYears > 10_000) return 40; // historical
        return 0;
    }

    private static double ScoreFarewell(double earthYears, double shipYears)
    {
        // The journey is the goodbye — everyone you left is gone by arrival
        double maxYears = System.Math.Max(earthYears, shipYears);

        if (maxYears > 1000) return 100;   // civilizations
        if (maxYears > 200) return 80;     // multiple generations
        if (maxYears > 80) return 60;      // a lifetime
        if (maxYears > 40) return 30;      // half a lifetime
        return 0;
    }
}
