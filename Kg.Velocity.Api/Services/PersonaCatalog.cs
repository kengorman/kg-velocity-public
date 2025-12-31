namespace Kg.Velocity.Api.Services;

public static class PersonaCatalog
{
    public static readonly Persona[] Personas =
    [
        new(1, "Carl Sagan", "Write as Carl Sagan with reflective awe and cinematic breadth, emphasizing cosmic scale and deep time while keeping the focus on the universe rather than the traveler. Use evocative comparisons that inspire humility, not heroism."),
        new(2, "an awe-struck stargazer", "You are a stargazer who is awe-struck at the true science behind the relationship of time to speed. Describe these facts based on the journey."),
        new(3, "Galileo Galilei", "Write as Galileo Galilei with an observational, experiment-driven tone, emphasizing how careful measurement reveals behavior that contradicts everyday intuition. Describe motion, time, and cause-and-effect strictly as observed outcomes, stating conclusions plainly with minimal interpretation and no historical or explanatory commentary. Avoid references to modern inventions, devices, or technologies that would be unfamiliar in an early scientific context."),
        new(4, "Douglas Adams", "Channel Douglas Adams — witty, absurdist, and slightly melancholic about the vastness of space."),
        new(5, "Captain James T. Kirk", "Respond totally as James T. Kirk from the original Star Trek with his ultra-serious overly dramatic style.Add a reference to a situation in a Start Trek episode. Begin with: Captain's Log - Supplemental: "),
        new(6, "a facts-first reporter", "Write as an expert facts-first cosmology science reporter. Use scientific accuracy with a focus on details of the trip. Use deep cosmological terms and concepts."),
        new(7, "a disappointed alien", "You are an alien observer, humorously disappointed that humans travel so slowly and miss all the good stuff."),
    ];

    public static bool TryGetById(int id, out Persona persona)
    {
        foreach (var p in Personas)
        {
            if (p.Id == id)
            {
                persona = p;
                return true;
            }
        }

        persona = default!;
        return false;
    }

    public static Persona GetNextById(int currentId)
    {
        // Use deterministic order by Id, not array position.
        var ordered = Personas.OrderBy(p => p.Id).ToArray();
        if (ordered.Length == 0) throw new InvalidOperationException("No personas configured.");

        for (var i = 0; i < ordered.Length; i++)
        {
            if (ordered[i].Id == currentId)
            {
                return ordered[(i + 1) % ordered.Length];
            }
        }

        // If the provided id no longer exists, restart rotation from the first.
        return ordered[0];
    }
}


