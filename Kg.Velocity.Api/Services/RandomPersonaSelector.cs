// Not used: left over from the persona experiment, when each trip summary was written in a
// narrator's voice (Carl Sagan, Douglas Adams, Captain Kirk...). The trip summary now uses
// JourneyInsightClassifier instead, and AiSummaryService always returns its default "Narrator".
// Kept here for reference. See also: Persona.cs (still used), PersonaCatalog.cs,
// IPersonaSelector.cs, RandomPersonaSelector.cs.
//
// namespace Kg.Velocity.Api.Services;
//
// public class RandomPersonaSelector : IPersonaSelector
// {
//     /// <summary>Picks a random narrative voice so each trip feels fresh without user configuration.</summary>
//     public Persona SelectPersona()
//     {
//         var personaIndex = Random.Shared.Next(PersonaCatalog.Personas.Length);
//         return PersonaCatalog.Personas[personaIndex];
//     }
// }
