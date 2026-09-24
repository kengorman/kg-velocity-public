using System.Text.Json;
using Kg.Velocity.Contracts.Trips;
using OpenAI.Chat;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Asks the AI model for the short milestone events printed on the trip poster (prompt: Prompts/poster-events.md).
/// Falls back to a fixed set of events if the AI call fails or its reply can't be read.
/// </summary>
public class AiPosterEventsService
{
    private readonly ChatClient _chatClient;
    private readonly PosterEventsPromptBuilder _promptBuilder;
    private readonly ILogger<AiPosterEventsService> _logger;

    public AiPosterEventsService(
        ChatClient chatClient,
        PosterEventsPromptBuilder promptBuilder,
        ILogger<AiPosterEventsService> logger)
    {
        _chatClient = chatClient;
        _promptBuilder = promptBuilder;
        _logger = logger;
    }

    /// <summary>Generates milestone events for the trip poster via LLM, giving the poster narrative depth.</summary>
    public async Task<List<JourneyEvent>> GenerateEventsAsync(
        TripComputationResult trip)
    {
        var prompt = _promptBuilder.BuildPrompt(trip);

        try
        {
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = 0.9f // Balanced creativity with factual accuracy
            };

            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            var responseText = completion.Value.Content[0].Text;

            return ParseEventsResponse(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI poster events generation failed, using fallback");
            return GetFallbackEvents();
        }
    }

    private List<JourneyEvent> ParseEventsResponse(string responseText)
    {
        try
        {
            // The prompt asks for plain JSON, but the model sometimes wraps it in a
            // ```json ... ``` block anyway. Keep just the part from the first { to the last }.
            var json = responseText.Trim();
            if (json.StartsWith("```"))
            {
                var startIndex = json.IndexOf('{');
                var endIndex = json.LastIndexOf('}');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    json = json.Substring(startIndex, endIndex - startIndex + 1);
                }
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<PosterEventsResponse>(json, options);

            if (response?.Events == null || response.Events.Count == 0)
            {
                _logger.LogWarning("AI returned empty or null events, using fallback");
                return GetFallbackEvents();
            }

            return response.Events
                .Where(e => !string.IsNullOrWhiteSpace(e.Text))
                .Select(e => new JourneyEvent
                {
                    Text = e.Text,
                    Description = e.Description
                })
                .ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response as JSON: {Response}", responseText);
            return GetFallbackEvents();
        }
    }

    private static List<JourneyEvent> GetFallbackEvents()
    {
        return new List<JourneyEvent>
        {
            new() { Text = "Departed Earth Orbit", Description = "Left the cradle of humanity" },
            new() { Text = "Crossed the Void", Description = "Endless darkness between stars" },
            new() { Text = "Time Dilation Active", Description = "Clocks ticking at different rates" },
            new() { Text = "Destination Approached", Description = "Journey's end in sight" }
        };
    }

    private class PosterEventsResponse
    {
        public List<EventItem> Events { get; set; } = new();
    }

    private class EventItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

/// <summary>
/// One milestone event on the trip poster: a short headline plus an optional longer description.
/// </summary>
public class JourneyEvent
{
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
}
