using Microsoft.JSInterop;

namespace Kg.Velocity.Blazor.Services;

public class PersonaIdStore(IJSRuntime js)
{
    private const string Key = "kgv.persona.id";

    public async Task<int?> TryGetAsync()
    {
        try
        {
            var str = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            return int.TryParse(str, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task TrySetAsync(int personaId)
    {
        if (personaId <= 0) return;

        try
        {
            // localStorage.setItem returns void/undefined; use InvokeVoidAsync to avoid marshal errors.
            await js.InvokeVoidAsync("localStorage.setItem", Key, personaId.ToString());
        }
        catch
        {
            // ignore
        }
    }
}






