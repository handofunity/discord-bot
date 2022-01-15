using System.Collections.Generic;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using System.Linq;

namespace HoU.GuildBot.BLL;

public class GuildInfoProvider : IGuildInfoProvider
{
    private readonly IUserStore _userStore;
    private readonly IDiscordAccess _discordAccess;
    private readonly IGameRoleProvider _gameRoleProvider;
    
    public GuildInfoProvider(IUserStore userStore,
                             IDiscordAccess discordAccess,
                             IGameRoleProvider gameRoleProvider)
    {
        _userStore = userStore;
        _discordAccess = discordAccess;
        _gameRoleProvider = gameRoleProvider;
    }
    
    EmbedData IGuildInfoProvider.GetGuildMemberStatus()
    {
        _discordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
        var guildMembers = _userStore.GetUsers(m => m.IsGuildMember);
        var total = guildMembers.Length;
        var online = guildMembers.Count(guildMember => _discordAccess.IsUserOnline(guildMember.DiscordUserId));
        var gamesToIncludeInGuildMemberStatus = _gameRoleProvider.Games
                                                                 .Where(m => m.IncludeInGuildMembersStatistic)
                                                                 .OrderBy(m => m.DisplayName)
                                                                 .ToList();

        var embedFields = new List<EmbedField>
        {
            new("Total", total.ToString(), true),
            new("Online", online.ToString(), true)
        };

        foreach (var game in gamesToIncludeInGuildMemberStatus)
        {
            // Total
            // ReSharper disable once PossibleInvalidOperationException
            var totalGameMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleId});
            embedFields.Add(new EmbedField(game.DisplayName + " (Total)", totalGameMembers.ToString(), false));

            // Intersected
            foreach (var otherGame in gamesToIncludeInGuildMemberStatus.Where(m => m.DisplayName != game.DisplayName
                                                                                && gamesToIncludeInGuildMemberStatus.IndexOf(m) > gamesToIncludeInGuildMemberStatus.IndexOf(game))
                                                                       .OrderBy(m => m.DisplayName))
            {
                // ReSharper disable once PossibleInvalidOperationException
                var intersectedMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleId, otherGame.PrimaryGameDiscordRoleId});
                embedFields.Add(new EmbedField(game.DisplayName + $" (also playing {otherGame.DisplayName})", intersectedMembers.ToString(), false));
            }

            // Disjunctive
            var disjunctiveMembers = _discordAccess.CountGuildMembersWithRoles(new[] {game.PrimaryGameDiscordRoleId},
                                                                               gamesToIncludeInGuildMemberStatus
                                                                                  .Where(m => m.DisplayName != game.DisplayName)
                                                                                   // ReSharper disable once PossibleInvalidOperationException
                                                                                  .Select(m => m.PrimaryGameDiscordRoleId)
                                                                                  .ToArray());
            embedFields.Add(new EmbedField(game.DisplayName + " (playing no other game)", disjunctiveMembers.ToString(), false));
        }

        // Not playing any game
        // ReSharper disable once PossibleInvalidOperationException
        var notPlaying = _discordAccess.CountGuildMembersWithRoles(null,
                                                                   gamesToIncludeInGuildMemberStatus.Select(m => m.PrimaryGameDiscordRoleId)
                                                                      .ToArray());
        embedFields.Add(new EmbedField("Not playing any of the games above", notPlaying.ToString(), false));

        return new EmbedData
        {
            Title = "Guild members",
            Color = Colors.LightGreen,
            Fields = embedFields.ToArray()
        };
    }
}