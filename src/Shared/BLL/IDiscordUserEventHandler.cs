namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;
    using Enums;
    using Objects;
    using StrongTypes;

    public interface IDiscordUserEventHandler
    {
        IDiscordAccess DiscordAccess { set; }

        void HandleJoined(DiscordUserID userID, Role roles);

        void HandleLeft(DiscordUserID userID, string username);

        UserRolesChangedResult HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles);

        Task HandleStatusChanged(DiscordUserID userID, bool wasOnline, bool isOnline);
    }
}