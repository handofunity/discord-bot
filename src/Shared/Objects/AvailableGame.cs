using System.Collections.Generic;
using System.Diagnostics;

namespace HoU.GuildBot.Shared.Objects
{
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public class AvailableGame
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }

        public ulong? PrimaryGameDiscordRoleID { get; set; }

        public bool IncludeInGuildMembersStatistic { get; set; }

        public bool IncludeInGamesMenu { get; set; }
        
        public string GameInterestEmojiName { get; set; }
        
        public ulong? GameInterestRoleId { get; set; }

        public List<AvailableGameRole> AvailableRoles { get; }

        public AvailableGame()
        {
            AvailableRoles = new List<AvailableGameRole>();
        }

        public AvailableGame Clone()
        {
            var c = new AvailableGame
            {
                LongName = LongName,
                ShortName = ShortName,
                PrimaryGameDiscordRoleID = PrimaryGameDiscordRoleID,
                IncludeInGuildMembersStatistic = IncludeInGuildMembersStatistic,
                IncludeInGamesMenu = IncludeInGamesMenu,
                GameInterestEmojiName = GameInterestEmojiName,
                GameInterestRoleId = GameInterestRoleId
            };

            foreach (var role in AvailableRoles)
            {
                c.AvailableRoles.Add(role.Clone());
            }

            return c;
        }

        public override string ToString()
        {
            return PrimaryGameDiscordRoleID == null
                       ? $"{LongName} ({ShortName})"
                       : $"{LongName} ({ShortName}) - {{{PrimaryGameDiscordRoleID.Value}}}";
        }
    }
}