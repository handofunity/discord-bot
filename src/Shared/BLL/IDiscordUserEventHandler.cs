namespace HoU.GuildBot.Shared.BLL;

public interface IDiscordUserEventHandler
{
    IDiscordAccess DiscordAccess { set; }

    void HandleJoined(DiscordUserId userID,
                      Role roles,
                      DateTime joinedDate);

    void HandleLeft(DiscordUserId userID,
                    string username,
                    ushort discriminatorValue);

    UserRolesChangedResult HandleRolesChanged(DiscordUserId userID, Role oldRoles, Role newRoles);

    Task HandleRolesChanged(DiscordUserId userId,
                            string currentRoles);

    Task HandleStatusChanged(DiscordUserId userID, bool wasOnline, bool isOnline);

    /// <summary>
    /// Handles the action a user created on a message component.
    /// </summary>
    /// <param name="userId">The Id of the user who triggered the action.</param>
    /// <param name="customId">The custom Id of the component the user interacted with.</param>
    /// <param name="selectedValues">The selected values in the given action component.</param>
    /// <returns>Any success or error message that can be forwarded as response.</returns>
    Task<string?> HandleMessageComponentExecutedAsync(DiscordUserId userId,
                                                      string customId,
                                                      IReadOnlyCollection<string> selectedValues);
}