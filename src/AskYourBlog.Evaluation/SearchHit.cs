namespace AskYourBlog.Evaluation;

public sealed record SearchHit(string PostId, string Section, string Url, double Score);