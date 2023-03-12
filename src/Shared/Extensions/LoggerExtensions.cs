namespace HoU.GuildBot.Shared.Extensions;

public static class LoggerExtensions
{
    public static void LogRequestError(this ILogger logger,
                                       string baseAddress,
                                       string route,
                                       string reason,
                                       Dictionary<string, string> details = null)
    {
        logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason} {@Details}", baseAddress, route, reason, details);
    }

    public static async Task LogRequestErrorAsync(this ILogger logger,
                                                  string baseAddress,
                                                  string route,
                                                  HttpResponseMessage responseMessage)
    {
        var statusCode = responseMessage.StatusCode;
        string responseMessageContent = string.Empty;
        try
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
                responseMessageContent = content;
        }
        catch
        {
            responseMessageContent = string.Empty;
        }
        logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason} {HttpStatusCodeName} {HttpStatusCode} {ResponseMessageContent}",
                          baseAddress,
                          route,
                          "HTTP Status Code",
                          statusCode.ToString(),
                          (int) statusCode,
                          responseMessageContent);
    }
}