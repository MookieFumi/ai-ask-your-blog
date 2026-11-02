namespace AskYourBlog.Api.Answering;

public sealed class AnswerOptions
{
    // Not calibrated: tune it with the unanswerable questions of the golden set.
    public double MinScore { get; init; } = 0.5;
}