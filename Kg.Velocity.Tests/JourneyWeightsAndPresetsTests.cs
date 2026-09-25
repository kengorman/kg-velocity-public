using Kg.Velocity.Engine;
using Kg.Velocity.Math;

namespace Kg.Velocity.Tests;

public class JourneyWeightsAndPresetsTests
{
    [Theory]
    [InlineData(238_855, 3, 79_618)]        // Moon, walking
    [InlineData(5.878625e12, 1e10, 0)]      // a light-year, faster than light
    public void Weights_AddUpTo100(double distanceMiles, double speedMph, double shipTimeHours)
    {
        var w = JourneyWeightCalculator.Compute(distanceMiles, speedMph, shipTimeHours);

        var total = w.Emotion + w.Distance + w.Awe + w.TimeGoneBy + w.Memories + w.Patience + w.Loneliness;
        Assert.Equal(100.0, total, 6);
    }

    [Fact]
    public void Weights_SameTrip_SameWeights()
    {
        Assert.Equal(
            JourneyWeightCalculator.Compute(238_855, 3, 79_618),
            JourneyWeightCalculator.Compute(238_855, 3, 79_618));
    }

    [Fact]
    public void SpeedPresets_AreSlowestFirst_WithUniqueNames()
    {
        var speeds = SpeedPresets.All.Select(p => p.SpeedMph).ToList();

        Assert.Equal(speeds.OrderBy(s => s), speeds);
        Assert.Equal(SpeedPresets.All.Count, SpeedPresets.All.Select(p => p.Name).Distinct().Count());
    }

    [Fact]
    public void SpeedPresets_GoFromWalkingTo1000xLight()
    {
        Assert.Equal(3, SpeedPresets.All.First().SpeedMph);
        Assert.Equal(PhysicsConstants.SpeedOfLightMph * 1000, SpeedPresets.All.Last().SpeedMph);
    }
}
