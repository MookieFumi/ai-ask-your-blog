using AskYourBlog.Evaluation;

namespace AskYourBlog.Tests.Evaluation;

public sealed class EvaluationRunnerTests
{
    private sealed class FakeApi(Dictionary<string, (SearchHit[] Hits, AskOutcome Outcome)> answers) : IBlogApi
    {
        public Task<IReadOnlyList<SearchHit>> SearchAsync(string question, CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<SearchHit>>(answers[question].Hits);
        }

        public Task<AskOutcome> AskAsync(string question, CancellationToken ct)
        {
            return Task.FromResult(answers[question].Outcome);
        }
    }

    [Fact]
    public async Task Computes_recall_abstention_and_score_gap()
    {
        var api = new FakeApi(new()
        {
            ["good"] = ([new SearchHit("post-a", "S", "https://x/post-a", 0.8)], new AskOutcome(true, ["https://x/post-a"])),
            ["miss"] = ([new SearchHit("post-z", "S", "https://x/post-z", 0.6)], new AskOutcome(true, ["https://x/post-z"])),
            ["none"] = ([new SearchHit("post-q", "S", "https://x/post-q", 0.3)], new AskOutcome(false, []))
        });
        var questions = new[]
        {
            new GoldenQuestion("good", ["post-a"], true),
            new GoldenQuestion("miss", ["post-a"], true),
            new GoldenQuestion("none", [], false)
        };

        var report = await new EvaluationRunner(api).RunAsync(questions, default);

        Assert.Equal(0.5, report.RecallAtK);
        Assert.Equal(0.5, report.CitationHitRate);
        Assert.Equal(1.0, report.AbstentionAccuracy);
        Assert.Equal(1.0, report.CitationValidity);
        Assert.Equal(0.6, report.LowestAnswerableScore);
        Assert.Equal(0.3, report.HighestUnanswerableScore);
    }

    [Fact]
    public void Golden_file_is_valid()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../golden/golden.v1.jsonl"));

        var questions = GoldenSetReader.Read(path);

        Assert.Equal(20, questions.Count);
        Assert.All(questions.Where(q => !q.Answerable), q => Assert.Empty(q.ExpectedSlugs));
        Assert.All(questions.Where(q => q.Answerable), q => Assert.NotEmpty(q.ExpectedSlugs));
    }

    [Fact]
    public void Slug_comes_from_the_last_url_segment()
    {
        Assert.Equal("2026-09-13-x", EvaluationRunner.SlugFromUrl("https://mookiefumi.com/2026-09-13-x/"));
    }
}
