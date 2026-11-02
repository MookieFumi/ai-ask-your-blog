namespace AskYourBlog.Evaluation;

public sealed record QuestionResult(
    GoldenQuestion Question,
    bool ExpectedPostRetrieved,
    double BestScore,
    bool Found,
    bool CitedExpectedPost,
    bool CitationsAllRetrieved);