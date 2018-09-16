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

        ulong AocGameRoleMenuMessageID { get; set; }

        IReadOnlyList<AvailableGame> Games { get; }

        Task SetGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji);

        Task RevokeGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji);

        Task LoadAvailableGames();

        Dictionary<string, int> GetGameRoleDistribution(AvailableGame game);
    }
}