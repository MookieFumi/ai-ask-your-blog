using AskYourBlog.Api.Answering;
using AskYourBlog.Api.Chunking;
using AskYourBlog.Api.Retrieval;
using AskYourBlog.Tests.TestDoubles;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;

namespace AskYourBlog.Tests.Answering;

public sealed class AnswerServiceTests
{
    private static async Task<(AnswerService Service, StubChatClient Chat)> CreateAsync(string modelJson, double minScore = 0)
    {
        var collection = new InMemoryVectorStore(new() { EmbeddingGenerator = new FakeEmbeddingGenerator() })
            .GetCollection<string, BlogChunk>("test");
        await collection.EnsureCollectionExistsAsync();
        await collection.UpsertAsync(new BlogChunk
        {
            Id = "a#00-s",
            PostId = "a",
            PostTitle = "Post A",
            Url = "https://x/a",
            Section = "S",
            Text = "text",
            Embedding = "question"
        });

        var chat = new StubChatClient(() => Task.FromResult(StubChatClient.Reply(modelJson)));
        var retriever = new Retriever(collection, NullLogger<Retriever>.Instance);
        return (new AnswerService(retriever, chat, new AnswerOptions { MinScore = minScore }), chat);
    }

    [Fact]
    public async Task Below_threshold_answers_unknown_without_calling_the_model()
    {
        var (service, chat) = await CreateAsync("{}", minScore: 2);

        var result = await service.AskAsync("question", default);

        Assert.False(result.Found);
        Assert.Equal(0, chat.Calls);
    }

    [Fact]
    public async Task Citation_urls_come_from_metadata()
    {
        var (service, _) = await CreateAsync("""{"found":true,"answer":"yes","citationIds":[1]}""");

        var result = await service.AskAsync("question", default);

        Assert.True(result.Found);
        Assert.Equal("https://x/a", Assert.Single(result.Citations).Url);
    }

    [Fact]
    public async Task Invented_citation_ids_become_unknown()
    {
        var (service, _) = await CreateAsync("""{"found":true,"answer":"yes","citationIds":[99]}""");

        var result = await service.AskAsync("question", default);

        Assert.False(result.Found);
    }

    [Fact]
    public async Task Model_saying_not_found_becomes_unknown()
    {
        var (service, _) = await CreateAsync("""{"found":false,"answer":"","citationIds":[]}""");

        Assert.False((await service.AskAsync("question", default)).Found);
    }
}