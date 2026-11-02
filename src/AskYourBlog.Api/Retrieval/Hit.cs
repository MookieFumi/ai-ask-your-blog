using AskYourBlog.Api.Chunking;

namespace AskYourBlog.Api.Retrieval;

public sealed record Hit(BlogChunk Chunk, double Score);