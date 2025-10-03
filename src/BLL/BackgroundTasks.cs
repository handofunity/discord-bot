namespace HoU.GuildBot.BLL;

public class BackgroundTasks(IDiscordAccess _discordAccess,
                             IBackgroundJobClient _backgroundJobClient,
                             ILogger<BackgroundTasks> _logger)
{
    public async Task DeleteCategoryChannelAsync(ulong categoryChannelId,
        DateTimeOffset gracePeriod)
    {
        try
        {
            var forceDelete = TimeProvider.System.GetUtcNow() > gracePeriod;
            var deleted = await _discordAccess.DeleteCategoryChannelAsync((DiscordCategoryChannelId)categoryChannelId,
                forceDelete);

            if (deleted)
            {
                _logger.LogInformation("Deleted category channel {CategoryChannelId}", categoryChannelId.ToString());
                return;
            }

            _logger.LogWarning("Failed to delete category channel {CategoryChannelId}, will retry in 5 minutes.",
                categoryChannelId.ToString());
            _backgroundJobClient.Schedule<BackgroundTasks>(m => m.DeleteCategoryChannelAsync(categoryChannelId, gracePeriod),
                TimeProvider.System.GetUtcNow().AddMinutes(5));
            return;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,
                "Failed to delete category channel {CategoryChannelId}",
                categoryChannelId.ToString());
        }
    }
}
