using System.Text.Json;
using Microsoft.JSInterop;

namespace Kg.Velocity.Blazor.Services;

public class PersonaRotationService(IJSRuntime js)
{
    // Must match server-side PersonaCatalog IDs.
    private static readonly int[] PersonaIds = [1, 2, 3, 4, 5, 6, 7];

    private const string BagKey = "kgv.persona.bag";
    private const string IndexKey = "kgv.persona.idx";

    public async Task<int?> TryGetNextPersonaIdAsync()
    {
        try
        {
            var bagJson = await js.InvokeAsync<string?>("localStorage.getItem", BagKey);
            var idxString = await js.InvokeAsync<string?>("localStorage.getItem", IndexKey);

            var bag = TryParseBag(bagJson) ?? CreateShuffledBag();
            var idx = TryParseIndex(idxString);

            if (idx >= bag.Count)
            {
                bag = CreateShuffledBag();
                idx = 0;
            }

            var personaId = bag[idx];
            idx++;

            await js.InvokeAsync<object?>("localStorage.setItem", BagKey, JsonSerializer.Serialize(bag));
            await js.InvokeAsync<object?>("localStorage.setItem", IndexKey, idx.ToString());

            return personaId;
        }
        catch
        {
            // localStorage unavailable (privacy mode, disabled storage, etc.)
            return null;
        }
    }

    private static List<int>? TryParseBag(string? bagJson)
    {
        if (string.IsNullOrWhiteSpace(bagJson)) return null;

        try
        {
            var bag = JsonSerializer.Deserialize<List<int>>(bagJson);
            if (bag is null || bag.Count == 0) return null;

            // Basic validation: bag should contain only known ids.
            foreach (var id in bag)
            {
                if (Array.IndexOf(PersonaIds, id) < 0)
                    return null;
            }

            return bag;
        }
        catch
        {
            return null;
        }
    }

    private static int TryParseIndex(string? idxString)
    {
        if (int.TryParse(idxString, out var idx) && idx >= 0)
            return idx;
        return 0;
    }

    private static List<int> CreateShuffledBag()
    {
        var bag = PersonaIds.ToList();

        // Fisher–Yates shuffle
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        return bag;
    }
}


