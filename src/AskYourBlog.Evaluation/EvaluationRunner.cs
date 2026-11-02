namespace AskYourBlog.Evaluation;

public sealed class EvaluationRunner(IBlogApi api)
{
    public async Task<EvaluationReport> RunAsync(IReadOnlyList<GoldenQuestion> questions, CancellationToken ct)
    {
        var results = new List<QuestionResult>();

        foreach (var question in questions)
        {
            var hits = await api.SearchAsync(question.Question, ct);
            var outcome = await api.AskAsync(question.Question, ct);

            var retrievedIds = hits.Select(h => h.PostId).ToHashSet();
            var citedIds = outcome.CitedUrls.Select(SlugFromUrl).ToList();

            results.Add(new QuestionResult(
                question,
                question.ExpectedSlugs.Any(retrievedIds.Contains),
                hits.Count == 0 ? 0 : hits.Max(h => h.Score),
                outcome.Found,
                question.ExpectedSlugs.Any(citedIds.Contains),
                citedIds.All(retrievedIds.Contains)));
        }

        return new EvaluationReport(results);
    }

    public static string SlugFromUrl(string url)
    {
        return url.TrimEnd('/').Split('/')[^1];
    }
}