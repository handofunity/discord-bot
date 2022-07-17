namespace HoU.GuildBot.BLL;

public class DiscordUserEventHandler : IDiscordUserEventHandler
{
    private readonly IUserStore _userStore;
    private readonly IPrivacyProvider _privacyProvider;
    private readonly INonMemberRoleProvider _nonMemberRoleProvider;
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly IDatabaseAccess _databaseAccess;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private IDiscordAccess? _discordAccess;
        
    public DiscordUserEventHandler(IUserStore userStore,
                                   IPrivacyProvider privacyProvider,
                                   INonMemberRoleProvider nonMemberRoleProvider,
                                   IGameRoleProvider gameRoleProvider,
                                   IDatabaseAccess databaseAccess,
                                   IDynamicConfiguration dynamicConfiguration)
    {
        _userStore = userStore;
        _privacyProvider = privacyProvider;
        _nonMemberRoleProvider = nonMemberRoleProvider;
        _gameRoleProvider = gameRoleProvider;
        _databaseAccess = databaseAccess;
        _dynamicConfiguration = dynamicConfiguration;
    }
        
    IDiscordAccess IDiscordUserEventHandler.DiscordAccess
    {
        set => _discordAccess = value;
    }

    void IDiscordUserEventHandler.HandleJoined(DiscordUserId userID,
                                               Role roles,
                                               DateTime joinedDate)
    {
        _ = Task.Run(async () =>
        {
            await _userStore.AddUserIfNewAsync(userID, roles);
            if (_userStore.TryGetUser(userID, out var user) && user is not null)
            {
                user.JoinedDate = joinedDate;
                await _databaseAccess.UpdateUserInformationAsync(new []{ user });
            }
        }).ConfigureAwait(false);
    }

    void IDiscordUserEventHandler.HandleLeft(DiscordUserId userID,
                                             string username,
                                             ushort discriminatorValue)
    {
        if(!_userStore.TryGetUser(userID, out var user) || user == null)
            return;
        _ = Task.Run(async () =>
        {
            var discordAccess = _discordAccess ?? throw new InvalidOperationException($"{nameof(IDiscordUserEventHandler.DiscordAccess)} not set.");
            await _userStore.RemoveUser(userID);
            await _privacyProvider.DeleteUserRelatedData(user!);
            var now = DateTime.UtcNow;
            // Only post to Discord log if the user was on the server for more than 10 minutes, or the time on the server cannot be determined.
            if ((now - user.JoinedDate).TotalMinutes > 10)
            {
                var mentionPrefix = string.Empty;
                if (user!.Roles != Role.NoRole
                 && user.Roles != Role.Guest
                 && user.Roles != Role.FriendOfMember)
                {
                    var leaderMention = discordAccess.GetRoleMention(Constants.RoleNames.LeaderRoleName);
                    var officerMention = discordAccess.GetRoleMention(Constants.RoleNames.OfficerRoleName);
                    mentionPrefix = $"{leaderMention} {officerMention}: ";
                }
                var formattedRolesMessage = user.CurrentRoles is null
                                                ? string.Empty
                                                : $"; Roles: {user.CurrentRoles}";
                await discordAccess.LogToDiscord(
                                                 $"{mentionPrefix}User `{username}#{discriminatorValue}` " +
                                                 $"(Membership level: **{user.Roles}**{formattedRolesMessage}) " +
                                                 $"has left the server on {now:D} at {now:HH:mm:ss} UTC.");
            }
            // If it has been less than 10 minutes, write to the #public-chat, so people will know that a new user left before greeting them.
            else
            {
                await discordAccess.CreateBotMessageInWelcomeChannel($"User `{username}#{discriminatorValue}` has left the server.");
            }
        }).ConfigureAwait(false);
    }

