namespace HoU.GuildBot.Shared.BLL;

public interface INonMemberRoleProvider
{
    IDiscordAccess DiscordAccess { set; }

    Task<string> ToggleNonMemberRoleAsync(DiscordUserId userId,
                                          string customId,
                                          IReadOnlyCollection<string>? availableOptions,
                                          IReadOnlyCollection<string> selectedValues);
}