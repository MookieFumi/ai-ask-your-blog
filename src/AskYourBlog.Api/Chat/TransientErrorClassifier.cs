using System.ClientModel;
using System.Net;

namespace AskYourBlog.Api.Chat;

// Decides whether a provider failure justifies trying the next provider.
// 429, 5xx and timeouts do; other 4xx (bad request, auth) and caller cancellation do not.
// The exception types thrown by each client are NOT verified against real 429/5xx responses.
public static class TransientErrorClassifier
{
    public static bool IsTransient(Exception exception, CancellationToken caller)
    {
        return exception switch
        {
            ClientResultException c => c.Status == 429 || c.Status >= 500,
            HttpRequestException h => h.StatusCode is null
                || h.StatusCode == HttpStatusCode.TooManyRequests
                || (int)h.StatusCode >= 500,
            TimeoutException => true,
            OperationCanceledException => !caller.IsCancellationRequested,
            _ => false
        };
    }
}