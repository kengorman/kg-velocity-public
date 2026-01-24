namespace Kg.Velocity.UI.Services;

/// <summary>
/// Abstraction for storing persona ID across sessions.
/// Web uses localStorage, MAUI uses Preferences.
/// </summary>
public interface IPersonaIdStore
{
    Task<int?> TryGetAsync();
    Task TrySetAsync(int personaId);
}
