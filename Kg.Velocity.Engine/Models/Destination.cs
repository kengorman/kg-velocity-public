namespace Kg.Velocity.Engine.Models;

/// <summary>
/// Represents a travel destination with its distance from New York, USA.
/// </summary>
public class Destination
{
    public string Name { get; set; } = string.Empty;
    public double DistanceMiles { get; set; }
    public string Category { get; set; } = string.Empty;

    public string DisplayName => $"{Name} ({FormatDistance()})";

    private string FormatDistance()
    {
        if (DistanceMiles < 1_000)
            return $"{DistanceMiles:N0} mi";
        else if (DistanceMiles < 1_000_000)
            return $"{DistanceMiles / 1_000:N1}k mi";
        else if (DistanceMiles < 1_000_000_000)
            return $"{DistanceMiles / 1_000_000:N1}M mi";
        else
            return $"{DistanceMiles / 1_000_000_000:N2}B mi";
    }
}

