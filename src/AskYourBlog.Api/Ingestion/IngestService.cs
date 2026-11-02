using AskYourBlog.Api.Chunking;
using Microsoft.Extensions.VectorData;

namespace AskYourBlog.Api.Ingestion;

// Incremental and idempotent: a post is skipped when its hash is unchanged and all of its
// chunks are stored; changed (or half-written) posts are replaced; removed posts are deleted.
public sealed class IngestService(
    VectorStoreCollection<string, BlogChunk> collection,
    PostReader reader,
    MarkdownChunker chunker)
{
    private sealed record Known(string Hash, int Expected, List<string> Ids);

    public async Task<IngestReport> RunAsync(string postsDir, string baseUrl, CancellationToken ct)
    {
        await collection.EnsureCollectionExistsAsync(ct);

        var known = await LoadKnownAsync(ct);
        var posts = await reader.ReadAsync(postsDir, baseUrl, ct);
        var seen = new HashSet<string>();
        int added = 0, updated = 0, deleted = 0, skipped = 0;

        foreach (var post in posts)
        {
            seen.Add(post.Id);

            if (known.TryGetValue(post.Id, out var existing))
            {
                if (existing.Hash == post.ContentHash && existing.Ids.Count == existing.Expected)
                {
                    skipped++;
                    continue;
                }

                await collection.DeleteAsync(existing.Ids, ct);
                updated++;
            }
            else
            {
                added++;
            }

            foreach (var chunk in chunker.Chunk(post))
            {
                await collection.UpsertAsync(chunk, ct);
            }
        }

        // Unpublished or removed posts must not stay in the index.
        foreach (var (postId, existing) in known.Where(k => !seen.Contains(k.Key)))
        {
            await collection.DeleteAsync(existing.Ids, ct);
            deleted++;
        }

        return new IngestReport(added, updated, deleted, skipped);
    }

    private async Task<Dictionary<string, Known>> LoadKnownAsync(CancellationToken ct)
    {
        var map = new Dictionary<string, Known>();

        // Paging for large stores is not handled: a blog has hundreds of chunks, not millions.
        await foreach (var chunk in collection.GetAsync(c => c.PostId != "", top: 100_000, cancellationToken: ct))
        {
            if (!map.TryGetValue(chunk.PostId, out var known))
            {
                known = new Known(chunk.ContentHash, chunk.ChunkCount, []);
                map[chunk.PostId] = known;
            }

            known.Ids.Add(chunk.Id);
        }

        return map;
    }
}