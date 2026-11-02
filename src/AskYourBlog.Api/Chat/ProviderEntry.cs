using Microsoft.Extensions.AI;

namespace AskYourBlog.Api.Chat;

public sealed record ProviderEntry(string Name, IChatClient Client, TimeSpan Timeout);