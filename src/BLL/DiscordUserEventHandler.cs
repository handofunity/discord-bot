namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading.Tasks;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;
    using Shared.StrongTypes;

    public class DiscordUserEventHandler : IDiscordUserEventHandler
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IDatabaseAccess _databaseAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordUserEventHandler(IUserStore userStore,
                                       IDatabaseAccess databaseAccess)
        {
            _userStore = userStore;
            _databaseAccess = databaseAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordUserEventHandler Members

        UserRolesChangedResult IDiscordUserEventHandler.HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles)
        {
            var user = _userStore.GetUser(userID);
            user.Roles = newRoles;

            // Check if the role change was a promotion
            Role promotedTo;
            if (!oldRoles.HasFlag(Role.Recruit)
             && !oldRoles.HasFlag(Role.Member)
             && !oldRoles.HasFlag(Role.Officer)
             && !oldRoles.HasFlag(Role.SeniorOfficer)
             && !oldRoles.HasFlag(Role.Leader)
             && newRoles.HasFlag(Role.Recruit))
            {
                promotedTo = Role.Recruit;
            }
            else
            {
                return new UserRolesChangedResult();
            }
            
            // Return result for announcement and logging the promotion
            var description = $"Congratulations {user.Mention}, you've been promoted to the rank **{promotedTo}**.";
            if (promotedTo == Role.Recruit)
                description += " Welcome aboard!";
            var a = new EmbedData
            {
                Title = "Promotion",
                Color = Colors.BrightBlue,
                Description = description
            };
            return new UserRolesChangedResult(a, $"{user.Mention} has been promoted to **{promotedTo}**.");
        }

        async Task IDiscordUserEventHandler.HandleStatusChanged(DiscordUserID userID, bool wasOnline, bool isOnline)
        {
            var user = _userStore.GetUser(userID);

            // We're only updating the info when the user goes offline
            if (!(wasOnline && !isOnline))
                return; // If the user does not change from online to offline, we can return here

            // Only save status for guild members, not guests
            if (!user.IsGuildMember)
                return;

            await _databaseAccess.UpdateUserInfoLastSeen(user, DateTime.UtcNow).ConfigureAwait(false);
        }

        #endregion
    }
}