using Kg.Velocity.Contracts.Trips;
using OpenAI.Chat;

namespace Kg.Velocity.Api.Services;

public class AiSummaryService
{
    private readonly ChatClient _chatClient;
    private readonly IPersonaSelector _personaSelector;
    private readonly TripSummaryPromptBuilder _tripSummaryPromptBuilder;

    public AiSummaryService(
        ChatClient chatClient,
        IPersonaSelector personaSelector,
        TripSummaryPromptBuilder tripSummaryPromptBuilder)
    {
        _chatClient = chatClient;
        _personaSelector = personaSelector;
        _tripSummaryPromptBuilder = tripSummaryPromptBuilder;
    }

    public async Task<(string Summary, Persona Persona)> GenerateSummaryAsync(
        TripEvaluateRequest request,
        TripComputationResult trip)
    {
        var persona = request.PersonaId is int personaId
            ? PersonaCatalog.GetNextById(personaId)
            : _personaSelector.SelectPersona();
        var prompt = _tripSummaryPromptBuilder.BuildPrompt(trip, persona);

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
            return (summary, persona);
        }
        catch (Exception ex)
        {
            // Keep a consistent PersonaId even when AI is unavailable:
            // - If caller provided PersonaId, we already advanced deterministically.
            // - Otherwise, we picked a persona via selector above.
            return ($"AI unavailable: {ex.Message}", persona);
        }
    }
}
