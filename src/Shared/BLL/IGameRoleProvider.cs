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

        IReadOnlyList<EmbedData> GetGameInfoAsEmbedData();

        Task SetGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji);

        Task RevokeGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, string emoji);

        Task LoadAvailableGames();

        (int GameMembers, Dictionary<string, int> RoleDistribution) GetGameRoleDistribution(AvailableGame game);

        Task<(bool Success, string Message)> AddGame(InternalUserID userID,
                                                     string gameLongName,
                                                     string gameShortName);

        Task<(bool Success, string Message, string OldValue)> EditGame(InternalUserID userID,
                                                                       string gameShortName,
                                                                       string property,
                                                                       string newValue);

        Task<(bool Success, string Message)> RemoveGame(string gameShortName);

        Task<(bool Success, string Message)> AddGameRole(InternalUserID userID,
                                                         string gameShortName,
                                                         string roleName,
                                                         ulong discordRoleID);

        Task<(bool Success, string Message, string OldRoleName)> EditGameRole(InternalUserID userID,
                                                                              ulong discordRoleID,
                                                                              string newRoleName);

        Task<(bool Success, string Message, string OldRoleName)> RemoveGameRole(ulong discordRoleID);
    }
}