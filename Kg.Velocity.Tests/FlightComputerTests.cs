using System.Globalization;
using Kg.Velocity.Engine;

namespace Kg.Velocity.Tests;

public class FlightComputerTests
{
    private const double SecondsPerYear = 365.25 * 24 * 3600;

    public FlightComputerTests()
    {
        // The app shows US-style numbers and dates; don't let the test machine's settings change that
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
    }

    [Theory]
    [InlineData(5, "00:00:05")]
    [InlineData(3661, "01:01:01")]
    [InlineData(90061, "1d 01:01:01")]
    public void FormatDuration_Verbose(double seconds, string expected)
    {
        Assert.Equal(expected, FlightComputer.FormatDuration(seconds));
    }

    [Theory]
    [InlineData(0.5, "0m")]
    [InlineData(30, "30s")]
    [InlineData(930, "15m 30s")]
    [InlineData(90061, "1d 1h")]
    public void FormatDuration_Compact(double seconds, string expected)
    {
        Assert.Equal(expected, FlightComputer.FormatDuration(seconds, DurationFormat.Compact));
    }

    [Fact]
    public void FormatDuration_MillionsOfYears_UsesShortForm()
    {
        Assert.Equal("2.54 million years", FlightComputer.FormatDuration(2_540_000 * SecondsPerYear));
    }

    [Fact]
    public void FormatDuration_BadValues()
    {
        Assert.Equal("N/A", FlightComputer.FormatDuration(double.PositiveInfinity));
        Assert.Equal("N/A", FlightComputer.FormatDuration(double.NaN));
        Assert.Equal("00:00:00", FlightComputer.FormatDuration(-5));
    }

    [Theory]
    [InlineData(10, 10, "0ms")]
    [InlineData(1.0005, 1.0, "0.50ms")]
    [InlineData(5 * 3600, 0, "5h")]
    public void CalculateTimeDifference(double earthSeconds, double shipSeconds, string expected)
    {
        Assert.Equal(expected, FlightComputer.CalculateTimeDifference(earthSeconds, shipSeconds));
    }

    [Fact]
    public void FormatDateTime_AddsTime()
    {
        var start = new DateTime(2026, 1, 1);

        Assert.Equal("01/01/2026 12:00:00 AM", FlightComputer.FormatDateTime(start, 0));
        Assert.Equal("01/02/2026 12:00:00 AM", FlightComputer.FormatDateTime(start, 24 * 3600));
    }

    [Fact]
    public void FormatDateTime_PastYear9999_ShowsJustTheYear()
    {
        var start = new DateTime(2026, 1, 1);
        const double calendarYear = 365.2425 * 24 * 3600; // FormatDateTime uses this year length

        Assert.Equal("Year 12,026", FlightComputer.FormatDateTime(start, 10_000 * calendarYear));
        Assert.Equal("Year 5.00 Million", FlightComputer.FormatDateTime(start, 5_000_000 * calendarYear));
    }
}
