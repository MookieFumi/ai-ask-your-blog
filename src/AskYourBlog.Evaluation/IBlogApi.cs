namespace AskYourBlog.Evaluation;

public interface IBlogApi
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string question, CancellationToken ct);

    Task<AskOutcome> AskAsync(string question, CancellationToken ct);
}