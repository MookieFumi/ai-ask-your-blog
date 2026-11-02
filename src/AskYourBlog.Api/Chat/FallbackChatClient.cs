using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AskYourBlog.Api.Chat;

// Tries each provider in order. Not a package: a small pattern written for this project.
public sealed class FallbackChatClient(IReadOnlyList<ProviderEntry> providers, ILogger<FallbackChatClient> logger) : IChatClient
{
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var list = messages.ToList();
        var errors = new List<Exception>();

        foreach (var provider in providers)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(provider.Timeout);

            try
            {
                return await provider.Client.GetResponseAsync(list, options, cts.Token);
            }
            catch (Exception e) when (TransientErrorClassifier.IsTransient(e, cancellationToken))
            {
                logger.LogWarning(e, "Provider {Provider} failed with a transient error; trying the next one", provider.Name);
                errors.Add(e);
            }
        }

        throw new AggregateException("All chat providers failed", errors);
    }

    // Simplification: not true streaming, because the fallback decision needs the whole response.
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var update in response.ToChatResponseUpdates())
        {
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey is null && serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        return providers[0].Client.GetService(serviceType, serviceKey);
    }

    public void Dispose()
    {
        foreach (var provider in providers)
        {
            provider.Client.Dispose();
        }
    }
}