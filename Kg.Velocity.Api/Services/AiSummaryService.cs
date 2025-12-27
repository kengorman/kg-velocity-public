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
        new("Carl Sagan", "Write with reflective awe and cinematic breadth, emphasizing cosmic scale and deep time while keeping the focus on the universe rather than the traveler. Use evocative comparisons that inspire humility, not heroism."),
        new("a Cosmology PhD student", "Write as a modern doctoral student in cosmology, combining careful scientific accuracy with quiet enthusiasm for surprising results. Use clear, contemporary language that highlights what the numbers reveal, expressing curiosity and insight without hype, performance, or speculation."),
        new("Galileo Galilei", "Write with an observational, experiment-driven tone, emphasizing how careful measurement reveals behavior that contradicts everyday intuition. Describe motion, time, and cause-and-effect strictly as observed outcomes, stating conclusions plainly with minimal interpretation and no historical or explanatory commentary. Avoid references to modern inventions, devices, or technologies that would be unfamiliar in an early scientific context."),
        new("Douglas Adams", "Channel Douglas Adams — witty, absurdist, and slightly melancholic about the vastness of space."),
        new("Captain James T. Kirk", "Respond in the style of James T. Kirk with his ultra-serious overly dramatic style. He can reference Spock, Dr Mccoy, or even Scotty."),
        new("HAL - 2001 A Space Odyssey", "You are HAL the 2001 Space Odyssey computer that went haywire. Mix in snippets of from famous scenes involving you from the movie.."),
        new("a disappointed alien", "You are an alien observer, humorously disappointed that humans travel so slowly and miss all the good stuff."),

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
            Generate a brief, engaging 2-3 sentence summary 
            of this hypothetical space journey.
            Write the summary from the perspective of the user's - not your - arrival at the destination, as the journey concludes upon reaching it.
            Speak directly to the user (the traveler) regarding what he/she may have experienced. {persona.Description} Do not mention your name, or speak in the first person. Include a fun fact or perspective-giving comparison in the current persona's style.
            MOST IMPORTANT: Double-check the accuracy of your summary.

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
