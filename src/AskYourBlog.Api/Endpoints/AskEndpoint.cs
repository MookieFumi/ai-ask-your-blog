using AskYourBlog.Api.Answering;

namespace AskYourBlog.Api.Endpoints;

public static class AskEndpoint
{
    public static IEndpointRouteBuilder MapAsk(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/ask", async (AskRequest request, AnswerService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest(new { error = "Question is required." });
            }

            return Results.Ok(await service.AskAsync(request.Question, ct));
        });

        return routes;
    }
}