namespace HoU.GuildBot.Shared.BLL
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DAL;
    using Objects;
    using StrongTypes;

    public interface IGameRoleProvider
    {
        IDiscordAccess DiscordAccess { set; }

        IReadOnlyList<AvailableGame> Games { get; }

        Task<(bool Success, string Response, string LogMessage)> SetGameRole((DiscordUserID UserID, string Mention) user, AvailableGame game, string className);

        Task LoadAvailableGames();
    }
}