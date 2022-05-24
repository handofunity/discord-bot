using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Game
    {
        public Game()
        {
            GameRole = new HashSet<GameRole>();
        }

        public short GameId { get; set; }
        public decimal PrimaryGameDiscordRoleId { get; set; }
        public int ModifiedByUserId { get; set; }
        public DateTime ModifiedAtTimestamp { get; set; }
        public bool IncludeInGuildMembersStatistic { get; set; }
        public bool IncludeInGamesMenu { get; set; }
        public decimal? GameInterestRoleId { get; set; }

        public virtual User? ModifiedByUser { get; set; } = null!;
        public virtual ICollection<GameRole> GameRole { get; set; }
    }
}
