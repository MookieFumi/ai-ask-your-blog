using AskYourBlog.Api.Retrieval;

namespace AskYourBlog.Api.Endpoints;

public static class SearchEndpoint
{
    // Retrieval only, with scores. Used to evaluate recall and to calibrate Rag:MinScore.
    public static IEndpointRouteBuilder MapSearch(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/search", async (AskRequest request, Retriever retriever, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest(new { error = "Question is required." });
            }

            var hits = await retriever.SearchAsync(request.Question, ct);
            return Results.Ok(hits.Select(h => new { postId = h.Chunk.PostId, section = h.Chunk.Section, url = h.Chunk.Url, score = h.Score }));
        });

        return routes;
    }
}