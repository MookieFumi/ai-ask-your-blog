using AskYourBlog.Api.Answering;
using AskYourBlog.Api.Chat;
using AskYourBlog.Api.Chunking;
using AskYourBlog.Api.Ingestion;
using AskYourBlog.Api.Retrieval;
using CommunityToolkit.VectorData.PgVector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Npgsql;

namespace AskYourBlog.Api.Composition;

public static class RagServiceCollectionExtensions
{
    // Not run against a real database yet.
    public static IServiceCollection AddRag(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(EmbeddingGeneratorFactory.Create(configuration));
        services.AddHostedService<DatabaseInitializer>();

        services.AddSingleton(sp =>
        {
            var builder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("blog"));
            builder.UseVector();
            var store = new PostgresVectorStore(
                builder.Build(),
                ownsDataSource: true,
                new PostgresVectorStoreOptions { EmbeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>() });

            // Bump the version suffix when the embedding model changes: the index must be rebuilt.
            return store.GetCollection<string, BlogChunk>($"blog_{BlogChunk.Dimensions}d_v1");
        });

        services.AddSingleton(new ChunkOptions());
        services.AddSingleton<MarkdownChunker>();
        services.AddSingleton<PostReader>();
        services.AddSingleton<IngestService>();
        services.AddSingleton<Retriever>();
        services.AddSingleton(new AnswerOptions { MinScore = configuration.GetValue("Rag:MinScore", 0.5) });
        services.AddSingleton<IChatClient>(sp => new ChatClientBuilder(
                new FallbackChatClient(ChatProvidersFactory.Create(configuration), sp.GetRequiredService<ILogger<FallbackChatClient>>()))
            .UseOpenTelemetry(sourceName: "AskYourBlog")
            .Build());
        services.AddSingleton<AnswerService>();

        return services;
    }
}