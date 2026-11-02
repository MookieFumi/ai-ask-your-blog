namespace AskYourBlog.Api.Ingestion;

public sealed record IngestReport(int Added, int Updated, int Deleted, int Skipped);