namespace Kg.Velocity.Api.Services;

/// <summary>
/// A narrator style for the trip summary. Left over from the persona experiment (see PersonaCatalog.cs);
/// now only AiSummaryService's default "Narrator" is used, and its Id and Name are passed back to the app.
/// </summary>
public record Persona(int Id, string Name, string Description);
