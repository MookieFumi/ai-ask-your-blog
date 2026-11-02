namespace AskYourBlog.Api.Chunking;

public sealed class ChunkOptions
{
    // About 512 tokens. To be tuned with the golden set.
    public int MaxChars { get; init; } = 2000;
}