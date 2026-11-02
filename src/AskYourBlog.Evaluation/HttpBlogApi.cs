using System.Net.Http.Json;

namespace AskYourBlog.Evaluation;

public sealed class HttpBlogApi(HttpClient http) : IBlogApi
{
    private sealed record AskResponse(bool Found, List<CitationResponse> Citations);

    private sealed record CitationResponse(string Url);

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string question, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/v1/search", new { question }, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<SearchHit>>(ct))!;
    }

    public async Task<AskOutcome> AskAsync(string question, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/v1/ask", new { question }, ct);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<AskResponse>(ct))!;
        return new AskOutcome(body.Found, body.Citations.Select(c => c.Url).ToList());
    }
}