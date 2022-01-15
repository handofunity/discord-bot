using System.Collections.Generic;
using System.Diagnostics;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

[DebuggerDisplay("{" + nameof(PrimaryGameDiscordRoleId) + "(),nq}")]
public class AvailableGame
{
    public DiscordRoleId PrimaryGameDiscordRoleId { get; init; }

    public bool IncludeInGuildMembersStatistic { get; set; }

    public bool IncludeInGamesMenu { get; set; }
        
    public DiscordRoleId? GameInterestRoleId { get; set; }

    public List<AvailableGameRole> AvailableRoles { get; }

    public string? DisplayName { get; set; }

    public AvailableGame()
    {
        AvailableRoles = new List<AvailableGameRole>();
    }

    public AvailableGame Clone()
    {
        var c = new AvailableGame
        {
            PrimaryGameDiscordRoleId = PrimaryGameDiscordRoleId,
            IncludeInGuildMembersStatistic = IncludeInGuildMembersStatistic,
            IncludeInGamesMenu = IncludeInGamesMenu,
            GameInterestRoleId = GameInterestRoleId
        };

        foreach (var role in AvailableRoles)
        {
            c.AvailableRoles.Add(role.Clone());
        }

        return c;
    }
}