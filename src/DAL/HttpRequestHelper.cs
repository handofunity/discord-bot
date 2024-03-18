namespace HoU.GuildBot.DAL;

public static class HttpRequestHelper
{
    public static async Task<TResult?> PerformAuthorizedRequestAsync<TResult>(this HttpClient httpClient,
                                                                           HttpRequestMessage request,
                                                                           IBearerTokenManager bearerTokenManager,
                                                                           AuthorizationEndpoint authorizationEndpoint,
                                                                           Func<HttpResponseMessage?, Task<TResult>> handleResult)
    {
        await bearerTokenManager.GetAndSetBearerTokenAsync(request,
                                                           new Uri(authorizationEndpoint.AccessTokenBaseAddress),
                                                           authorizationEndpoint,
                                                           false);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
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
                await bearerTokenManager.GetAndSetBearerTokenAsync(request,
                                                                   new Uri(authorizationEndpoint.AccessTokenBaseAddress),
                                                                   authorizationEndpoint,
                                                                   true);

                // If the token is set, perform the request again.
                response = await httpClient.SendAsync(request);
            }
        }

        // If the status code is no success status code, and not Unauthorized (401), or was 401 and was invoked again with a refreshed token, handle the result here.
        return await handleResult(response);
    }
}