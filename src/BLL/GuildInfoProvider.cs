namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
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
        private readonly IGameRoleProvider _gameRoleProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildInfoProvider(IUserStore userStore,
                                 IDiscordAccess discordAccess,
                                 IGameRoleProvider gameRoleProvider)
        {
            _userStore = userStore;
            _discordAccess = discordAccess;
            _gameRoleProvider = gameRoleProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildInfoProvider Members

        EmbedData IGuildInfoProvider.GetGuildMemberStatus()
        {
            var guildMembers = _userStore.GetUsers(m => m.IsGuildMember);
            var total = guildMembers.Length;
            var online = guildMembers.Count(guildMember => _discordAccess.IsUserOnline(guildMember.DiscordUserID));
            var gamesToIncludeInGuildMemberStatus = _gameRoleProvider.Games
                                                                     .Where(m => m.IncludeInGuildMembersStatistic)
                                                                     .OrderBy(m => m.LongName)
                                                                     .ToArray();

            var embedFields = new List<EmbedField>
            {
                new EmbedField("Total", total.ToString(), true),
                new EmbedField("Online", online.ToString(), true)
            };

            foreach (var game in gamesToIncludeInGuildMemberStatus)
            {
                if (game.PrimaryGameDiscordRoleID == null)
                    continue;
                var count = _discordAccess.CountGuildMembersWithRole(game.PrimaryGameDiscordRoleID.Value);
                embedFields.Add(new EmbedField(game.LongName, count.ToString(), false));
            }

            return new EmbedData
            {
                Title = "Guild members",
                Color = Colors.LightGreen,
                Fields = embedFields.ToArray()
            };
        }

        #endregion
    }
}