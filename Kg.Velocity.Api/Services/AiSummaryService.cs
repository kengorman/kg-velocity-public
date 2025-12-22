using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Kg.Velocity.Api.Models;

namespace Kg.Velocity.Api.Services;

public class AiSummaryService
{
    private readonly ChatClient _chatClient;

    private static readonly string[] Tones = 
    [
        "Be dramatic and awe-inspiring.",
        "Be philosophical and contemplative.",
        "Be humorous with a dry wit.",
        "Be poetic and lyrical.",
        "Be matter-of-fact with a surprising twist at the end.",
        "Channel your inner Carl Sagan.",
        "Be enthusiastic like an excited scientist.",
        "Be humorously disappointed about not passing any alien spaceships.",
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

    public async Task<string> GenerateSummaryAsync(TripEvaluationRequest request)
    {
        // Pick a tone based on current milliseconds
        var toneIndex = DateTime.UtcNow.Millisecond % Tones.Length;
        var tone = Tones[toneIndex];

        var prompt = $"""
            You are a brilliant cosmologist. Generate a brief, engaging 2-3 sentence summary 
            of this hypothetical space journey.  {tone} Include a fun fact or perspective-giving comparison.
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
            return summary;
        }
        catch (Exception ex)
        {
            return $"AI unavailable: {ex.Message}";
        }
    }
}
