using Microsoft.Extensions.VectorData;

namespace AskYourBlog.Api.Chunking;

public sealed class BlogChunk
{
    // NOTE: not compiled against a real store yet. Attribute shapes follow Microsoft Learn.
    public const int Dimensions = 768; // nomic-embed-text. Same model for indexing and querying.

    [VectorStoreKey]
    public string Id { get; set; } = "";

    [VectorStoreData(IsIndexed = true)]
    public string PostId { get; set; } = "";

    [VectorStoreData]
    public string ContentHash { get; set; } = "";

    [VectorStoreData]
    public int ChunkCount { get; set; }

    [VectorStoreData]
    public string PostTitle { get; set; } = "";

    [VectorStoreData]
    public string Url { get; set; } = "";

    [VectorStoreData]
    public string Date { get; set; } = "";

    [VectorStoreData]
    public string Topic { get; set; } = "";

    [VectorStoreData]
    public string Series { get; set; } = "";

    [VectorStoreData]
    public string Section { get; set; } = "";

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; set; } = "";

    // A string vector property is embedded by the store-level generator.
    [VectorStoreVector(Dimensions, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public string Embedding { get; set; } = "";
}