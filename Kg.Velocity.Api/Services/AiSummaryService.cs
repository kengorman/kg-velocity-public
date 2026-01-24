using Kg.Velocity.Contracts.Trips;
using OpenAI.Chat;

namespace Kg.Velocity.Api.Services;

public class AiSummaryService
{
    private readonly ChatClient _chatClient;
    private readonly TripSummaryPromptBuilder _tripSummaryPromptBuilder;

    // Default persona returned when personas are bypassed
    private static readonly Persona DefaultPersona = new(0, "Narrator", "");

    public AiSummaryService(
        ChatClient chatClient,
        TripSummaryPromptBuilder tripSummaryPromptBuilder)
    {
        _chatClient = chatClient;
        _tripSummaryPromptBuilder = tripSummaryPromptBuilder;
    }

    public async Task<(string Summary, Persona Persona)> GenerateSummaryAsync(
        TripEvaluateRequest request,
        TripComputationResult trip)
    {
        var prompt = _tripSummaryPromptBuilder.BuildPrompt(trip);

        try
        {
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = 1.1f
            };

            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            var summary = completion.Value.Content[0].Text;
            return (summary, DefaultPersona);
        }
        catch (Exception ex)
        {
            return ($"AI unavailable: {ex.Message}", DefaultPersona);
        }
    }
}
