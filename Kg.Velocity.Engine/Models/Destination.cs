namespace Kg.Velocity.Engine.Models;

/// <summary>
/// A place the user can travel to, from the Moon to distant galaxies.
/// </summary>
/// <param name="Name">Name of the destination, e.g. "Andromeda Galaxy".</param>
/// <param name="DistanceMiles">Approximate distance in miles; typical or average values, not exact positions.</param>
/// <param name="Category">Region the destination belongs to, e.g. "Solar System" or "Milky Way".</param>
/// <param name="Tagline">Short descriptive phrase shown under the name, e.g. "The Red Planet".</param>
public sealed record Destination(string Name, double DistanceMiles, string Category, string Tagline)
{
    /// <summary>
    /// Name shown to the user in the destination list and the trip animation. Currently the same as <see cref="Name"/>;
    /// kept as a separate hook so a destination can later have a friendlier display label without changing its name.
    /// </summary>
    public string DisplayName => Name;
}
