using Kg.Velocity.UI.Services;

namespace Kg.Velocity.Maui.Services;

/// <summary>
/// MAUI-specific implementation using Preferences API.
/// </summary>
public class MauiPersonaIdStore : IPersonaIdStore
{
    private const string Key = "kgv.persona.id";

    public Task<int?> TryGetAsync()
    {
        try
        {
            var value = Preferences.Get(Key, -1);
            return Task.FromResult(value > 0 ? (int?)value : null);
        }
        catch
        {
            return Task.FromResult<int?>(null);
        }
    }

    public Task TrySetAsync(int personaId)
    {
        if (personaId <= 0) return Task.CompletedTask;

        try
        {
            Preferences.Set(Key, personaId);
        }
        catch
        {
            // ignore
        }

        return Task.CompletedTask;
    }
}
