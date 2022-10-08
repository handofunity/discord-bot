namespace HoU.GuildBot.Shared.BLL;

public interface IKeycloakSyncService
{
    Task SyncAllUsersAsync();

    Task DeleteFlaggedUsersAsync();
}