using Microsoft.Extensions.AI;

namespace AskYourBlog.Tests.TestDoubles;

public sealed class StubChatClient(Func<Task<ChatResponse>> respond) : IChatClient
{
    public int Calls { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Calls++;
        return respond();
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }

    public void Dispose()
    {
    }

    public static ChatResponse Reply(string text)
    {
        return new ChatResponse(new ChatMessage(ChatRole.Assistant, text));
    }
}