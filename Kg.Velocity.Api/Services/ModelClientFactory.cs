using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Globalization;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Creates the chat client used for all the AI writing (summary, poster events, mission log).
/// TODO: This factory leans towards supporting OpenAI's models. Update to make a generic approach for using any model.
/// </summary>
/// <remarks>
/// TODO: (in the future) plug in any LLM here. This implementation currently only uses OpenAI's
/// request format: OpenAI itself by default, or any server that accepts the same requests
/// (Ollama, Groq, OpenRouter, Gemini, ...) by setting OpenAI:Endpoint. Other providers would
/// need their own client created here.
///
/// Settings (user secrets, appsettings.json, or environment variables):
/// <list type="bullet">
/// <item>OpenAI:ApiKey - required, unless OpenAI:Endpoint is set (local servers often need no key).
/// It's sent to whichever server is configured, so when you switch servers, switch to that server's key.</item>
/// <item>OpenAI:Endpoint - optional. Leave empty to use OpenAI, e.g. "http://localhost:11434/v1" for Ollama.</item>
/// <item>OpenAI:Model - optional. Defaults to gpt-5.2.</item>
/// <item>OpenAI:MaxTemperature - optional. Caps the temperature, for providers that only allow up to 1.0.</item>
/// </list>
/// </remarks>
public class ModelClientFactory(IConfiguration configuration)
{
    private const string DefaultModel = "gpt-5.2";

    private readonly string? _endpoint = NullIfBlank(configuration["OpenAI:Endpoint"]);
    private readonly string? _apiKey = NullIfBlank(configuration["OpenAI:ApiKey"]);
    private readonly float? _maxTemperature =
        float.TryParse(configuration["OpenAI:MaxTemperature"], NumberStyles.Float, CultureInfo.InvariantCulture, out var max)
            ? max
            : null;

    /// <summary>The model name sent with every request.</summary>
    public string Model { get; } = NullIfBlank(configuration["OpenAI:Model"]) ?? DefaultModel;

    /// <summary>Creates a chat client for the configured server and model.</summary>
    public ChatClient CreateChatClient()
    {
        if (_endpoint is null)
        {
            var key = _apiKey ?? throw new InvalidOperationException("OpenAI:ApiKey not configured");
            return new OpenAIClient(new ApiKeyCredential(key)).GetChatClient(Model);
        }

        // The client library insists on some key, even for local servers that don't check it
        var options = new OpenAIClientOptions { Endpoint = new Uri(_endpoint) };
        return new OpenAIClient(new ApiKeyCredential(_apiKey ?? "none"), options).GetChatClient(Model);
    }

    /// <summary>Request options with the given temperature, lowered to OpenAI:MaxTemperature if that's set.</summary>
    public ChatCompletionOptions CreateOptions(float temperature) => new()
    {
        Temperature = _maxTemperature is { } cap ? System.Math.Min(temperature, cap) : temperature
    };

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
