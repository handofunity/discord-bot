using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL;

public interface IGameRoleProvider
{
    event EventHandler<GameChangedEventArgs> GameChanged;

    IDiscordAccess DiscordAccess { set; }

    IReadOnlyList<AvailableGame> Games { get; }

    string[] GamesRolesCustomIds { get; set; }

    IReadOnlyList<EmbedData> GetGameInfoAsEmbedData(string? filter);

    Task LoadAvailableGames();

    (int GameMembers, Dictionary<string, int> RoleDistribution) GetGameRoleDistribution(AvailableGame game);

    Task<(bool Success, string Message, AvailableGame? AddedGame)> AddGameAsync(InternalUserId userID,
                                                                                DiscordRoleId primaryGameDiscordRoleId);

    Task<(bool Success, string Message, AvailableGame? UpdatedGame)> UpdateGameAsync(InternalUserId userID,
                                                                                     DiscordRoleId primaryGameDiscordRoleId,
                                                                                     Action<AvailableGame> update);

    Task<(bool Success, string Message, AvailableGame? RemovedGame)> RemoveGameAsync(DiscordRoleId primaryGameDiscordRoleId);

    Task<(bool Success, string Message, AvailableGameRole? AddedGameRole)> AddGameRoleAsync(InternalUserId userID,
                                                                                            DiscordRoleId primaryGameDiscordRoleId,
                                                                                            DiscordRoleId discordRoleID);

    Task<(bool Success, string Message, AvailableGameRole? RemovedGameRole)> RemoveGameRoleAsync(DiscordRoleId discordRoleID);

    Task<string?> ToggleGameSpecificRolesAsync(DiscordUserId userId,
                                               string customId,
                                               AvailableGame game,
                                               IReadOnlyCollection<string> availableOptions,
                                               IReadOnlyCollection<string> selectedValues);

    Task<string?> TogglePrimaryGameRolesAsync(DiscordUserId userId,
                                              AvailableGame[] availableGames,
                                              AvailableGame[] selectedGames);
}