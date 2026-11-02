using AskYourBlog.Evaluation;

// Usage: dotnet run --project src/AskYourBlog.Evaluation -- <apiBaseUrl> <goldenFile> [--no-timeout]
// Needs the API running (aspire run) and the index populated (POST /api/v1/ingest).
var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
var goldenFile = args.Length > 1 ? args[1] : "golden/golden.v1.jsonl";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromMinutes(5) };
var questions = GoldenSetReader.Read(goldenFile);
var report = await new EvaluationRunner(new HttpBlogApi(http)).RunAsync(questions, CancellationToken.None);

foreach (var r in report.Results)
{
    var mark = r.Question.Answerable
        ? (r.ExpectedPostRetrieved && r.CitedExpectedPost ? "OK  " : "FAIL")
        : (r.Found ? "FAIL" : "OK  ");
    Console.WriteLine($"{mark} score={r.BestScore:F3} found={r.Found} {r.Question.Question}");
}

Console.WriteLine();
Console.WriteLine($"recall@k             {report.RecallAtK:P0}");
Console.WriteLine($"citation hit rate    {report.CitationHitRate:P0}");
Console.WriteLine($"abstention accuracy  {report.AbstentionAccuracy:P0}");
Console.WriteLine($"citation validity    {report.CitationValidity:P0}");
Console.WriteLine($"lowest answerable score   {report.LowestAnswerableScore:F3}");
Console.WriteLine($"highest unanswerable score {report.HighestUnanswerableScore:F3}");