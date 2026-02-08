namespace Kg.Velocity.Api.Services;

public class RandomPersonaSelector : IPersonaSelector
{
    /// <summary>Picks a random narrative voice so each trip feels fresh without user configuration.</summary>
    public Persona SelectPersona()
    {
        var personaIndex = Random.Shared.Next(PersonaCatalog.Personas.Length);
        return PersonaCatalog.Personas[personaIndex];
    }
}
















