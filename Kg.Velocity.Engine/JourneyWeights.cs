using static System.Math;
using Kg.Velocity.Contracts.Trips;

namespace Kg.Velocity.Engine;

public static class JourneyWeightCalculator
{
    public static JourneyWeights Compute(
        double distanceMiles,
        double speedMph,
        double shipTimeHours,
        double random = 0.5)
    {
        // -----------------------------
        // Safety
        // -----------------------------
        distanceMiles = Max(1, distanceMiles);
        speedMph = Max(1, speedMph);
        random = Clamp(random, 0, 1);

        bool isFtl = shipTimeHours <= 0;

        // -----------------------------
        // Time core (this is the soul)
        // -----------------------------

        // Raw outside time from kinematics
        double tOutside = distanceMiles / speedMph;     // hours

        // Distance scale
        double Ld = Log10(distanceMiles);

        // FTL outside-time floor based on distance class
        double outsideFloorHours =
            Pow(10, Max(-2, (Ld - 7) * 1.2));
        // Ld < 7  -> 0.01 h (36 seconds)
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
