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
                                               IReadOnlyCollection<DiscordRoleId> selectedValues,
                                               RoleToggleMode roleToggleMode);

    Task<string?> TogglePrimaryGameRolesAsync(DiscordUserId userId,
                                              AvailableGame[] selectedGames,
                                              RoleToggleMode roleToggleMode);
}