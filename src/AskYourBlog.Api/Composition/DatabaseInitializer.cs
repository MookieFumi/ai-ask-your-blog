using AskYourBlog.Api.Chunking;
using Microsoft.Extensions.AI;
using Npgsql;

namespace AskYourBlog.Api.Composition;

// Creates the pgvector extension and checks that the embedding model matches the index dimension.
public sealed class DatabaseInitializer(
    IConfiguration configuration,
    IEmbeddingGenerator<string, Embedding<float>> embedder) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Needs privileges to create the extension. A plain data source: UseVector() needs the extension first.
        await using var dataSource = NpgsqlDataSource.Create(configuration.GetConnectionString("blog")!);
        await using var command = dataSource.CreateCommand("CREATE EXTENSION IF NOT EXISTS vector");
        await command.ExecuteNonQueryAsync(cancellationToken);

        var probe = await embedder.GenerateAsync(["probe"], cancellationToken: cancellationToken);
        var dimensions = probe[0].Vector.Length;
        if (dimensions != BlogChunk.Dimensions)
        {
            throw new InvalidOperationException(
                $"The embedding model returns {dimensions} dimensions but the index expects {BlogChunk.Dimensions}. Reindex is required.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}