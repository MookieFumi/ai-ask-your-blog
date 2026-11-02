using AskYourBlog.Api.Chunking;
using AskYourBlog.Api.Ingestion;

namespace AskYourBlog.Tests.Chunking;

public sealed class MarkdownChunkerTests
{
    private static BlogPost Post(string body)
    {
        return new BlogPost("2026-01-01-a", "Title", "https://x/2026-01-01-a", "2026-01-01", "t", "s", "es", body, "hash");
    }

    [Fact]
    public void Splits_by_headings_with_stable_ids()
    {
        var chunks = new MarkdownChunker(new ChunkOptions()).Chunk(Post("Intro text\n\n## First\n\nA\n\n## Second\n\nB"));

        Assert.Equal(["2026-01-01-a#00-intro", "2026-01-01-a#01-first", "2026-01-01-a#02-second"], chunks.Select(c => c.Id));
        Assert.All(chunks, c => Assert.Equal(3, c.ChunkCount));
    }

    [Fact]
    public void Heading_inside_a_code_fence_is_not_a_section()
    {
        var chunks = new MarkdownChunker(new ChunkOptions()).Chunk(Post("## S\n\n```bash\n## not a heading\n```"));

        Assert.Single(chunks);
    }

    [Fact]
    public void Code_blocks_are_never_split()
    {
        var code = "```csharp\n" + string.Join("\n", Enumerable.Repeat("var x = 1;", 400)) + "\n```";

        var chunks = new MarkdownChunker(new ChunkOptions { MaxChars = 500 }).Chunk(Post("## S\n\nbefore\n\n" + code + "\n\nafter"));

        Assert.All(chunks, c => Assert.Equal(1, c.Text.Split("```").Length % 2));
        Assert.True(chunks.Count >= 2);
    }

    [Fact]
    public void Embedding_text_has_title_and_section_prefix()
    {
        var chunk = new MarkdownChunker(new ChunkOptions()).Chunk(Post("## Sec\n\nbody")).Single();

        Assert.StartsWith("Title > Sec\n", chunk.Embedding);
    }
}