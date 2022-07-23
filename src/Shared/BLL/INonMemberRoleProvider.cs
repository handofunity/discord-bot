namespace HoU.GuildBot.Shared.BLL;

public interface INonMemberRoleProvider
{
    IDiscordAccess DiscordAccess { set; }

    Task<string> ToggleNonMemberRoleAsync(DiscordUserId userId,
                                          string customId,
                                          IEnumerable<DiscordRoleId> selectedRoleIds,
                                          RoleToggleMode roleToggleMode);
}