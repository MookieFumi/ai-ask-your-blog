namespace AskYourBlog.Api.Chat;

// Aspire (CommunityToolkit) injects Ollama as "Endpoint=<url>;Model=<model>".
public sealed record OllamaConnection(Uri Endpoint, string Model)
{
    public static OllamaConnection? TryParse(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var pairs = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        if (pairs.TryGetValue("Endpoint", out var endpoint) && pairs.TryGetValue("Model", out var model))
        {
            return new OllamaConnection(new Uri(endpoint), model);
        }

        return null;
    }
}