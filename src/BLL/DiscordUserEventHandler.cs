namespace HoU.GuildBot.BLL;

public class DiscordUserEventHandler : IDiscordUserEventHandler
{
    private readonly IUserStore _userStore;
    private readonly IPrivacyProvider _privacyProvider;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDatabaseAccess _databaseAccess;
    private readonly IMenuRegistry _menuRegistry;
    private IDiscordAccess? _discordAccess;

    public DiscordUserEventHandler(IUserStore userStore,
                                   IPrivacyProvider privacyProvider,
                                   IDynamicConfiguration dynamicConfiguration,
                                   IDatabaseAccess databaseAccess,
                                   IMenuRegistry menuRegistry)
    {
        _userStore = userStore;
        _privacyProvider = privacyProvider;
        _dynamicConfiguration = dynamicConfiguration;
        _databaseAccess = databaseAccess;
        _menuRegistry = menuRegistry;
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

            var infosAndRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["InfoAndRolesChannelId"];
            var messageContent = $"Welcome {userID.ToMention()}! Please use the two menus above to assign yourself roles "
                               + "regarding your relationship to the guild and your game interests.";

            await Task.Delay(TimeSpan.FromSeconds(15));
            var messageIds = await _discordAccess!.CreateBotMessagesInChannelAsync(infosAndRolesChannelId,
                                                                                   new[] { messageContent });
            await Task.Delay(TimeSpan.FromMinutes(15));
            await _discordAccess!.DeleteBotMessageInChannelAsync(infosAndRolesChannelId, messageIds[0]);

        }).ConfigureAwait(false);
    }

    void IDiscordUserEventHandler.HandleLeft(DiscordUserId userID,
                                             string username)
    {
        if(!_userStore.TryGetUser(userID, out var user) || user == null)
            return;
        _ = Task.Run(async () =>
        {
            var discordAccess = _discordAccess ?? throw new InvalidOperationException($"{nameof(IDiscordUserEventHandler.DiscordAccess)} not set.");
            await _userStore.RemoveUser(userID);
            await _privacyProvider.DeleteUserRelatedData(user);
            var now = DateTime.UtcNow;
            // Only post to Discord log if the user was on the server for more than a day, or the time on the server cannot be determined.
            if (now.Date > user.JoinedDate)
            {
                var mentionPrefix = string.Empty;
                if (user.Roles != Role.NoRole
                 && user.Roles != Role.Guest
                 && user.Roles != Role.FriendOfMember
                 && user.Roles != Role.TnlFriend)
                {
                    var leaderMention = discordAccess.GetRoleMention(Constants.RoleNames.LeaderRoleName);
                    var officerMention = discordAccess.GetRoleMention(Constants.RoleNames.OfficerRoleName);
                    mentionPrefix = $"{leaderMention} {officerMention}: ";
                }
                var formattedRolesMessage = user.CurrentRoles is null
                                                ? string.Empty
                                                : $"; Roles: {user.CurrentRoles}";
                await discordAccess.LogToDiscordAsync(
                                                 $"{mentionPrefix}User `{username}` " +
                                                 $"(Membership level: **{user.Roles}**{formattedRolesMessage}) " +
                                                 $"has left the server on {now:D} at {now:HH:mm:ss} UTC.");
            }
            // If it has been less than a day, write to the #public-chat, so people will know that a new user left before greeting them.
            else
            {
                await discordAccess.CreateBotMessageInWelcomeChannelAsync($"User `{username}` has left the server.");
            }
        }).ConfigureAwait(false);
    }

    async Task<UserRolesChangedResult> IDiscordUserEventHandler.HandleRolesChanged(DiscordUserId userID, Role oldRoles, Role newRoles)
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
        else if (oldRoles.HasFlag(Role.TrialMember)
              && !newRoles.HasFlag(Role.TrialMember))
        {
            // Discard promotion date in memory and database
            user.PromotedToTrialMemberDate = null;
            await _databaseAccess.UpdateUserInfoPromotionToTrialMemberDateAsync(user);
            return new UserRolesChangedResult();
        }
        else
        {
            return new UserRolesChangedResult();
        }

        // Persist promotion date in memory and database
        user.PromotedToTrialMemberDate = DateOnly.FromDateTime(DateTime.Today);
        await _databaseAccess.UpdateUserInfoPromotionToTrialMemberDateAsync(user);

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
                                                                                     IReadOnlyCollection<string> selectedValues)
    {
        if (_menuRegistry.IsButtonMenu(customId, out var buttonCallback))
            return await buttonCallback!(userId, customId);

        if (_menuRegistry.IsSelectMenu(customId, out var selectCallback))
            return await selectCallback!(userId, customId, selectedValues);

        // CustomId is unknown.
        return null;
    }

    async Task<string?> IDiscordUserEventHandler.HandleModalSubmittedAsync(ModalResponse response)
    {
        if (_menuRegistry.IsModalMenu(response.CustomId, out var modalCallback))
            return await modalCallback!(response);

        // CustomId is unknown.
        return null;
    }
}