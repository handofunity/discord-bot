namespace HoU.GuildBot.Shared.BLL;

public interface IMenuRegistry
{
    delegate Task<string?> MenuCallback(DiscordUserId userId, string customId, IReadOnlyCollection<string> selectedValues);
    
    delegate Task<string?> ModalCallback(ModalResponse response);
    
    IDiscordAccess DiscordAccess { set; }

    void Initialize();
    
    bool IsButtonMenu(string customId, out MenuCallback? callback);

    ButtonComponent GetButtonMenuComponent(string customId);

    bool IsRevokeButtonMenu(string customId);

    ModalData? GetRevokeMenuModal(string customId,
                                  DiscordUserId userId);
    
    SelectMenuComponent? GetRevokeMenuSelectWorkaround(string customId,
                                                       DiscordUserId userId);
    
    bool IsSelectMenu(string customId, out MenuCallback? callback);

    IEnumerable<ActionComponent> GetSelectMenuComponents(string customId);
    
    bool IsModalMenu(string customId, out ModalCallback? callback);

    void UpdateExistingPartialCustomIds(string customId,
                                        string[] existingPartialCustomIds);
}