namespace HoU.GuildBot.Shared.Objects
{
    using System.Threading;
    using Enums;
    using Extensions;
    using StrongTypes;

    public class User
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private int _roles;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public InternalUserID InternalUserID { get; }

        public DiscordUserID DiscordUserID { get; }

        public string Mention => DiscordUserID.ToMention();

        public Role Roles
        {
            get => (Role)_roles;
            set => Interlocked.Exchange(ref _roles, (int)value);
        }

        public bool IsGuildMember => (Role.AnyGuildMember & Roles) != Role.NoRole;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public User(InternalUserID internalUserID, DiscordUserID discordUserID)
        {
            InternalUserID = internalUserID;
            DiscordUserID = discordUserID;
        }

        #endregion
    }
}