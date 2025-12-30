namespace Kg.Velocity.Api.Services;

public class RandomPersonaSelector : IPersonaSelector
{
    public Persona SelectPersona()
    {
        var personaIndex = Random.Shared.Next(PersonaCatalog.Personas.Length);
        return PersonaCatalog.Personas[personaIndex];
    }
}


