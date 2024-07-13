namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class UserInfoProvider : IUserInfoProvider
{
    private readonly IUserStore _userStore;
    private readonly IDiscordAccess _discordAccess;
    private readonly IDatabaseAccess _databaseAccess;

    public UserInfoProvider(IUserStore userStore,
                            IDiscordAccess discordAccess,
                            IDatabaseAccess databaseAccess)
    {
        _userStore = userStore;
        _discordAccess = discordAccess;
        _databaseAccess = databaseAccess;
    }

    private EmbedData CreateEmbedDataForWhoIs(User user)
    {
        var displayName = _discordAccess.GetUserDisplayNames([user.DiscordUserId])[user.DiscordUserId];
        var avatarId = _discordAccess.GetAvatarId(user.DiscordUserId);
        return new EmbedData
        {
            Color = Colors.LightGreen,
            Title = $"\"Who is\" information about {displayName}",
            Fields = new[]
            {
                new EmbedField("DiscordUserId", user.DiscordUserId, true),
                new EmbedField("DiscordAvatarID", avatarId ?? "<null>", true),
                new EmbedField("InternalUserId", user.InternalUserId, false),
                new EmbedField("Is Guild Member", user.IsGuildMember, false),
                new EmbedField("Bot Permission Roles", user.Roles, false),
            }
        };
    }

    private static EmbedData CreateEmbedDataForWhoIsUnknownUser()
    {
        return new EmbedData
        {
            Title = "Unknown user",
            Color = Colors.Red,
            Description = "No user found matching the given ID."
        };
    }

    async Task<string[]> IUserInfoProvider.GetLastSeenInfo()
    {
        var ids = _userStore.GetUsers(m => m.IsGuildMember).Select(m => m.DiscordUserId).ToArray();
        // LastSeen = null equals the user is currently online
        var data = new List<(DiscordUserId UserID, string Username, bool IsOnline, DateTime? LastSeen)>(ids.Length);

        // Fetch all user names
        var userDisplayNames = _discordAccess.GetUserDisplayNames(ids);

        // Fetch data for online members
        foreach (var userID in ids)
        {
            var isOnline = _discordAccess.IsUserOnline(userID);
            if (isOnline)
                data.Add((userID, userDisplayNames[userID], true, null));
        }

        // Fetch data for offline members
        var missingUserIDs = ids.Except(data.Select(m => m.UserID)).ToArray();
        var missingUsers = new List<User>();
        foreach (var discordUserID in missingUserIDs)
        {
            if (!_userStore.TryGetUser(discordUserID, out var user))
                continue;
            missingUsers.Add(user!);
        }
        var lastSeenData = await _databaseAccess.GetLastSeenInfoForUsersAsync(missingUsers.ToArray());
        var noInfoFallback = new DateTime(2018, 1, 1);
        foreach (var (userId, lastSeen) in lastSeenData)
        {
            var user = missingUsers.Single(m => m.InternalUserId == userId);
            data.Add((user.DiscordUserId, userDisplayNames[user.DiscordUserId], false, lastSeen ?? (DateTime?)noInfoFallback));
        }

        // Format
        var result =
            data.OrderByDescending(m => m.IsOnline)
                .ThenByDescending(m => m.LastSeen)
                .ThenBy(m => m.Username)
                .Select(m => $"{(m.IsOnline? "Online" : m.LastSeen?.ToString("yyyy-MM-dd HH:mm") ?? "<UNKNOWN>")}: {m.Username}")
                .ToArray();

        return await Task.FromResult(result);
    }

    EmbedData IUserInfoProvider.WhoIs(DiscordUserId userID)
    {
        return _userStore.TryGetUser(userID, out var user)
                   ? CreateEmbedDataForWhoIs(user!)
                   : CreateEmbedDataForWhoIsUnknownUser();
    }

    EmbedData IUserInfoProvider.WhoIs(InternalUserId userID)
    {
        return _userStore.TryGetUser(userID, out var user)
                   ? CreateEmbedDataForWhoIs(user!)
                   : CreateEmbedDataForWhoIsUnknownUser();
    }
}