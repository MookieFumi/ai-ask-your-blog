# ask-your-blog

RAG assistant over the posts of [mookiefumi.com](https://mookiefumi.com). It answers with the blog's content, cites the post URL and says "I don't know" when there is no evidence.

Stack: .NET 10, Aspire, PostgreSQL + pgvector, Microsoft.Extensions.AI, local Ollama embeddings.

## Layout

- `src/AskYourBlog.AppHost`: Aspire AppHost (Postgres with pgvector, persistent volume).
- `src/AskYourBlog.Api`: ingestion and chunking (retrieval and answering come next).
- `src/AskYourBlog.Tests`: unit tests.

## Posts source

Astro posts in `src/content/posts/yyyy-mm-dd-slug.md|mdx` of the blog repository. Only posts with `published: true` (or no flag) and without `noindex: true` are indexed. The public URL is `/{file name without extension}`.

## Status

Foundation only. Not yet run against Docker, Postgres or Ollama.

## Evaluation

`golden/golden.v1.jsonl` holds 20 draft questions (15 answerable, 3 without an answer in the published posts, 2 with exact terms). It is a draft: review the questions and expected posts before trusting the numbers.

With the API running and the index populated (`POST /api/v1/ingest` with `X-Admin-Key`):

```bash
dotnet run --project src/AskYourBlog.Evaluation -- http://localhost:<api-port> golden/golden.v1.jsonl
```

It reports recall@k, citation hit rate, abstention accuracy, citation validity and the score gap used to calibrate `Rag:MinScore`.
