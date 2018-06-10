namespace HoU.GuildBot.DAL.Database.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserInfo", Schema = "hou")]
    public class UserInfo
    {
        public int UserID { get; set; }
        public DateTime LastSeen { get; set; }

        public User User { get; set; }
    }
}