    UserRolesChangedResult IDiscordUserEventHandler.HandleRolesChanged(DiscordUserId userID, Role oldRoles, Role newRoles)
    {
        if (!_userStore.TryGetUser(userID, out var user))
            return new UserRolesChangedResult();
        user!.Roles = newRoles;

        // Check if the role change was a promotion
        Role promotedTo;
        if (!oldRoles.HasFlag(Role.TrialMember)
         && !oldRoles.HasFlag(Role.Member)
         && !oldRoles.HasFlag(Role.Coordinator)
         && !oldRoles.HasFlag(Role.Officer)
         && !oldRoles.HasFlag(Role.Leader)
         && newRoles.HasFlag(Role.TrialMember))
        {
            promotedTo = Role.TrialMember;
        }
        else
        {
            return new UserRolesChangedResult();
        }
            
        // Return result for announcement and logging the promotion
        var description = $"Congratulations {user.Mention}, you've been promoted to the rank **{promotedTo}**. Welcome aboard!";
        var a = new EmbedData
        {
            Title = "Promotion",
            Color = Colors.BrightBlue,
            Description = description
        };
        return new UserRolesChangedResult(a, $"{user.Mention} has been promoted to **{promotedTo}**.");
    }

    async Task IDiscordUserEventHandler.HandleRolesChanged(DiscordUserId userId,
                                                           string currentRoles)
    {
        if (!_userStore.TryGetUser(userId, out var user) || user == null)
            return;

        user.CurrentRoles = currentRoles;
        await _databaseAccess.UpdateUserInformationAsync(new []{ user });
    }

    async Task IDiscordUserEventHandler.HandleStatusChanged(DiscordUserId userID, bool wasOnline, bool isOnline)
    {
        if (!_userStore.TryGetUser(userID, out var user))
            return;

        // We're only updating the info when the user goes offline
        if (!(wasOnline && !isOnline))
            return; // If the user does not change from online to offline, we can return here

        // Only save status for guild members, not guests
        if (!user!.IsGuildMember)
            return;

        await _databaseAccess.UpdateUserInfoLastSeenAsync(user, DateTime.UtcNow);
    }

    async Task<string?> IDiscordUserEventHandler.HandleMessageComponentExecutedAsync(DiscordUserId userId,
                                                                                     string customId,
                                                                                     IReadOnlyCollection<string>? availableOptions,
                                                                                     IReadOnlyCollection<string> selectedValues)
    {
        if (customId == Constants.GameInterestMenu.CustomId || Constants.FriendOrGuestMenu.GetOptions().ContainsKey(customId))
        {
            // If the message is from the friend or guest menu, forward the data to the non-member role provider.
            return await _nonMemberRoleProvider.ToggleNonMemberRoleAsync(userId, customId, availableOptions, selectedValues);
        }

        // All other options require the availableOptions to be set.
        if (availableOptions == null)
            throw new ArgumentNullException(nameof(availableOptions),
                                            $"For the given {nameof(customId)} the {nameof(availableOptions)} must be set.");

        if (Constants.Menus.IsMappedToPrimaryGameRoleIdConfigurationKey(customId, out var primaryGameRoleIdConfigurationKey)
            && primaryGameRoleIdConfigurationKey != null)
        {
            // If the action is one of the primary game role actions, forward the data to the game role provider.
            var primaryGameDiscordRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[primaryGameRoleIdConfigurationKey];
            return await _gameRoleProvider.ToggleGameSpecificRolesAsync(userId,
                                                                        customId,
                                                                        _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId),
                                                                        availableOptions,
                                                                        selectedValues);
        }

        if (_gameRoleProvider.GamesRolesCustomIds.Contains(customId))
        {
            // If the message is one of the games roles menu messages, forward the data to the game role provider.
            var selectedGames = _gameRoleProvider.Games
                                                 .Where(m => selectedValues.Contains(m.PrimaryGameDiscordRoleId.ToString()))
                                                 .ToArray();
            var availableGames = _gameRoleProvider.Games
                                                  .Where(m => availableOptions.Contains(m.PrimaryGameDiscordRoleId.ToString()))
                                                  .ToArray();
            return await _gameRoleProvider.TogglePrimaryGameRolesAsync(userId, availableGames, selectedGames);
        }

        // CustomId is unknown.
        return null;
    }
}