var builder = DistributedApplication.CreateBuilder(args);

// pgvector image: tag pg17 verified on Docker Hub (2026-11-02).
var postgres = builder.AddPostgres("pg")
    .WithImage("pgvector/pgvector")
    .WithImageTag("pg17")
    .WithDataVolume("askyourblog-pgdata");
var blog = postgres.AddDatabase("blog");

var api = builder.AddProject<Projects.AskYourBlog_Api>("api")
    .WithReference(blog)
    .WaitFor(blog)
    .WithEnvironment("Blog__PostsDir", builder.AddParameter("posts-dir"))
    .WithEnvironment("Admin__Key", builder.AddParameter("admin-key", secret: true));

// Remote chat provider (Groq or OpenRouter): endpoint, model and key live in parameters, never in code.
// Base URLs verified in the official docs (2026-11-02): https://api.groq.com/openai/v1 and https://openrouter.ai/api/v1
api.WithEnvironment("Chat__Remote__Endpoint", builder.AddParameter("chat-endpoint"))
    .WithEnvironment("Chat__Remote__Model", builder.AddParameter("chat-model"))
    .WithEnvironment("Chat__Remote__ApiKey", builder.AddParameter("chat-api-key", secret: true));

if (builder.ExecutionContext.IsPublishMode)
{
    // Cloud: no Ollama (slow on CPU, expensive on GPU). Embeddings go to a remote provider.
    api.WithEnvironment("Embeddings__Remote__Endpoint", builder.AddParameter("embeddings-endpoint"))
        .WithEnvironment("Embeddings__Remote__Model", builder.AddParameter("embeddings-model"))
        .WithEnvironment("Embeddings__Remote__ApiKey", builder.AddParameter("embeddings-api-key", secret: true));
}
else
{
    // Local: Ollama with a persistent volume so models are not downloaded again.
    var ollama = builder.AddOllama("ollama").WithDataVolume();
    var chat = ollama.AddModel("chat", "llama3.2:3b");
    var embeddings = ollama.AddModel("embeddings", "nomic-embed-text");
    api.WithReference(chat).WaitFor(chat)
        .WithReference(embeddings).WaitFor(embeddings);
}

builder.Build().Run();