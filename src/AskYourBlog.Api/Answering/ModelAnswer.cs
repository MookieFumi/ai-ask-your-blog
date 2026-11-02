namespace AskYourBlog.Api.Answering;

public sealed record ModelAnswer(bool Found, string Answer, int[] CitationIds);