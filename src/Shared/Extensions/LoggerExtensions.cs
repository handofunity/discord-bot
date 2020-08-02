using System.Net;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.Shared.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogRequestError(this ILogger logger,
                                           string baseAddress,
                                           string route,
                                           string reason)
        {
            logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason}", baseAddress, route, reason);
        }

        public static void LogRequestError(this ILogger logger,
                                           string baseAddress,
                                           string route,
                                           HttpStatusCode statusCode)
        {
            logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason} {HttpStatusCodeName} {HttpStatusCode}",
                              baseAddress,
                              route,
                              "HTTP Status Code",
                              statusCode.ToString(),
                              (int) statusCode);
        }
    }
}