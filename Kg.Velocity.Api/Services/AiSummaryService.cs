using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Kg.Velocity.Api.Models;

namespace Kg.Velocity.Api.Services;

public record Persona(string Name, string Description);

public class AiSummaryService
{
    private readonly ChatClient _chatClient;

    private static readonly Persona[] Personas = 
    [
        new("Carl Sagan", "Channel Carl Sagan's sense of wonder and poetic reverence for the cosmos."),
        new("Gandalf", "Channel Tolkien's Gandalf... being reverent, serious, wizard-like, fan of all Hobbits."),
        new("Neil deGrasse Tyson", "Be enthusiastic and accessible like Neil deGrasse Tyson, with a touch of playful humor."),
        new("Shakespeare", "Channel Shakespeare's sense of drama and early English prose."),
        new("Douglas Adams", "Channel Douglas Adams — witty, absurdist, and slightly melancholic about the vastness of space."),
  //      new("A Bored Ship's Computer", "You are a bored, slightly passive-aggressive ship's computer who has seen too many journeys."),
        new("HAL - from 2001 A Space Odyssey", "You are HAL the 2001 movie computer that went haywire."),
        new("Movie Trailer Narrator", "Be an overly dramatic movie trailer narrator. Epic. Intense. Every sentence matters."),
        new("A Disappointed Alien", "You are an alien observer, humorously disappointed that humans travel so slowly and miss all the good stuff."),
        new("A Dry British Academic", "Be a dry, understated British academic who finds everything mildly interesting but never exciting."),
    ];

    public AiSummaryService(IConfiguration configuration)
    {
        var apiKey = configuration["Groq:ApiKey"] 
            ?? throw new InvalidOperationException("Groq:ApiKey not configured");
        
        var credential = new ApiKeyCredential(apiKey);
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.groq.com/openai/v1")
        };
        
        var client = new OpenAIClient(credential, options);
        _chatClient = client.GetChatClient("meta-llama/llama-4-scout-17b-16e-instruct");
    }

    public async Task<(string Summary, string PersonaName)> GenerateSummaryAsync(TripEvaluationRequest request)
    {
        // Pick a persona based on current milliseconds
        var personaIndex = DateTime.UtcNow.Millisecond % Personas.Length;
        var persona = Personas[personaIndex];

        var prompt = $"""
            You are a brilliant cosmologist. Generate a brief, engaging 2-3 sentence summary 
            of this hypothetical space journey. {persona.Description} Include a fun fact or perspective-giving comparison.
            Do not use markdown.

            Journey Details:
            - Destination: {request.Destination}
            - Speed: {request.SpeedName} ({request.SpeedMph:N0} mph)
            - Distance: {request.DistanceMiles:N0} miles
            - Earth time elapsed: {request.EarthTimeFormatted}
            - Ship time elapsed (due to time dilation): {request.ShipTimeFormatted}
            - Time saved by traveler: {request.TimeDifference}
            """;

        try
        {
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = 1.0f
            };

            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            var summary = completion.Value.Content[0].Text;
            return (summary, persona.Name);
        }
        catch (Exception ex)
        {
            return ($"AI unavailable: {ex.Message}", "System");
        }
    }
}
