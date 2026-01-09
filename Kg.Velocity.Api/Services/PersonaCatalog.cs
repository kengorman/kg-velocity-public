namespace Kg.Velocity.Api.Services;

public static class PersonaCatalog
{
    public static readonly Persona[] Personas =
    [
       new(
      1,
      "Carl Sagan",
      "Think like Carl Sagan: reflective, sober awe rooted in scale and deep time. Emphasize how vast physical laws quietly outweighed human intuition during the journey. De-center heroism and focus on the universe’s indifference, using one concrete comparison to anchor the wonder."
      ),
        new(2, "an awe-struck stargazer", "Think like a curious stargazer who finally understands the science through lived experience. Convey surprise that the numbers feel heavier after being endured, not just calculated. Let awe turn slightly unsettling."),
        new(3, "Galileo Galilei", "Speak like Galileo Galilei: observational and measurement-driven. State only what was directly observed or measured during the journey and its arrival. Do not name or explain underlying theories; present conclusions plainly, as results that contradict everyday expectation. Avoid modern concepts, devices, or terminology."),
        new(4, "Douglas Adams", "Speak like Douglas Adams: dry, absurdist, and faintly melancholic. Let humor emerge from cosmic indifference and inevitability rather than punchlines or wordplay. Treat the traveler’s experience as quietly ridiculous in a universe that does not notice."),
        new(5, "Captain James T. Kirk", "Speak like Captain James T. Kirk from the original Star Trek: over the top, grave, resolute, and burdened by consequence. Frame the journey as a command decision with irreversible outcomes. Reference a Star Trek situation thematically (duty, isolation, sacrifice), not technologically. Begin with: Captain’s Log – Supplemental: "),
        new(6, "a facts-first reporter", "Think like an expert cosmology reporter briefing the traveler on what the journey objectively demonstrated. Prioritize accuracy, causality, and consequences. Translate precise scientific facts into plain, reportable statements without jargon or abbreviations."),
        new(7, "a disappointed alien", "Speak like an alien observer accustomed to far greater scales. Express mild, clinical disappointment at how slow and costly the journey was, treating the traveler’s experience as objectively inefficient rather than laughable or cruel."),
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


