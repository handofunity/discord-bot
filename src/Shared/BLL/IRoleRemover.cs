namespace HoU.GuildBot.Shared.BLL;

public interface IRoleRemover
{
    Task RemoveBasementRolesAsync();

    Task RemoveStaleTrialMembersAsync();
}