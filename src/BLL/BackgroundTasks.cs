namespace HoU.GuildBot.BLL;

public class BackgroundTasks(IDiscordAccess _discordAccess,
                             ILogger<BackgroundTasks> _logger)
{
    public async Task DeleteCategoryChannelAsync(ulong categoryChannelId)
    {
        try
        {
            await _discordAccess.DeleteCategoryChannelAsync((DiscordCategoryChannelId)categoryChannelId);
            _logger.LogInformation("Deleted category channel {CategoryChannelId}", categoryChannelId.ToString());
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,
                "Failed to delete category channel {CategoryChannelId}",
                categoryChannelId.ToString());
        }
    }
}
