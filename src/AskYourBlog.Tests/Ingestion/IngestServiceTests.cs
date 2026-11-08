using AskYourBlog.Api.Chunking;
using AskYourBlog.Api.Ingestion;
using AskYourBlog.Tests.TestDoubles;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.VectorData;

namespace AskYourBlog.Tests.Ingestion;

public sealed class IngestServiceTests : IDisposable
{
    private readonly string dir = Directory.CreateTempSubdirectory().FullName;
    private readonly VectorStoreCollection<string, BlogChunk> collection =
        new InMemoryVectorStore(new() { EmbeddingGenerator = new FakeEmbeddingGenerator() })
            .GetCollection<string, BlogChunk>("test");

    private IngestService Service => new(collection, new PostReader(), new MarkdownChunker(new ChunkOptions()));

    public void Dispose()
    {
        Directory.Delete(dir, true);
    }

    private void Write(string name, string body, string frontMatter = "title: T")
    {
        File.WriteAllText(Path.Combine(dir, name), $"---\n{frontMatter}\n---\n{body}");
    }

    private Task<IngestReport> Run()
    {
        return Service.RunAsync(dir, "https://x", CancellationToken.None);
    }

    [Fact]
    public async Task New_post_is_added_then_skipped()
    {
        Write("2026-01-01-a.md", "## S\n\ntext");

        Assert.Equal(new IngestReport(1, 0, 0, 0), await Run());
        Assert.Equal(new IngestReport(0, 0, 0, 1), await Run());
    }

    [Fact]
    public async Task Modified_post_replaces_old_chunks()
    {
        Write("2026-01-01-a.md", "## S\n\nold");
        await Run();
        Write("2026-01-01-a.md", "## S\n\nnew");

        Assert.Equal(new IngestReport(0, 1, 0, 0), await Run());

        var texts = new List<string>();
        await foreach (var chunk in collection.GetAsync(c => c.PostId == "2026-01-01-a", top: 10))
        {
            texts.Add(chunk.Text);
        }

        Assert.Equal(["new"], texts);
    }

    [Fact]
    public async Task Removed_file_deletes_its_chunks()
    {
        Write("2026-01-01-a.md", "## S\n\ntext");
        await Run();
        File.Delete(Path.Combine(dir, "2026-01-01-a.md"));

        Assert.Equal(new IngestReport(0, 0, 1, 0), await Run());
    }

    [Fact]
    public async Task Post_turned_unpublished_is_removed_from_the_index()
    {
        Write("2026-01-01-a.md", "## S\n\ntext");
        await Run();
        Write("2026-01-01-a.md", "## S\n\ntext", "title: T\npublished: false");

        Assert.Equal(new IngestReport(0, 0, 1, 0), await Run());
    }
}