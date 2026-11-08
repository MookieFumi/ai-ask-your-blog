using AskYourBlog.Api.Chunking;
using Microsoft.Extensions.AI;

namespace AskYourBlog.Tests.TestDoubles;

// Deterministic fake: a one-hot vector derived from the text. Only for tests.
public sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = new GeneratedEmbeddings<Embedding<float>>();
        foreach (var value in values)
        {
            var vector = new float[BlogChunk.Dimensions];
            vector[(uint)value.GetHashCode() % BlogChunk.Dimensions] = 1;
            result.Add(new Embedding<float>(vector));
        }

        return Task.FromResult(result);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }

    public void Dispose()
    {
    }
}