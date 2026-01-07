using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Kg.Velocity.Api.Services;

public class OpenAIChatClientFactory(IConfiguration configuration)
{
    public ApiKeyCredential Credential { get; } = new ApiKeyCredential(
        configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured"));

    public ChatClient CreateChatClient(string model)
    {
        var client = new OpenAIClient(Credential);
        return client.GetChatClient(model);
    }
}
