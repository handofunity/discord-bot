using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IGameRoleProvider
    {
        event EventHandler<GameChangedEventArgs> GameChanged;

        IDiscordAccess DiscordAccess { set; }

        ulong[] AocGameRoleMenuMessageIDs { get; set; }

        ulong WowGameRoleMenuMessageID { get; set; }

        ulong[] NewWorldGameRoleMenuMessageIDs { get; set; }

        ulong[] GamesRolesMenuMessageIDs { get; set; }

        IReadOnlyList<AvailableGame> Games { get; }

        IReadOnlyList<EmbedData> GetGameInfoAsEmbedData([CanBeNull] string filter);

        Task SetGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, EmojiDefinition emoji);

        Task RevokeGameRole(DiscordChannelID channelID, DiscordUserID userID, AvailableGame game, EmojiDefinition emoji);

        Task SetPrimaryGameRole(DiscordChannelID channelID,
                                DiscordUserID userID,
                                AvailableGame game);

        Task RevokePrimaryGameRole(DiscordChannelID channelID,
                                   DiscordUserID userID,
                                   AvailableGame game);

        Task LoadAvailableGames();

        (int GameMembers, Dictionary<string, int> RoleDistribution) GetGameRoleDistribution(AvailableGame game);

        Task<(bool Success, string Message)> AddGame(InternalUserID userID,
                                                     string gameLongName,
                                                     string gameShortName,
                                                     ulong? primaryGameDiscordRoleID);

        Task<(bool Success, string Message, string OldValue, AvailableGame UpdatedGame)> EditGame(InternalUserID userID,
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