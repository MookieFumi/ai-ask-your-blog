var builder = DistributedApplication.CreateBuilder(args);

// pgvector image: tag pg17 verified on Docker Hub (2026-11-02).
var postgres = builder.AddPostgres("pg")
    .WithImage("pgvector/pgvector")
    .WithImageTag("pg17")
    .WithDataVolume("askyourblog-pgdata");
var blog = postgres.AddDatabase("blog");

builder.AddProject<Projects.AskYourBlog_Api>("api")
    .WithReference(blog)
    .WaitFor(blog);

builder.Build().Run();