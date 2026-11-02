using System.Text.Json;

namespace AskYourBlog.Evaluation;

public static class GoldenSetReader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<GoldenQuestion> Read(string path)
    {
        return File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GoldenQuestion>(line, Options)!)
            .ToList();
    }
}