namespace Kg.Velocity.Engine.Models;

/// <summary>
/// A named travel speed the user can pick, from walking pace to faster-than-light.
/// </summary>
/// <param name="Name">Display label, e.g. "99% Light Speed".</param>
/// <param name="SpeedMph">Speed in miles per hour. Values above
/// <see cref="Kg.Velocity.Math.PhysicsConstants.SpeedOfLightMph"/> are faster than light.</param>
/// <param name="Group">Category label for grouping related presets, e.g. "— Light Speeds —".
/// Includes the decorative dashes, so it can be displayed as-is.</param>
public sealed record SpeedPreset(string Name, double SpeedMph, string Group);
