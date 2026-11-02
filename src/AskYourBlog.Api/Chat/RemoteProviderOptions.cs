namespace AskYourBlog.Api.Chat;

// Remote OpenAI-compatible provider (Groq, OpenRouter...). Everything comes from configuration.
public sealed class RemoteProviderOptions
{
    public string Endpoint { get; init; } = "";

    public string Model { get; init; } = "";

    public string ApiKey { get; init; } = "";

    public bool IsConfigured => ApiKey.Length > 0 && Endpoint.Length > 0 && Model.Length > 0;
}