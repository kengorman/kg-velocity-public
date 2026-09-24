namespace Kg.Velocity.UI.Services;

/// <summary>
/// Remembers which narrator style (persona) was last used, so it survives closing the app.
/// The web app keeps it in the browser's local storage (the Android app used its settings store).
/// Currently has no effect: the API always uses its default narrator (id 0), and 0 is never saved.
/// </summary>
public interface IPersonaIdStore
{
    /// <summary>
    /// Returns the saved narrator id, or null if none is saved or it can't be read.
    /// </summary>
    Task<int?> TryGetAsync();
    /// <summary>
    /// Saves the narrator id for next time. Fails quietly if it can't be saved.
    /// </summary>
    Task TrySetAsync(int personaId);
}
