using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Kg.Velocity.Api.Services;

/// <summary>
/// Creates OpenAI chat clients using the API key from configuration (OpenAI:ApiKey, e.g. user secrets).
/// </summary>
public class OpenAIChatClientFactory(IConfiguration configuration)
{
    public ApiKeyCredential Credential { get; } = new ApiKeyCredential(
        configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured"));

    /// <summary>Creates a chat client for the specified model, centralizing API key management.</summary>
    public ChatClient CreateChatClient(string model)
    {
        var client = new OpenAIClient(Credential);
        return client.GetChatClient(model);
    }
}
