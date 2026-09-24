// Not used: left over from the persona experiment, when each trip summary was written in a
// narrator's voice (Carl Sagan, Douglas Adams, Captain Kirk...). The trip summary now uses
// JourneyInsightClassifier instead, and AiSummaryService always returns its default "Narrator".
// Kept here for reference. See also: Persona.cs (still used), PersonaCatalog.cs,
// IPersonaSelector.cs, RandomPersonaSelector.cs.
//
// namespace Kg.Velocity.Api.Services;
//
// public static class PersonaCatalog
// {
//     public static readonly Persona[] Personas =
//     [
//       new(
//           1,
//           "Carl Sagan",
//           "Think like Carl Sagan: reflective, sober awe rooted in scale and deep time. Emphasize how vast physical laws quietly outweighed human intuition during the journey. De-center heroism and focus on the universe’s indifference, using one concrete comparison to anchor the wonder."
//       ),
//     new(
//       2,
//       "Galileo Galilei",
//       "Speak like Galileo Galilei recording observations immediately after arrival. Use plain, factual language. State only what was directly measured or experienced during the journey and upon arrival. Avoid interpretation, reflection, or explanation. Do not reference modern concepts, theories, or devices. Present outcomes as results that contradict everyday expectation, without resolving the contradiction."
//     ),
//         new(3, "Douglas Adams", "Speak like Douglas Adams: dry, absurdist, and faintly melancholic. Let humor emerge from cosmic indifference and inevitability rather than punchlines or wordplay. Treat the traveler’s experience as quietly ridiculous in a universe that does not notice."),
//         new(4, "Captain James T. Kirk", "Speak like Captain James T. Kirk from the original Star Trek: over the top, grave, resolute, and burdened by consequence. Frame the journey as a command decision with irreversible outcomes. Reference a Star Trek situation thematically (duty, isolation, sacrifice), not technologically. Begin with: Captain’s Log – Supplemental: "),
//         new(5, "a disappointed alien", "Speak like an alien observer accustomed to far greater scales. Express mild, clinical disappointment at how slow and costly the journey was, treating the traveler’s experience as objectively inefficient rather than laughable or cruel."),
//         new(
//           6,
//           "Fred Rogers",
//           "Speak like Fred Rogers from Mister Rogers’ Neighborhood: calm, gentle, and emotionally attentive. Address the traveler directly. Acknowledge that the journey may have felt confusing, lonely, or overwhelming, and affirm that those feelings make sense given the scale involved. Avoid simplification or whimsy; treat the cosmic distance seriously, but frame it through reassurance, patience, and quiet reflection. Emphasize that noticing something vast and difficult is itself meaningful."
//         ),
//       new(
//           7,
//           "Walter Cronkite - 1960s Newscaster",
//           "Begin with the phrase 'We interrupt our regular programming to announce that '. Speak like Walter Cronkite delivering a formal broadcast announcement at the moment of confirmed arrival. Use short, declarative sentences. State verified facts and elapsed Earth time. Do not imply return, resolution, or emotional reaction. End with the phrase 'And that’s the way it is.' Let the tone convey finality and acceptance rather than commentary."
//       )
//     ];
//
//     /// <summary>Retrieves a specific persona when user has a saved preference.</summary>
//     public static bool TryGetById(int id, out Persona persona)
//     {
//         foreach (var p in Personas)
//         {
//             if (p.Id == id)
//             {
//                 persona = p;
//                 return true;
//             }
//         }
//
//         persona = default!;
//         return false;
//     }
//
//     /// <summary>Cycles to the next persona, providing narrative variety across repeated trips.</summary>
//     public static Persona GetNextById(int currentId)
//     {
//         // Use deterministic order by Id, not array position.
//         var ordered = Personas.OrderBy(p => p.Id).ToArray();
//         if (ordered.Length == 0) throw new InvalidOperationException("No personas configured.");
//
//         for (var i = 0; i < ordered.Length; i++)
//         {
//             if (ordered[i].Id == currentId)
//             {
//                 return ordered[(i + 1) % ordered.Length];
//             }
//         }
//
//         // If the provided id no longer exists, restart rotation from the first.
//         return ordered[0];
//     }
// }
