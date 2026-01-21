namespace HoU.GuildBot.BLL;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GameRoleProvider : IGameRoleProvider
{
    private readonly IDatabaseAccess _databaseAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<GameRoleProvider> _logger;
    private readonly List<AvailableGame> _games;

    private IDiscordAccess? _discordAccess;

    public GameRoleProvider(IDatabaseAccess databaseAccess,
                            IDynamicConfiguration dynamicConfiguration,
                            ILogger<GameRoleProvider> logger)
    {
        _databaseAccess = databaseAccess;
        _dynamicConfiguration = dynamicConfiguration;
        _logger = logger;
        _games = new List<AvailableGame>();
    }

    public event EventHandler<GameChangedEventArgs>? GameChanged;

    public IDiscordAccess DiscordAccess
    {
        set => _discordAccess = value;
        private get => _discordAccess ?? throw new InvalidOperationException();
    }

    public IReadOnlyList<AvailableGame> Games => _games;

    IReadOnlyList<EmbedData> IGameRoleProvider.GetGameInfoAsEmbedData(string? filter)
    {
        var result = new List<EmbedData>();
        var caseInsensitiveFilter = filter?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(caseInsensitiveFilter))
            caseInsensitiveFilter = null;

        DiscordAccess.EnsureDisplayNamesAreSet(_games);
        foreach (var game in _games.OrderBy(m => m.DisplayName))
        {
            if (caseInsensitiveFilter != null
             && !game.DisplayName!.ToLowerInvariant().Contains(caseInsensitiveFilter))
            {
                continue;
            }

            var fields = new List<EmbedField>
            {
                // Primary game role ID
                new(nameof(AvailableGame.PrimaryGameDiscordRoleId), game.PrimaryGameDiscordRoleId, false),

                // Flags
                new(nameof(AvailableGame.IncludeInGuildMembersStatistic), game.IncludeInGuildMembersStatistic, false),
                new(nameof(AvailableGame.IncludeInGamesMenu), game.IncludeInGamesMenu, false),

                // Game interest
                new(nameof(AvailableGame.GameInterestRoleId), game.GameInterestRoleId?.ToString() ?? "<null>", false)
            };

            // Game role IDs
            if (game.AvailableRoles.Count > 0)
            {
                fields.AddRange(game.AvailableRoles
                                    .OrderBy(m => m.DisplayName)
                                    .Select(m => new EmbedField($"Game role '{m.DisplayName}'",
                                                                $"DiscordRoleID: {m.DiscordRoleId}",
                                                                false)));
            }

            if (fields.Count > 25)
            {
                var chunks = fields.Chunk(25).ToArray();
                for (var i = 1; i <= chunks.Length; i++)
                {
                    var chunk = chunks[i - 1];
                    var ed = new EmbedData
                    {
                        Color = Colors.LightGreen,
                        Title = $"Game information for \"{game.DisplayName}\" ({i}/{chunks.Length})",
                        Fields = chunk
                    };
                    result.Add(ed);
                }
            }
            else
            {
                var ed = new EmbedData
                {
                    Color = Colors.LightGreen,
                    Title = $"Game information for \"{game.DisplayName}\"",
                    Fields = fields.ToArray()
                };
                result.Add(ed);
            }
        }

        return result;
    }

    async Task IGameRoleProvider.LoadAvailableGames()
    {
        _games.Clear();
        var games = await _databaseAccess.GetAvailableGamesAsync();
        DiscordAccess.EnsureDisplayNamesAreSet(games);
        foreach (var game in games.Where(m => m.AvailableRoles.Any()))
            DiscordAccess.EnsureDisplayNamesAreSet(game.AvailableRoles);
        _games.AddRange(games);
        _logger.LogInformation("Loaded {Games} games with a total count of {GameRoles} roles",
                               _games.Count,
                               _games.Sum(m => m.AvailableRoles.Count));
    }

    (int GameMembers, Dictionary<string, int> RoleDistribution) IGameRoleProvider.GetGameRoleDistribution(AvailableGame game)
    {
        DiscordAccess.EnsureDisplayNamesAreSet(new[] { game });
        DiscordAccess.EnsureDisplayNamesAreSet(game.AvailableRoles);
        var distribution = new Dictionary<string, int>();
        foreach (var role in game.AvailableRoles)
        {
            var count = DiscordAccess.CountGuildMembersWithRoles(new[] { role.DiscordRoleId });
            distribution.Add(role.DisplayName!, count);
        }

        var gameMembers = DiscordAccess.CountGuildMembersWithRoles(new[] { game.PrimaryGameDiscordRoleId });

        return (gameMembers, distribution);
    }

    async Task<(bool Success, string Message, AvailableGame? AddedGame)> IGameRoleProvider.AddGameAsync(InternalUserId userID,
        DiscordRoleId primaryGameDiscordRoleID)
    {
        // Precondition
        if (_games.Any(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleID))
            return (false, $"Game with the primary game Discord role Id `{primaryGameDiscordRoleID}` already exists.", null);

        // Act
        var (success, error) = await _databaseAccess.TryAddGameAsync(userID, primaryGameDiscordRoleID);
        if (!success)
            return (false, $"Failed to add the game: {error}", null);

        // Update cache
        var newGame = new AvailableGame
        {
            PrimaryGameDiscordRoleId = primaryGameDiscordRoleID
        };
        _games.Add(newGame);
        DiscordAccess.EnsureDisplayNamesAreSet(_games);

        GameChanged?.Invoke(this, new GameChangedEventArgs(newGame, GameModification.Added));

        return (true, "The game was added successfully.", newGame);
    }

    async Task<(bool Success, string Message, AvailableGame? UpdatedGame)> IGameRoleProvider.UpdateGameAsync(InternalUserId userID,
        DiscordRoleId primaryGameDiscordRoleId,
        Action<AvailableGame> update)
    {
        // Precondition
        var internalGameId = await _databaseAccess.TryGetInternalGameIdAsync(primaryGameDiscordRoleId);
        if (internalGameId == null)
            return (false, $"Couldn't find any game with the primary game Discord role Id `{primaryGameDiscordRoleId}` in the database.",
                    null);
        var cachedGame = _games.SingleOrDefault(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId);
        if (cachedGame == null)
            return (false, $"Couldn't find any game with the primary game Discord role Id `{primaryGameDiscordRoleId}` in the cache.",
                    null);

        // Act
        var clone = cachedGame.Clone();
        update(clone);

        var (success, error) = await _databaseAccess.TryUpdateGameAsync(userID, internalGameId.Value, clone);
        if (!success) return (false, $"Failed to edit the game: {error}", null);

        // Update cache
        cachedGame.IncludeInGuildMembersStatistic = clone.IncludeInGuildMembersStatistic;
        cachedGame.IncludeInGamesMenu = clone.IncludeInGamesMenu;
        cachedGame.GameInterestRoleId = clone.GameInterestRoleId;

        GameChanged?.Invoke(this, new GameChangedEventArgs(cachedGame, GameModification.Edited));

        return (true, "The game was edited successfully.", cachedGame);
    }

    async Task<(bool Success, string Message, AvailableGame? RemovedGame)> IGameRoleProvider.RemoveGameAsync(
        DiscordRoleId primaryGameDiscordRoleID)
    {
        // Precondition
        var internalGameId = await _databaseAccess.TryGetInternalGameIdAsync(primaryGameDiscordRoleID);
        if (internalGameId == null)
            return (false, $"Couldn't find any game with the primary game Discord role Id `{primaryGameDiscordRoleID}` in the database.",
                    null);
        var cachedGame = _games.SingleOrDefault(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleID);
        if (cachedGame == null)
            return (false, $"Couldn't find any game with the primary game Discord role Id `{primaryGameDiscordRoleID}` in the cache.",
                    null);

        // Act
        var (success, error) = await _databaseAccess.TryRemoveGameAsync(internalGameId.Value);
        if (!success)
            return (false, $"Failed to remove the game: {error}", null);

        // Update cache
        _games.Remove(cachedGame);
        GameChanged?.Invoke(this, new GameChangedEventArgs(cachedGame, GameModification.Removed));
        return (true, "The game was removed successfully.", cachedGame);
    }

    async Task<(bool Success, string Message, AvailableGameRole? AddedGameRole)> IGameRoleProvider.AddGameRoleAsync(InternalUserId userID,
        DiscordRoleId primaryGameDiscordRoleID,
        DiscordRoleId discordRoleID)
    {
        // Precondition
        var internalGameId = await _databaseAccess.TryGetInternalGameIdAsync(primaryGameDiscordRoleID);
        if (internalGameId == null)
            return (false,
                    $"Couldn't find any game with the primary game Discord role Id `{primaryGameDiscordRoleID}`.",
                    null);

        // Act
        var (success, error) = await _databaseAccess.TryAddGameRoleAsync(userID, internalGameId.Value, discordRoleID);
        if (!success)
            return (false,
                    $"Failed to add the game role: {error}",
                    null);

        // Update cache
        var game = _games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleID);
        var role = new AvailableGameRole
        {
            DiscordRoleId = discordRoleID
        };
        game.AvailableRoles.Add(role);

        DiscordAccess.EnsureDisplayNamesAreSet(game.AvailableRoles);
        GameChanged?.Invoke(this, new GameChangedEventArgs(game, GameModification.RoleAdded));

        return (true, "The game role was added successfully.", role);
    }

    async Task<(bool Success, string Message, AvailableGameRole? RemovedGameRole)> IGameRoleProvider.RemoveGameRoleAsync(
        DiscordRoleId discordRoleId)
    {
        // Precondition
        var gameRole = await _databaseAccess.TryGetInternalGameRoleIdAsync(discordRoleId);
        if (gameRole == null)
            return (false, $"Couldn't find any game role with the Discord role ID `{discordRoleId}` in the database.", null);
        var cachedGameRole = _games.SelectMany(m => m.AvailableRoles).SingleOrDefault(m => m.DiscordRoleId == discordRoleId);
        if (cachedGameRole == null)
            return (false, $"Couldn't find any game role with the Discord role ID `{discordRoleId}` in the cache.", null);
        DiscordAccess.EnsureDisplayNamesAreSet(new[] { cachedGameRole });

        // Act
        var (success, error) = await _databaseAccess.TryRemoveGameRoleAsync(gameRole.Value);
        if (!success) return (false, $"Failed to remove the game role: {error}", null);

        // Update cache
        var gameWithCachedRole = _games.Single(m => m.AvailableRoles.Contains(cachedGameRole));
        gameWithCachedRole.AvailableRoles.Remove(cachedGameRole);

        GameChanged?.Invoke(this, new GameChangedEventArgs(gameWithCachedRole, GameModification.RoleRemoved));

        return (true, "The game role was removed successfully.", cachedGameRole);
    }

    async Task<string?> IGameRoleProvider.ToggleGameSpecificRolesAsync(DiscordUserId userId,
                                                                       string customId,
                                                                       AvailableGame game,
                                                                       IReadOnlyCollection<string> selectedMenuValues,
                                                                       RoleToggleMode roleToggleMode)
    {
        if (!DiscordAccess.CanManageRolesForUser(userId))
            return "The bot is not allowed to change your roles.";

        var sb = new StringBuilder();

        DiscordAccess.EnsureDisplayNamesAreSet(new[] { game });
        DiscordAccess.EnsureDisplayNamesAreSet(game.AvailableRoles);

        var selectedAndValidRoleIds = selectedMenuValues.Select(selectedMenuValue => _dynamicConfiguration.DiscordMapping
                .TryGetValue($"{customId}___{selectedMenuValue}", out var roleId)
                    ? (DiscordRoleId)roleId
                    : DiscordRoleId.Unknown)
            .Where(m => m != DiscordRoleId.Unknown && game.AvailableRoles.Any(r => r.DiscordRoleId == m))
            .ToArray();
        var currentUserRoleIds = DiscordAccess.GetUserRoles(userId);

        switch (roleToggleMode)
        {
            case RoleToggleMode.Assign:
            {
                var rolesToAdd = selectedAndValidRoleIds.Except(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToAdd)
                {
                    var roleDisplayName = game.AvailableRoles.Single(r => r.DiscordRoleId == discordRoleId).DisplayName;
                    var success = await DiscordAccess.TryAssignRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully assigned the role **{roleDisplayName}**."
                                      : $"Failed to assign the role **{roleDisplayName}**.");
                }
                break;
            }
            case RoleToggleMode.Revoke:
            {
                var rolesToRemove = selectedAndValidRoleIds.Intersect(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToRemove)
                {
                    var roleDisplayName = game.AvailableRoles.Single(r => r.DiscordRoleId == discordRoleId).DisplayName;
                    var success = await DiscordAccess.TryRevokeRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully revoked the role **{roleDisplayName}**."
                                      : $"Failed to revoke the role **{roleDisplayName}**.");
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(roleToggleMode), roleToggleMode, null);
        }

        return sb.Length == 0
                   ? "No roles were assigned or revoked."
                   : sb.ToString();
    }

    async Task<string?> IGameRoleProvider.TogglePrimaryGameRolesAsync(DiscordUserId userId,
                                                                      AvailableGame[] selectedGames,
                                                                      RoleToggleMode roleToggleMode)
    {
        if (!DiscordAccess.CanManageRolesForUser(userId))
            return "The bot is not allowed to change your roles.";

        var sb = new StringBuilder();

        DiscordAccess.EnsureDisplayNamesAreSet(selectedGames);

        var selectedGameRoleIds = selectedGames.Select(m => m.PrimaryGameDiscordRoleId).ToArray();
        var currentUserRoleIds = DiscordAccess.GetUserRoles(userId);

        switch (roleToggleMode)
        {
            case RoleToggleMode.Assign:
            {
                var rolesToAdd = selectedGameRoleIds.Except(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToAdd)
                {
                    var roleDisplayName = selectedGames.Single(r => r.PrimaryGameDiscordRoleId == discordRoleId).DisplayName;
                    var success = await DiscordAccess.TryAssignRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully assigned the game role **{roleDisplayName}**."
                                      : $"Failed to assign the game role **{roleDisplayName}**.");
                }
                break;
            }
            case RoleToggleMode.Revoke:
            {
                var rolesToRemove = selectedGameRoleIds.Intersect(currentUserRoleIds).ToArray();
                foreach (var discordRoleId in rolesToRemove)
                {
                    var roleDisplayName = selectedGames.Single(r => r.PrimaryGameDiscordRoleId == discordRoleId).DisplayName;
                    var success = await DiscordAccess.TryRevokeRoleAsync(userId, discordRoleId);
                    sb.AppendLine(success
                                      ? $"Successfully revoked the game role **{roleDisplayName}**."
                                      : $"Failed to revoke the game role **{roleDisplayName}**.");
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(roleToggleMode), roleToggleMode, null);
        }

        return sb.Length == 0
                   ? "No roles were assigned or revoked."
                   : sb.ToString();
    }
}