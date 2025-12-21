using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Kg.Velocity.Api.Models;

namespace Kg.Velocity.Api.Services;

public class AiSummaryService
{
    private readonly ChatClient _chatClient;

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
        _chatClient = client.GetChatClient("llama-3.3-70b-versatile");
    }

    public async Task<string> GenerateSummaryAsync(TripEvaluationRequest request)
    {
        var prompt = $"""
            You are a witty science narrator. Generate a brief, engaging 2-3 sentence summary 
            of this hypothetical space journey. Include a fun fact or perspective-giving comparison.
            Be conversational but informative. Do not use markdown.

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
            var completion = await _chatClient.CompleteChatAsync(prompt);
            var summary = completion.Value.Content[0].Text;
            return $"{summary}";
        }
        catch (Exception ex)
        {
            return $"AI unavailable: {ex.Message}";
        }
    }
}

