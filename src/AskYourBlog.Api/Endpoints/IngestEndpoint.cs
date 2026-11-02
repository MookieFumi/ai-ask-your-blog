using AskYourBlog.Api.Ingestion;

namespace AskYourBlog.Api.Endpoints;

public static class IngestEndpoint
{
    // Minimal guard with a shared key. Use real authentication if this is ever exposed.
    public static IEndpointRouteBuilder MapIngest(this IEndpointRouteBuilder routes, IConfiguration configuration)
    {
        routes.MapPost("/api/v1/ingest", async (HttpRequest http, IngestService service, CancellationToken ct) =>
        {
            var key = configuration["Admin:Key"];
            if (string.IsNullOrEmpty(key) || http.Headers["X-Admin-Key"] != key)
            {
                return Results.Unauthorized();
            }

            var report = await service.RunAsync(configuration["Blog:PostsDir"]!, configuration["Blog:BaseUrl"]!, ct);
            return Results.Ok(report);
        });

        return routes;
    }
}