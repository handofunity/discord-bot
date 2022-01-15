using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.DAL;

namespace HoU.GuildBot.DAL;

public class WebAccess : IWebAccess
{
    private readonly ILogger<WebAccess> _logger;

    public WebAccess(ILogger<WebAccess> logger)
    {
        _logger = logger;
    }

    async Task<byte[]?> IWebAccess.GetContentFromUrlAsync(string? url)
    {
        if (url == null)
            return null;

        try
        {
            using var client = new HttpClient();
            using var result = await client.GetAsync(url);

            return result.IsSuccessStatusCode
                       ? await result.Content.ReadAsByteArrayAsync()
                       : null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load content from {Url}", url);
            return null;
        }
    }
}