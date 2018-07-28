namespace HoU.GuildBot.BLL
{
    using Shared.Objects;
    using Shared.BLL;
    using Shared.DAL;
    using System.Linq;

    public class GuildInfoProvider : IGuildInfoProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserStore _userStore;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildInfoProvider(IUserStore userStore,
                                 IDiscordAccess discordAccess)
        {
            _userStore = userStore;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildInfoProvider Members

        EmbedData IGuildInfoProvider.GetGuildMemberStatus()
        {
            var guildMembers = _userStore.GetUsers(m => m.IsGuildMember);
            var total = guildMembers.Length;
            var online = guildMembers.Count(guildMember => _discordAccess.IsUserOnline(guildMember.DiscordUserID));

            return new EmbedData
            {
                Title = "Guild members",
                Color = Colors.LightGreen,
                Fields = new[]
                {
                    new EmbedField("Total", total.ToString(), true),
                    new EmbedField("Online", online.ToString(), true)
                }
            };
        }

        #endregion
    }
}