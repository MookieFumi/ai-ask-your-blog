namespace AskYourBlog.Api.Ingestion;

public sealed record FrontMatter(IReadOnlyDictionary<string, string> Values, string Body)
{
    public string Get(string key, string fallback = "")
    {
        return Values.TryGetValue(key, out var value) ? value : fallback;
    }
}