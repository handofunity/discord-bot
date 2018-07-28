namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Enums;
    using Objects;
    using StrongTypes;

    public interface IDiscordUserEventHandler
    {
        UserRolesChangedResult HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles);

        Task HandleStatusChanged(DiscordUserID userID, bool wasOnline, bool isOnline);
    }
}