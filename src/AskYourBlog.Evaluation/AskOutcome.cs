namespace AskYourBlog.Evaluation;

public sealed record AskOutcome(bool Found, IReadOnlyList<string> CitedUrls);