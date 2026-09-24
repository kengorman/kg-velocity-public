using System.Text.Json;
using Kg.Velocity.Contracts.Trips;
using OpenAI.Chat;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Asks the AI model for the first-person travel log entries (prompt: Prompts/travel-log.md).
/// Falls back to a fixed set of entries if the AI call fails or its reply can't be read.
/// </summary>
public class AiTravelLogService
{
    private readonly ChatClient _chatClient;
    private readonly ModelClientFactory _modelClientFactory;
    private readonly TravelLogPromptBuilder _promptBuilder;
    private readonly ILogger<AiTravelLogService> _logger;

    public AiTravelLogService(
        ChatClient chatClient,
        ModelClientFactory modelClientFactory,
        TravelLogPromptBuilder promptBuilder,
        ILogger<AiTravelLogService> logger)
    {
        _chatClient = chatClient;
        _modelClientFactory = modelClientFactory;
        _promptBuilder = promptBuilder;
        _logger = logger;
    }

    /// <summary>Generates journal-style log entries via LLM, creating an immersive first-person narrative of the journey.</summary>
    public async Task<List<TravelLogEntry>> GenerateEntriesAsync(TripComputationResult trip)
    {
        var prompt = _promptBuilder.BuildPrompt(trip);

        try
        {
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatOptions = _modelClientFactory.CreateOptions(temperature: 0.9f);

            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            var responseText = completion.Value.Content[0].Text;

            return ParseResponse(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI travel log generation failed, using fallback");
            return GetFallbackEntries();
        }
    }

    private List<TravelLogEntry> ParseResponse(string responseText)
    {
        try
        {
            // The model sometimes wraps its JSON in a ```json ... ``` block; strip it (same as the poster events).
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

            var response = JsonSerializer.Deserialize<TravelLogResponse>(json, options);

            if (response?.Events == null || response.Events.Count == 0)
            {
                _logger.LogWarning("AI returned empty travel log, using fallback");
                return GetFallbackEntries();
            }

            return response.Events
                .Where(e => !string.IsNullOrWhiteSpace(e.Text))
                .Select(e => new TravelLogEntry { Text = e.Text })
                .ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse travel log JSON: {Response}", responseText);
            return GetFallbackEntries();
        }
    }

    private static List<TravelLogEntry> GetFallbackEntries()
    {
        return new List<TravelLogEntry>
        {
            new() { Text = "Ship systems nominal. The void stretches endlessly ahead." },
            new() { Text = "Time passes differently here. Earth feels like a distant memory." },
            new() { Text = "Stars shift colors as we approach relativistic speeds." },
            new() { Text = "The silence of space is both peaceful and unnerving." },
            new() { Text = "Destination growing closer. A new world awaits." }
        };
    }

    private class TravelLogResponse
    {
        public List<EventItem> Events { get; set; } = new();
    }

    private class EventItem
    {
        public string Text { get; set; } = string.Empty;
    }
}
