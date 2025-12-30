using Kg.Velocity.Api.Models;
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

    public async Task<(string Summary, TimelineResponse timeline, string PersonaName)> GenerateSummaryAsync(TripEvaluationRequest request)
    {
        var persona = request.PersonaId is int personaId && PersonaCatalog.TryGetById(personaId, out var requestedPersona)
            ? requestedPersona
            : _personaSelector.SelectPersona();
        var prompt = _tripSummaryPromptBuilder.BuildPrompt(request, persona);

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
            var timeline = new TimelineResponse();
            return (summary, timeline, persona.Name);
        }
        catch (Exception ex)
        {
            return ($"AI unavailable: {ex.Message}",new TimelineResponse(), "System");
        }
    }
}
