using System.ComponentModel.DataAnnotations.Schema;

namespace HoU.GuildBot.DAL.Database.Model
{
    using System.Collections.Generic;

    [Table("User", Schema = "hou")]
    public class User
    {
        public int UserID { get; set; }
        public decimal DiscordUserID { get; set; }

        public UserInfo UserInfo { get; set; }
        public List<Vacation> Vacations { get; set; }
    }
}
