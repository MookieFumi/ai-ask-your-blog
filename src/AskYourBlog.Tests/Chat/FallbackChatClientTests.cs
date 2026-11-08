using System.Net;
using AskYourBlog.Api.Chat;
using AskYourBlog.Tests.TestDoubles;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace AskYourBlog.Tests.Chat;

public sealed class FallbackChatClientTests
{
    private static FallbackChatClient Create(params StubChatClient[] clients)
    {
        var providers = clients.Select((c, i) => new ProviderEntry($"p{i}", c, TimeSpan.FromSeconds(5))).ToList();
        return new FallbackChatClient(providers, NullLogger<FallbackChatClient>.Instance);
    }

    private static Task<ChatResponse> Fail(HttpStatusCode status)
    {
        throw new HttpRequestException("failure", null, status);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task Falls_back_on_transient_errors(HttpStatusCode status)
    {
        var second = new StubChatClient(() => Task.FromResult(StubChatClient.Reply("ok")));
        var client = Create(new StubChatClient(() => Fail(status)), second);

        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        Assert.Equal("ok", response.Text);
        Assert.Equal(1, second.Calls);
    }

    [Fact]
    public async Task Does_not_fall_back_on_bad_request()
    {
        var second = new StubChatClient(() => Task.FromResult(StubChatClient.Reply("ok")));
        var client = Create(new StubChatClient(() => Fail(HttpStatusCode.BadRequest)), second);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]));

        Assert.Equal(0, second.Calls);
    }

    [Fact]
    public async Task Throws_when_all_providers_fail()
    {
        var client = Create(
            new StubChatClient(() => Fail(HttpStatusCode.TooManyRequests)),
            new StubChatClient(() => Fail(HttpStatusCode.ServiceUnavailable)));

        await Assert.ThrowsAsync<AggregateException>(() => client.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]));
    }

    [Fact]
    public void Ollama_connection_string_is_parsed()
    {
        var connection = OllamaConnection.TryParse("Endpoint=http://localhost:11434;Model=llama3.2:3b");

        Assert.Equal("llama3.2:3b", connection!.Model);
        Assert.Null(OllamaConnection.TryParse(null));
        Assert.Null(OllamaConnection.TryParse("Endpoint=http://localhost:11434"));
    }
}