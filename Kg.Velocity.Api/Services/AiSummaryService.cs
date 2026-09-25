using Kg.Velocity.Contracts.Trips;
using OpenAI.Chat;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Asks the AI model for the short written trip summary (prompt: Prompts/trip-summary.md).
/// If the AI call fails, returns an "AI unavailable" message instead of throwing.
/// </summary>
public class AiSummaryService
{
    private readonly ChatClient _chatClient;
    private readonly ModelClientFactory _modelClientFactory;
    private readonly TripSummaryPromptBuilder _tripSummaryPromptBuilder;
    private readonly ILogger<AiSummaryService> _logger;

    // Default persona returned when personas are bypassed
    private static readonly Persona DefaultPersona = new(0, "Narrator", "");

    public AiSummaryService(
        ChatClient chatClient,
        ModelClientFactory modelClientFactory,
        TripSummaryPromptBuilder tripSummaryPromptBuilder,
        ILogger<AiSummaryService> logger)
    {
        _chatClient = chatClient;
        _modelClientFactory = modelClientFactory;
        _tripSummaryPromptBuilder = tripSummaryPromptBuilder;
        _logger = logger;
    }

    /// <summary>Generates a narrative summary of the trip via LLM, making raw physics results emotionally engaging.</summary>
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

            var chatOptions = _modelClientFactory.CreateOptions(temperature: 1.1f);

            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            var summary = completion.Value.Content[0].Text;
            return (summary, DefaultPersona);
        }
        catch (Exception ex)
        {
            // Keep error details in the server log; users only see a plain message.
            _logger.LogError(ex, "AI summary generation failed");
            return ("AI unavailable. Please try again in a moment.", DefaultPersona);
        }
    }
}
