namespace Kg.Velocity.Engine.Models;

/// <summary>
/// Represents a travel destination with its distance from New York, USA.
/// </summary>
public class Destination
{
    public string Name { get; set; } = string.Empty;
    public double DistanceMiles { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;

    public string DisplayName => Name;
}

