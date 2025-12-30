using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Kg.Velocity.Api.Services;

public class GroqChatClientFactory(IConfiguration configuration)
{
    public string ApiKey { get; } = configuration["Groq:ApiKey"]
        ?? throw new InvalidOperationException("Groq:ApiKey not configured");

    public ApiKeyCredential Credential { get; } = new ApiKeyCredential(
        configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey not configured"));

    public OpenAIClientOptions Options { get; } = new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.groq.com/openai/v1")
    };

    public ChatClient CreateChatClient(string model)
    {
        var client = new OpenAIClient(Credential, Options);
        return client.GetChatClient(model);
    }
}


