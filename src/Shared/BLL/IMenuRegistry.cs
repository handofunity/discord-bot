namespace HoU.GuildBot.Shared.BLL;

public interface IMenuRegistry
{
    delegate Task<string?> MenuCallback(DiscordUserId userId, string customId, IReadOnlyCollection<string> selectedValues);
    
    IDiscordAccess DiscordAccess { set; }
    
    bool IsButtonMenu(string customId, out MenuCallback? callback);

    ButtonComponent GetButtonMenuComponent(string customId);

    bool IsRevokeButtonMenu(string customId);

    ModalData? GetRevokeMenuModal(string customId,
                                  DiscordUserId userId);
    
    bool IsSelectMenu(string customId, out MenuCallback? callback);

    IEnumerable<ActionComponent> GetSelectMenuComponents(string customId);
    
    bool IsModalMenu(string customId, out MenuCallback? callback);

    void UpdateExistingPartialCustomIds(string customId,
                                        string[] existingPartialCustomIds);
}