namespace HoU.GuildBot.DAL;

internal static class HttpRequestHelper
{
    internal static async Task<TResult?> PerformAuthorizedRequestAsync<TClient, TResult>(this HttpClient httpClient,
                                                                                         IBearerTokenManager<TClient> bearerTokenManager,
                                                                                         AuthorizationEndpoint authorizationEndpoint,
                                                                                         Func<Task<HttpResponseMessage?>> invokeHttpRequest,
                                                                                         Func<HttpResponseMessage?, Task<TResult>>
                                                                                             handleResult)
        where TClient : class
    {
        var tokenSet = await bearerTokenManager.GetAndSetBearerToken(httpClient, authorizationEndpoint, false);
        if (!tokenSet)
        {
            // If the Bearer token is not set, the HTTP request will fail anyway.
            // Instead of invoking it, just return.
            return default;
        }

        var response = await invokeHttpRequest();

        if (response is null || response.IsSuccessStatusCode)
        {
            // If the result is null, or a success, invoke the result handler.
            return await handleResult(response);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // If the first response status code is Unauthorized (401), the token might either be expired, or invalid.
            var isExpired = response.Headers.TryGetValues("Token-Expired", out var expiredTokenValues)
                         && expiredTokenValues.Any(m => m.ToLowerInvariant() == "true");
            if (isExpired)
            {
                // If the token is expired, refresh the token.
                tokenSet = await bearerTokenManager.GetAndSetBearerToken(httpClient, authorizationEndpoint, true);
                if (!tokenSet)
                {
                    // If the Bearer token is not set, the HTTP request will fail anyway.
                    // Instead of invoking it, just return.
                    return default;
                }

                // If the token is set, perform the request again.
                response = await invokeHttpRequest();
            }
        }

        // If the status code is no success status code, and not Unauthorized (401), or was 401 and was invoked again with a refreshed token, handle the result here.
        return await handleResult(response);
    }
}