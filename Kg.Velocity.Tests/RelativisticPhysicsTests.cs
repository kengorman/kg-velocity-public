using Kg.Velocity.Math;

namespace Kg.Velocity.Tests;

public class RelativisticPhysicsTests
{
    private const double C = PhysicsConstants.SpeedOfLightMph;

    [Theory]
    [InlineData(0.5, 1.1547)]
    [InlineData(0.9, 2.2942)]
    [InlineData(0.99, 7.0888)]
    public void LorentzFactor_MatchesKnownValues(double fractionOfLight, double expected)
    {
        Assert.Equal(expected, RelativisticPhysics.CalculateLorentzFactor(C * fractionOfLight), 4);
    }

    [Fact]
    public void LorentzFactor_IsAboutOne_AtEverydaySpeeds()
    {
        // Even a jet is so slow next to light that the effect is invisible
        Assert.Equal(1.0, RelativisticPhysics.CalculateLorentzFactor(570), 10);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(1000.0)]
    public void LorentzFactor_IsInfinite_AtOrAboveLightSpeed(double fractionOfLight)
    {
        Assert.True(double.IsPositiveInfinity(RelativisticPhysics.CalculateLorentzFactor(C * fractionOfLight)));
    }

    [Fact]
    public void ShipClockRate_IsOneOverGamma()
    {
        Assert.Equal(0.5, RelativisticPhysics.CalculateShipClockRate(2.0));
    }

    [Fact]
    public void ShipClockRate_IsZero_WhenGammaIsInfinite()
    {
        Assert.Equal(0.0, RelativisticPhysics.CalculateShipClockRate(double.PositiveInfinity));
    }

    [Fact]
    public void PercentageOfLightSpeed_HalfLight_Is50()
    {
        Assert.Equal(50.0, RelativisticPhysics.CalculatePercentageOfLightSpeed(C / 2), 10);
    }

    [Fact]
    public void MilesToLightYears_OneLightYear_IsOne()
    {
        Assert.Equal(1.0, RelativisticPhysics.MilesToLightYears(PhysicsConstants.LightYearMiles), 10);
    }
}
