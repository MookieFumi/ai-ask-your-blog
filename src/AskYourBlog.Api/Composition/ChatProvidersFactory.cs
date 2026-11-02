using System.ClientModel;
using AskYourBlog.Api.Chat;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

namespace AskYourBlog.Api.Composition;

public static class ChatProvidersFactory
{
    // Order matters: remote first, local Ollama as the fallback (run mode only).
    public static IReadOnlyList<ProviderEntry> Create(IConfiguration configuration)
    {
        var providers = new List<ProviderEntry>();

        var remote = configuration.GetSection("Chat:Remote").Get<RemoteProviderOptions>();
        if (remote is not null && remote.IsConfigured)
        {
            var client = new OpenAIClient(new ApiKeyCredential(remote.ApiKey), new OpenAIClientOptions { Endpoint = new Uri(remote.Endpoint) })
                .GetChatClient(remote.Model)
                .AsIChatClient();
            providers.Add(new ProviderEntry("remote", client, TimeSpan.FromSeconds(30)));
        }

        var local = OllamaConnection.TryParse(configuration.GetConnectionString("chat"));
        if (local is not null)
        {
            providers.Add(new ProviderEntry("local", new OllamaApiClient(local.Endpoint, local.Model), TimeSpan.FromSeconds(180)));
        }

        if (providers.Count == 0)
        {
            throw new InvalidOperationException("No chat provider configured.");
        }

        return providers;
    }
}