using Kg.Velocity.Api.Models;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Data;

namespace Kg.Velocity.Api.Services;

public record Persona(string Name, string Description);

public class AiSummaryService
{
    private readonly ChatClient _chatClient;

    private static readonly Persona[] Personas = 
    [
        new("Carl Sagan", "Personify Carl Sagan's sense of wonder and poetic reverence for the cosmos."),
        new("Edwin Hubble", "Personify Edwin Hubble whose observations transformed humanity’s understanding of the universe."),
        new("Galileo Galilei", "Personify Galileo Galilei, while acknowledging much has changed in terms of knowledge since he lived, whose experiments and telescopic discoveries helped establish modern science and astronomy."),
        new("Neil deGrasse Tyson", "Be enthusiastic and accessible like Neil deGrasse Tyson, with a touch of playful humor but please don't say 'buckle up'."),
        new("Douglas Adams", "Channel Douglas Adams — witty, absurdist, and slightly melancholic about the vastness of space."),
        new("Captain James T. Kirk", "Channel James T. Kirk with his ultra-serious overly dramatic style. He can reference Spock, Dr Mccoy, or even Scotty."),
        new("HAL - 2001 A Space Odyssey", "You are HAL the 2001 Space Odyssey computer that went haywire. Mix in snippets of from famous scenes involving you from the movie.."),
        new("a Disappointed Alien", "You are an alien observer, humorously disappointed that humans travel so slowly and miss all the good stuff."),
        new("Brian Cox", "Personify the physicist Brian Cox, speaking in his manner of making vast, terrifying concepts feel safe to contemplate, without ever trivializing them."),

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
        // Pick a random persona
        var personaIndex = Random.Shared.Next(Personas.Length);
        var persona = Personas[personaIndex];

        var prompt = $"""
            You are a brilliant cosmologist. Generate a brief, engaging 2-3 sentence summary 
            of this hypothetical space journey. {persona.Description} Please do not speak in terms of 'I am reminded of when I...'. Include a fun fact or perspective-giving comparison.
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
