using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL;

public class NonMemberRoleProvider : INonMemberRoleProvider
{
    private readonly IUserStore _userStore;
    private readonly IGameRoleProvider _gameRoleProvider;
    private IDiscordAccess? _discordAccess;

    public NonMemberRoleProvider(IUserStore userStore,
                                 IGameRoleProvider gameRoleProvider)
    {
        _userStore = userStore;
        _gameRoleProvider = gameRoleProvider;
    }

    private static Role MapCustomIdToStaticRole(string customId) =>
        customId switch
        {
            Constants.FriendOrGuestMenu.FriendOfMemberCustomId => Role.FriendOfMember,
            Constants.FriendOrGuestMenu.GuestCustomId => Role.Guest,
            Constants.GameInterestMenu.CustomId => Role.NoRole,
            _ => throw new ArgumentOutOfRangeException(nameof(customId))
        };

    private bool IsGameInterestRole(DiscordRoleId roleId) =>
        _gameRoleProvider.Games
                         .Any(m => m.GameInterestRoleId != null && m.GameInterestRoleId == roleId);

    public IDiscordAccess DiscordAccess
    {
        set => _discordAccess = value;
        private get => _discordAccess ?? throw new InvalidOperationException();
    }

    async Task<string> INonMemberRoleProvider.ToggleNonMemberRoleAsync(DiscordUserId userId,
                                                                       string customId,
                                                                       IReadOnlyCollection<string> values)
    {
        if (!_userStore.TryGetUser(userId, out var user))
            return "Couldn't find user to edit.";
        if (!DiscordAccess.CanManageRolesForUser(userId))
            return "The bot is not allowed to change your roles.";

        var staticRole = MapCustomIdToStaticRole(customId);
        if (staticRole == Role.NoRole)
        {
            var sb = new StringBuilder();

            foreach (var selectedValue in values)
            {
                if (!ulong.TryParse(selectedValue, out var selectedRoleId))
                    continue;

                var roleId = (DiscordRoleId)selectedRoleId;
                if (IsGameInterestRole(roleId) == false)
                    continue;

                var (success, roleName) = await DiscordAccess.TryAddNonMemberRole(userId, roleId);
                if (success)
                {
                    await DiscordAccess.LogToDiscord($"User {user!.Mention} **added** the role **_{roleName}_**.");
                    sb.AppendLine($"The role **_{roleName}_** was **added**.");
                }
            }

            return sb.Length > 0
                       ? sb.ToString()
                       : "No roles were added.";
        }

        var added = await DiscordAccess.TryAddNonMemberRole(userId, staticRole);
        if (!added)
            return "Failed to identify matching role.";

        await DiscordAccess.LogToDiscord($"User {user!.Mention} **added** the role **_{staticRole}_**.");
        return $"The role **_{staticRole}_** was **added**.";

    }
}