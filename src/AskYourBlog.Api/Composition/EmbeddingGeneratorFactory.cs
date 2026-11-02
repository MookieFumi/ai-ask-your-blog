using System.ClientModel;
using AskYourBlog.Api.Chat;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

namespace AskYourBlog.Api.Composition;

public static class EmbeddingGeneratorFactory
{
    // Run mode: local Ollama. Publish mode: a remote OpenAI-compatible provider.
    public static IEmbeddingGenerator<string, Embedding<float>> Create(IConfiguration configuration)
    {
        var local = OllamaConnection.TryParse(configuration.GetConnectionString("embeddings"));
        if (local is not null)
        {
            return new OllamaApiClient(local.Endpoint, local.Model);
        }

        var remote = configuration.GetSection("Embeddings:Remote").Get<RemoteProviderOptions>();
        if (remote is null || !remote.IsConfigured)
        {
            throw new InvalidOperationException("No embedding provider configured.");
        }

        return new OpenAIClient(new ApiKeyCredential(remote.ApiKey), new OpenAIClientOptions { Endpoint = new Uri(remote.Endpoint) })
            .GetEmbeddingClient(remote.Model)
            .AsIEmbeddingGenerator();
    }
}