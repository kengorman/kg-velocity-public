using Kg.Velocity.Contracts.Trips;
using static System.Math;

namespace Kg.Velocity.Engine;

/// <summary>
/// Turns a trip's distance, speed, and ship time into seven "journey weights"
/// (emotion, distance, awe, time gone by, memories, patience, loneliness) that add up to 100%.
/// </summary>
/// <remarks>
/// I designed what this class measures; the formulas and numbers are hand-tuned guesses, not science,
/// worked out with help from Google, Claude, and ChatGPT.
/// </remarks>
public static class JourneyWeightCalculator
{

    /// <summary>
    /// Computes perceptual journey weights by comparing how long the universe waits versus how long the traveler experiences.
    /// These weights guide the tone of the AI-generated travel log (travel-log.md). The trip summary
    /// does not use them; it uses <see cref="JourneyInsightClassifier"/> instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The core insight: relativistic and FTL travel create a desynchronization between "outside time" (what the
    /// universe experiences) and "proper time" (what the traveler lives through). This function maps that
    /// desynchronization into psychological dimensions like awe, loneliness, and patience.
    /// </para>
    /// <para>
    /// All calculations happen in log-space because human perception of time and distance is logarithmic—the
    /// difference between 1 year and 10 years feels similar to the difference between 10 years and 100 years.
    /// </para>
    /// <para>
    /// For FTL travel (shipTimeHours ≤ 0), the traveler experiences essentially zero time, but we impose a
    /// distance-scaled floor on outside time so that even instant travel to Andromeda feels different from
    /// instant travel to the Moon.
    /// </para>
    /// </remarks>
    /// <param name="distanceMiles">Physical separation (sets scale regime and awe).</param>
    /// <param name="speedMph">Reference speed used to estimate outside time.</param>
    /// <param name="shipTimeHours">Time experienced by the traveler (may differ from outside time for relativistic/FTL travel).</param>
    /// <param name="random">Optional small nudge (0-1) to a few of the weights. Defaults to 0.5, and no caller
    /// currently passes it, so the same trip always gets the same weights.</param>
    public static JourneyWeights Compute(
        double distanceMiles,
        double speedMph,
        double shipTimeHours,
        double random = 0.5)
    {
        // -----------------------------
        // Safety block
        // -----------------------------
        distanceMiles = Max(1, distanceMiles);
        speedMph = Max(1, speedMph);
        random = Clamp(random, 0, 1);

        // -----------------------------
        // Determines how time is supposed to behave
        // -----------------------------
        bool isFtl = shipTimeHours <= 0;

        // -----------------------------
        // Outside time: how long the universe waits
        // -----------------------------

        // Travel time as seen from Earth: distance ÷ speed, in hours
        double tOutside = distanceMiles / speedMph;

        // -----------------------------
        // Distance scale
        // -----------------------------
        double Ld = Log10(distanceMiles);

        // -----------------------------
        // Boundary between local scale and astronomical scale
        // 10⁷ miles = inner solar system scale
        // Below → planetary / local
        // Above → true astronomical distances
        // -----------------------------
        const double AstronomicalDistancePivot = 7.0;

        // -----------------------------
        // How fast the outside-time floor grows with distance scale
        // -----------------------------
        const double TimeFloorGrowthRate = 1.2;

        // -----------------------------
        // FTL outside-time floor based on distance class
        // -----------------------------
        double outsideFloorHours =
            Pow(10, Max(-2, (Ld - AstronomicalDistancePivot) * TimeFloorGrowthRate));
        // Ld ≤ 5.3 -> 0.01 h (36 seconds), about the Moon's distance and closer
        // Ld = 6  -> ~4 minutes
        // Ld = 7  -> 1 hour
        // Ld = 8  -> ~16 hours
        // Ld = 9  -> ~10 days
        // Ld = 11 -> ~7 years
        // Ld = 12 -> ~114 years

        if (isFtl)
        {
            // Outside time never collapses to zero for FTL
            tOutside = Max(tOutside, outsideFloorHours);
        }

        // Traveler proper time
        double tTraveler;

        if (isFtl)
        {
            // Essentially zero lived time, but finite for logs
            tTraveler = 1e-6;   // ~0.0036 seconds
        }
        else
        {
            tTraveler = Max(1e-6, shipTimeHours);
        }

        // -----------------------------
        // Log-space perceptual drivers
        // -----------------------------

        double Lt = Log10(tOutside);     // outside time (log hours)
        double Lτ = Log10(tTraveler);    // lived time   (log hours)
        double Lv = Log10(speedMph);

        // Desynchronization magnitude
        double D = Lt - Lτ;

        // Stability clamp (prevents domination at absurd scales)
        D = Min(12, D);

        // -----------------------------
        // Secondary perceptual modifiers
        // -----------------------------

        // Endurance / waiting
        double slowness = Ld - 0.5 * Lv;

        // Fast + relativistic = compressed, disorienting
        double compression = Lv - 0.3 * Ld + 0.7 * D;

        // Isolation driver: separation + time + desync
        double isolation =
              0.35 * Ld
            + 0.40 * Lt
            + 0.80 * D
            - 0.15 * Lv
            + 0.20 * random;

        // -----------------------------
        // Psychological dimensions
        // -----------------------------

        // Emotion: intensity + speed + disorientation + noise
        double emotion =
              0.35 * compression
            + 0.25 * Lv
            + 0.25 * D
            + 0.40 * random;

        // Perceived scale
        double perceivedDistance =
              0.90 * Ld
            + 0.10 * random;

        // Awe: scale + deep time + relativistic weirdness
        double awe =
              0.40 * Ld
            + 0.30 * Lt
            + 0.30 * D;

        // Time gone by (outside universe)
        double timeGoneBy =
              0.85 * Lt
            + 0.25 * D;

        // Memories: formed only from lived time
        double memories =
              0.60 * Lτ
            + 0.20 * Lt
            + 0.20 * random;

        // Patience: endurance minus compression
        double patience =
              0.65 * Lτ
            + 0.25 * slowness
            - 0.40 * compression;

        // Loneliness / isolation
        double loneliness = isolation;

        // -----------------------------
        // Clamp negatives
        // -----------------------------

        var raw = new[]
        {
            Max(0, emotion),
            Max(0, perceivedDistance),
            Max(0, awe),
            Max(0, timeGoneBy),
            Max(0, memories),
            Max(0, patience),
            Max(0, loneliness),
        };

        // -----------------------------
        // Normalize to 100%
        // -----------------------------

        var sum = raw.Sum();

        if (sum == 0)
            return new JourneyWeights(0, 0, 0, 0, 0, 0, 0);

        return new JourneyWeights(
            Emotion: raw[0] / sum * 100,
            Distance: raw[1] / sum * 100,
            Awe: raw[2] / sum * 100,
            TimeGoneBy: raw[3] / sum * 100,
            Memories: raw[4] / sum * 100,
            Patience: raw[5] / sum * 100,
            Loneliness: raw[6] / sum * 100
        );
    }
}
