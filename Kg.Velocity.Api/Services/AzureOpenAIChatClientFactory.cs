using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Kg.Velocity.Api.Services;

public class AzureOpenAIChatClientFactory(IConfiguration configuration)
{
    private readonly string _endpoint = configuration["AzureOpenAI:Endpoint"]
        ?? throw new InvalidOperationException("AzureOpenAI:Endpoint not configured");

    private readonly string _apiKey = configuration["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("AzureOpenAI:ApiKey not configured");

    public ChatClient CreateChatClient(string deploymentName)
    {
        var client = new AzureOpenAIClient(
            new Uri(_endpoint),
            new ApiKeyCredential(_apiKey));

        return client.GetChatClient(deploymentName);
    }
}
