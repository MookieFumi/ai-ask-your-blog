using AskYourBlog.Api.Chunking;
using Microsoft.Extensions.VectorData;

namespace AskYourBlog.Api.Retrieval;

public sealed class Retriever(VectorStoreCollection<string, BlogChunk> collection, ILogger<Retriever> logger)
{
    public async Task<IReadOnlyList<Hit>> SearchAsync(string question, CancellationToken ct, int top = 5)
    {
        var hits = new List<Hit>();

        await foreach (var result in collection.SearchAsync(question, top, cancellationToken: ct))
        {
            hits.Add(new Hit(result.Record, result.Score ?? 0));
        }

        // Visible in the Aspire dashboard: which chunks did this question retrieve, and with what score?
        logger.LogInformation(
            "Retrieved {Count} chunks for {Question}: {Chunks}",
            hits.Count,
            question,
            string.Join(" | ", hits.Select(h => $"{h.Chunk.Id}@{h.Score:F3}")));

        return hits;
    }
}