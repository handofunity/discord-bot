using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class User
    {
        public User()
        {
            Game = new HashSet<Game>();
            GameRole = new HashSet<GameRole>();
            Vacation = new HashSet<Vacation>();
        }

        public int UserID { get; set; }
        public decimal DiscordUserID { get; set; }

        public UserInfo UserInfo { get; set; }
        public ICollection<Game> Game { get; set; }
        public ICollection<GameRole> GameRole { get; set; }
        public ICollection<Vacation> Vacation { get; set; }
    }
}
