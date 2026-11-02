namespace AskYourBlog.Api.Answering;

public sealed record AskResult(bool Found, string Answer, IReadOnlyList<Citation> Citations);