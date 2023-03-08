namespace HoU.GuildBot.Shared.BLL;

public interface IKeycloakSyncService
{
    /// <summary>
    /// Syncs all current Discord users with Keycloak.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SyncAllUsersAsync();

    /// <summary>
    /// Deletes users flagged for deletion, if they are past their expiration date. 
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteFlaggedUsersAsync();
}