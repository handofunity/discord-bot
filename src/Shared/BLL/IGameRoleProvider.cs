namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Enums;

    public interface IGameRoleProvider
    {
        Task<(bool Success, string Response, string LogMessage)> SetGameRole((ulong UserID, string Mention) user, Game game, string className);
    }
}