namespace AskYourBlog.Evaluation;

public sealed record EvaluationReport(IReadOnlyList<QuestionResult> Results)
{
    private IEnumerable<QuestionResult> Answerable => Results.Where(r => r.Question.Answerable);

    private IEnumerable<QuestionResult> Unanswerable => Results.Where(r => !r.Question.Answerable);

    // Retrieval: is an expected post among the top-k hits?
    public double RecallAtK => Ratio(Answerable.Count(r => r.ExpectedPostRetrieved), Answerable.Count());

    // Generation: did the answer cite an expected post?
    public double CitationHitRate => Ratio(Answerable.Count(r => r.Found && r.CitedExpectedPost), Answerable.Count());

    // Abstention: unanswerable questions that got "no lo sé".
    public double AbstentionAccuracy => Ratio(Unanswerable.Count(r => !r.Found), Unanswerable.Count());

    // Citations must point to posts that were actually retrieved.
    public double CitationValidity => Ratio(Results.Count(r => r.CitationsAllRetrieved), Results.Count);

    // Lowest top score among answerable questions, and highest among unanswerable ones.
    // A MinScore between the two separates them; if they overlap, no threshold is enough.
    public double? LowestAnswerableScore => Answerable.Select(r => (double?)r.BestScore).DefaultIfEmpty().Min();

    public double? HighestUnanswerableScore => Unanswerable.Select(r => (double?)r.BestScore).DefaultIfEmpty().Max();

    private static double Ratio(int numerator, int denominator)
    {
        return denominator == 0 ? 0 : (double)numerator / denominator;
    }
}