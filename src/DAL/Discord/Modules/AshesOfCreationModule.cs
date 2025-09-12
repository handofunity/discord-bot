namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("aoc", "Ashes of Creation related commands.")]
public class AshesOfCreationModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string ModuleChapter = "aoc";
    private const string UnitsHouGuildRoleIdKey = "UnitsHouGuildRoleId";
    private const string UnitsFouGuildRoleIdKey = "UnitsFouGuildRoleId";
    private const string UnitsPvpRoleIdKey = "UnitsPvpRoleId";
    private const string UnitsArtisanRoleIdKey = "UnitsArtisanRoleId";
    private const string UnitsPveRoleIdKey = "UnitsPveRoleId";

    private readonly IImageProvider _imageProvider;
    private readonly IUnitsAccess _unitsAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<AshesOfCreationModule> _logger;
    private readonly IDatabaseAccess _databaseAccess;

    public AshesOfCreationModule(IImageProvider imageProvider,
                                 IUnitsAccess unitsAccess,
                                 IDynamicConfiguration dynamicConfiguration,
                                 ILogger<AshesOfCreationModule> logger,
                                 IDatabaseAccess databaseAccess)
    {
        _imageProvider = imageProvider;
        _unitsAccess = unitsAccess;
        _dynamicConfiguration = dynamicConfiguration;
        _logger = logger;
        _databaseAccess = databaseAccess;
    }

    [SlashCommand("profile", "Shows the profile image for the given or current user.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetProfileCardAsync(IGuildUser? guildUser = null)
    {
        await DeferAsync();
        guildUser ??= (IGuildUser)Context.User;
        var userId = (DiscordUserId)guildUser.Id;

        try
        {
            var endpoint = _dynamicConfiguration.UnitsEndpoints.FirstOrDefault(m => m.ConnectToRestApi
                && m.Chapter.Equals(ModuleChapter, StringComparison.InvariantCultureIgnoreCase));
            if (endpoint is null)
            {
                await FollowupAsync("No endpoint configured.");
                return;
            }

            var profileData = await _unitsAccess.GetProfileDataAsync(endpoint, userId);
            if (profileData is null)
            {
                await FollowupAsync("Failed to fetch profile data from UNITS.");
                return;
            }

            // Determine guild tag
            var guildTag = "N/A";
            var houRoleId = _dynamicConfiguration.DiscordMapping[UnitsHouGuildRoleIdKey];
            var fouRoleId = _dynamicConfiguration.DiscordMapping[UnitsFouGuildRoleIdKey];
            if (guildUser.RoleIds.Contains(houRoleId))
                guildTag = "HoU";
            else if (guildUser.RoleIds.Contains(fouRoleId))
                guildTag = "FoU";

            // Determine play style
            var pvpRoleId = _dynamicConfiguration.DiscordMapping[UnitsPvpRoleIdKey];
            var artisanRoleId = _dynamicConfiguration.DiscordMapping[UnitsArtisanRoleIdKey];
            var pveRoleId = _dynamicConfiguration.DiscordMapping[UnitsPveRoleIdKey];
            var hasPvpRole = guildUser.RoleIds.Contains(pvpRoleId);
            var hasArtisanRole = guildUser.RoleIds.Contains(artisanRoleId);
            var hasPveRole = guildUser.RoleIds.Contains(pveRoleId);

            await using var imageStream = await _imageProvider.CreateProfileCardImage(userId,
                                                                                      guildUser.GetAvatarUrl(ImageFormat.Png),
                                                                                      profileData,
                                                                                      guildTag,
                                                                                      hasPvpRole,
                                                                                      hasArtisanRole,
                                                                                      hasPveRole);
            await Context.Interaction.FollowupWithFileAsync(imageStream,
                                                            $"units_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create UNITS profile card image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("leaderboard", "Shows the leaderboard for the current season or the heritag ranking.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetLeaderboardTableAsync([Summary(description: "Which leaderboard to show")] LeadboardType leadboardType)
    {
        await DeferAsync();

        try
        {
            var endpoint = _dynamicConfiguration.UnitsEndpoints.FirstOrDefault(m => m.ConnectToRestApi
                && m.Chapter.Equals(ModuleChapter, StringComparison.InvariantCultureIgnoreCase));
            if (endpoint is null)
            {
                await FollowupAsync("No endpoint configured.");
                return;
            }

            DiscordLeaderboardResponse? leaderboardData;
            if (leadboardType == LeadboardType.CurrentSeasonLeaderboard)
            {
                leaderboardData = await _unitsAccess.GetCurrentSeasonLeaderboardAsync(endpoint);
            }
            else
            {
                leaderboardData = await _unitsAccess.GetHeritageLeaderboardAsync(endpoint);
            }
            if (leaderboardData is null)
            {
                await FollowupAsync("Failed to fetch leaderboard data from UNITS.");
                return;
            }

            await using var imageStream = _imageProvider.CreateLeaderboardTable(leaderboardData);
            await Context.Interaction.FollowupWithFileAsync(imageStream,
                                                            $"units_{leadboardType}_{DateTime.UtcNow:yyyyMMddHHmmss}.png",
                                                            $"View full leaderboard: {endpoint.BaseAddress}leaderboards");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create UNITS leaderboard table image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("archetype-combinations", "Shows an overview table of all possible archetype combinations in AoC.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetArchetypeCombinationsImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadClassListImage();
            await FollowupWithFileAsync(imageStream, "archetype-combinations.jpg");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create the archetype combinations image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("artisan-professions", "Shows an overview of all possible artisan professions in AoC.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetArtisanProfessionsImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadArtisanProfessionsImage();
            await FollowupWithFileAsync(imageStream, "artisan-professions.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create the artisan professions image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("launch-roster", "Shows the possibilities to get into the AoC launch roster.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetLaunchRosterImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadLaunchRosterImage();
            await FollowupWithFileAsync(imageStream, "launch-roster.jpg");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create launch roster image");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("chart", "Shows various charts related to AoC member roles.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetAocRolesChartAsync(ChartType chartType)
    {
        await DeferAsync();
        try
        {
            Func<Stream> getImage = chartType switch
            {
                ChartType.Classes => () => _imageProvider.CreateAocClassDistributionImage(),
                ChartType.Races => () => _imageProvider.CreateAocRaceDistributionImage(),
                ChartType.PlayStyles => () => _imageProvider.CreateAocPlayStyleDistributionImage(),
                ChartType.GuildPreference => () => _imageProvider.CreateAocGuildPreferenceDistributionImage(),
                ChartType.RolePreference => () => _imageProvider.CreateAocRolePreferenceDistributionImage(),
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported.")
            };
            await using var imageStream = getImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "chart.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create {ChartType} chart", chartType);
            await FollowupAsync("Failed to create chart.");
        }
    }

    [SlashCommand("units-auction-sync", "Creates the commands to sync UNITS to the Auction bot.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AocManagement)]
    public async Task GetUnitsToAuctionBotSyncCommandsAsync()
    {
        await DeferAsync();
        try
        {
            var endpoint = _dynamicConfiguration.UnitsEndpoints.FirstOrDefault(m => m.ConnectToRestApi
                && m.Chapter.Equals(ModuleChapter, StringComparison.InvariantCultureIgnoreCase));
            if (endpoint is null)
            {
                await FollowupAsync("No endpoint configured.");
                return;
            }

            var storedHeritageTokens = await _databaseAccess.GetLastHeritageTokensAsync();
            var newHeritageTokens = await _unitsAccess.GetHeritageTokensAsync(endpoint);
            if (newHeritageTokens is null)
            {
                await FollowupAsync("Failed to fetch heritage tokens from UNITS.");
                return;
            }

            var diff = new Dictionary<DiscordUserId, long>();
            foreach (var (userId, newTokenCount) in newHeritageTokens)
            {
                storedHeritageTokens.TryGetValue(userId, out var oldTokenCount);
                var difference = newTokenCount - oldTokenCount;
                if (difference != 0)
                    diff[userId] = difference;
            }

            var negativeDiff = diff.Where(m => m.Value < 0).ToList();
            var positiveGroupedDiff = diff.Where(m => m.Value > 0)
                .GroupBy(m => m.Value)
                .OrderByDescending(m => m.Key)
                .ToDictionary(m => m.Key,
                    m => m.Select(kvp => kvp.Key).ToArray());

            if (negativeDiff.Count == 0 && positiveGroupedDiff.Count == 0)
            {
                await FollowupAsync("No changes detected between stored and UNITS heritage tokens.");
                return;
            }

            var messages = new List<string>();
            var negativeMessages = new List<string>();
            foreach (var negativeEntry in negativeDiff)
            {
                negativeMessages.Add($"/remove-money amount:{negativeEntry.Value} user:{negativeEntry.Key.ToMention()} \n");
            }
            var negativeSplit = negativeMessages.SplitLongMessageWithList(17);
            foreach (var negativePat in negativeSplit)
            {
                messages.Add($"```plaintext\n{negativePat}```");
            }

            foreach (var positiveEntry in positiveGroupedDiff)
            {
                var items = positiveEntry.Value.Select(m => m.ToMention()).ToArray();
                var prefix = $"```plaintext\n/bulk add-money amount:{positiveEntry.Key} users:";
                var suffix = "\n```";
                var split = items.SplitLongMessageWithList(prefix.Length + suffix.Length);
                foreach (var part in split)
                {
                    messages.Add($"{prefix}{part}{suffix}");
                }
            }

            await FollowupAsync($"Use the following commands to sync heritage tokens from UNITS to the Auction bot. " +
                $"Please include any trailing whitespaces per line. Otherwise the last user will be dropped.");

            foreach (var message in messages)
            {
                await Task.Delay(500); // Small delay to avoid rate limits
                await FollowupAsync(message);
            }

            // Only save if we send the response successfully
            await _databaseAccess.PersistHeritageTokensAsync(newHeritageTokens);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create UNITS to Auction bot sync commands");
            await FollowupAsync("Failed to create commands. Do NOT execute of the commands generated!");
        }
    }

    /// <summary>
    /// Chart types that can be generated by <see cref="GetAocRolesChartAsync"/>.
    /// </summary>
    public enum ChartType
    {
        Classes = 1,
        Races = 2,
        [ChoiceDisplay("Play styles")]
        PlayStyles = 3,
        [ChoiceDisplay("Guild preference")]
        GuildPreference = 4,
        [ChoiceDisplay("Role preference")]
        RolePreference = 5
    }

    /// <summary>
    /// Leaderboard types that can be generated by <see cref="GetLeaderboardTableAsync"/>.
    /// </summary>
    public enum LeadboardType
    {
        [ChoiceDisplay("Current season")]
        CurrentSeasonLeaderboard = 1,
        [ChoiceDisplay("Heritage")]
        HeritageLeaderboard = 2
    }
}