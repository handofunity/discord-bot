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
                                                                     .Where(m => m.IncludeInGuildMembersStatistic && m.PrimaryGameDiscordRoleID != null)
                                                                     .OrderBy(m => m.LongName)
                                                                     .ToList();

            var embedFields = new List<EmbedField>
            {
                new EmbedField("Total", total.ToString(), true),
                new EmbedField("Online", online.ToString(), true)
            };

            foreach (var game in gamesToIncludeInGuildMemberStatus)
            {
                // Total
                // ReSharper disable once PossibleInvalidOperationException
                var totalGameMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleID.Value});
                embedFields.Add(new EmbedField(game.LongName + " (Total)", totalGameMembers.ToString(), false));

                // Intersected
                foreach (var otherGame in gamesToIncludeInGuildMemberStatus.Where(m => m.LongName != game.LongName
                                                                                       && gamesToIncludeInGuildMemberStatus.IndexOf(m) > gamesToIncludeInGuildMemberStatus.IndexOf(game))
                                                                           .OrderBy(m => m.LongName))
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    var intersectedMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleID.Value, otherGame.PrimaryGameDiscordRoleID.Value});
                    embedFields.Add(new EmbedField(game.LongName + $" (also playing {otherGame.LongName})", intersectedMembers.ToString(), false));
                }

                // Disjunctive
                var disjunctiveMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleID.Value},
                                                                                   gamesToIncludeInGuildMemberStatus
                                                                                      .Where(m => m.LongName != game.LongName)
                                                                                       // ReSharper disable once PossibleInvalidOperationException
                                                                                      .Select(m => m.PrimaryGameDiscordRoleID.Value)
                                                                                      .ToArray());
                embedFields.Add(new EmbedField(game.LongName + " (playing no other game)", disjunctiveMembers.ToString(), false));
            }

            // Not playing any game
            // ReSharper disable once PossibleInvalidOperationException
            var notPlaying = _discordAccess.CountGuildMembersWithRoles(null, gamesToIncludeInGuildMemberStatus.Select(m => m.PrimaryGameDiscordRoleID.Value).ToArray());
            embedFields.Add(new EmbedField("Not playing any of the games above", notPlaying.ToString(), false));

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