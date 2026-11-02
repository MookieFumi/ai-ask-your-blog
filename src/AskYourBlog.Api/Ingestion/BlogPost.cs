namespace AskYourBlog.Api.Ingestion;

public sealed record BlogPost(
    string Id,
    string Title,
    string Url,
    string Date,
    string Topic,
    string Series,
    string Lang,
    string Body,
    string ContentHash);