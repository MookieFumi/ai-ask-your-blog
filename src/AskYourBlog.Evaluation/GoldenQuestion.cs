namespace AskYourBlog.Evaluation;

public sealed record GoldenQuestion(string Question, string[] ExpectedSlugs, bool Answerable);